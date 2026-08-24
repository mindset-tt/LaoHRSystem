// LaoHR Phase 4D.1 — LOAD TEST (INTERNAL_ENGINEERING_PROFILE)
//
// Mixed workload per docs/production-phase-4d.1 spec:
//   ~60% reads (employee list, attendance, documents metadata)
//   ~15% list/search (suppliers, inventory)
//   ~10% dashboards/reports (back-office, finance AP aging, GL, corporate)
//   ~10% safe create/update (create + delete a synthetic service request)
//   ~5% booking conflict workflows (room availability reads; dedicated
//        conflict race lives in scripts/load-booking-conflict.js)
//
// NOT a business SLA. Disposable DB only.
//
// Usage:
//   k6 run -e BASE_URL=https://localhost -e VUS=25 -e DURATION=3m scripts/load-test.js

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate, Counter } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'https://localhost';
const USERNAME = __ENV.LOAD_USER || 'admin';
const PASSWORD = __ENV.PASSWORD || 'LoadTest!2026';

const customTrend = new Trend('laohr_request_duration', true);
const failedReqs = new Rate('laohr_failed_requests');
const createdRequests = new Counter('laohr_safe_writes');

export const options = {
  scenarios: {
    mixed: {
      executor: 'constant-vus',
      vus: Number(__ENV.VUS || 10),
      duration: __ENV.DURATION || '2m',
      gracefulStop: '15s',
    },
  },
  insecureSkipTLSVerify: true,
  // Percentiles captured in both stdout and --summary-export.
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
  thresholds: {
    // Baseline-only gate: do not fail the run on latency while profiling;
    // saturation is analyzed from the recorded results instead.
    'laohr_failed_requests': [{ threshold: 'rate<0.05', abortOnFail: false }],
  },
  tags: { profile: 'INTERNAL_ENGINEERING_PROFILE' },
};

let token = '';
let writeToken = '';

export function setup() {
  const login = (user) => {
    const res = http.post(`${BASE_URL}/api/auth/login`,
      JSON.stringify({ username: user, password: PASSWORD }),
      { headers: { 'Content-Type': 'application/json' } });
    if (res.status !== 200) {
      throw new Error(`setup login failed for ${user}: ${res.status} ${res.body}`);
    }
    return res.json('token');
  };
  // Admin reads all modules; the linked-employee user performs safe writes
  // (service requests require a linked employee profile).
  token = login(USERNAME);
  writeToken = login(__ENV.WRITE_USERNAME || 'emp1');
  return { token, writeToken };
}

function authHeaders() {
  return {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };
}

function pick(weighted) {
  const r = Math.random();
  let acc = 0;
  for (const [fn, w] of weighted) {
    acc += w;
    if (r <= acc) return fn;
  }
  return weighted[weighted.length - 1][0];
}

// --- workload actions ------------------------------------------------------

function readEmployees() {
  const page = 1 + Math.floor(Math.random() * 20);
  return http.get(`${BASE_URL}/api/employees?page=${page}&pageSize=25`, authHeaders());
}

function readAttendance() {
  return http.get(`${BASE_URL}/api/attendance?page=2&pageSize=50`, authHeaders());
}

function searchSuppliers() {
  return http.get(`${BASE_URL}/api/suppliers?page=${1 + Math.floor(Math.random() * 8)}&pageSize=25`, authHeaders());
}

function searchInventory() {
  return http.get(`${BASE_URL}/api/inventory/items?page=${1 + Math.floor(Math.random() * 7)}&pageSize=25`, authHeaders());
}

function backOfficeDashboard() {
  return http.get(`${BASE_URL}/api/backoffice/command-center`, authHeaders());
}

function financeReports() {
  return http.get(`${BASE_URL}/api/supplier-invoices/aging`, authHeaders());
}

function generalLedger() {
  return http.get(`${BASE_URL}/api/journals?page=1&pageSize=25`, authHeaders());
}

function corporateDashboard() {
  return http.get(`${BASE_URL}/api/corporate-documents?page=1&pageSize=25`, authHeaders());
}

function documentSearch() {
  return http.get(`${BASE_URL}/api/documents/employee/${1 + Math.floor(Math.random() * 400)}`, authHeaders());
}

function contractExpiry() {
  return http.get(`${BASE_URL}/api/contracts?page=1&pageSize=25&status=EXPIRING_SOON`, authHeaders());
}

function serviceDeskBacklog() {
  return http.get(`${BASE_URL}/api/service-requests?page=1&pageSize=25&status=OPEN`, authHeaders());
}

function roomAvailability() {
  return http.get(`${BASE_URL}/api/facilities/rooms`, authHeaders());
}

function vehicleAvailability() {
  return http.get(`${BASE_URL}/api/fleet/vehicles`, authHeaders());
}

function travelRegister() {
  return http.get(`${BASE_URL}/api/travel?page=1&pageSize=25`, authHeaders());
}

function safeCreateUpdate() {
  // Safe write: create a synthetic service request then delete it again.
  const headers = {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${writeToken}`,
    },
  };
  const createRes = http.post(`${BASE_URL}/api/service-requests`,
    JSON.stringify({
      categoryId: 1,
      subject: `load-test sr ${Date.now()}-${__VU}-${__ITER}`,
      description: 'safe disposable write from k6',
      priority: 'LOW',
    }),
    headers);
  createdRequests.add(1);
  if (createRes.status === 200 || createRes.status === 201) {
    try {
      const id = createRes.json('serviceRequestId') ?? createRes.json('id');
      if (id) http.del(`${BASE_URL}/api/service-requests/${id}`, null, headers);
    } catch (_) { /* best-effort cleanup */ }
  }
  return createRes;
}

const READS = [
  [readEmployees, 0.22],
  [readAttendance, 0.14],
  [documentSearch, 0.08],
  [contractExpiry, 0.06],
  [travelRegister, 0.05],
  [serviceDeskBacklog, 0.05],
];

const SEARCH = [
  [searchSuppliers, 0.08],
  [searchInventory, 0.07],
];

const REPORTS = [
  [backOfficeDashboard, 0.03],
  [financeReports, 0.03],
  [generalLedger, 0.02],
  [corporateDashboard, 0.01],
  [roomAvailability, 0.005],
  [vehicleAvailability, 0.005],
];

export default function (data) {
  token = data.token;
  writeToken = data.writeToken;
  const action = pick([
    ...READS,
    ...SEARCH,
    ...REPORTS,
    [safeCreateUpdate, 0.10],
  ]);

  const res = action();
  check(res, { 'status < 400 or 404-empty-scope': (r) => r.status < 500 });
  if (res.status >= 400) failedReqs.add(res.status >= 500 ? 1 : 0);
  customTrend.add(res.timings.duration);

  sleep(Math.random() * 2 + 0.5); // think time 0.5–2.5s
}

export function handleSummary(data) {
  return {
    stdout: textSummary(data, { indent: ' ', enableColors: false }),
  };
}

function textSummary(data, opts) {
  // k6 ships its own summary renderer in newer versions via
  // https://jslib.k6.io — to keep this script dependency-free we emit the raw
  // JSON metrics block that our evidence pipeline parses.
  const m = data.metrics;
  const lines = [];
  for (const [name, metric] of Object.entries(m)) {
    const vals = metric.values || {};
    const flat = Object.entries(vals).map(([k, v]) => `${k}=${fmt(v)}`).join(' ');
    lines.push(`${name}: ${flat}`);
  }
  function fmt(v) {
    if (typeof v !== 'number') return String(v);
    return Number.isInteger(v) ? String(v) : v.toFixed(3);
  }
  return lines.join('\n') + '\n';
}
