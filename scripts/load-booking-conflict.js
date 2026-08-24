// LaoHR Phase 4D.1 — BOOKING CONFLICT RACE (room + vehicle)
//
// N VUs simultaneously attempt to book the SAME room / vehicle for the SAME
// time window (window chosen once in setup() so every racer collides).
// Invariant under load: exactly ONE booking succeeds (201) per resource,
// every other attempt gets 409 Conflict. Verified again via SQL afterwards.
//
// INTERNAL_ENGINEERING_PROFILE — disposable DB only.
//
// Usage: k6 run -e BASE_URL=https://localhost -e RACERS=25 scripts/load-booking-conflict.js

import http from 'k6/http';
import { sleep } from 'k6';
import { Counter } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'https://localhost';
const PASSWORD = __ENV.PASSWORD || 'LoadTest!2026';
const ROOM_ID = Number(__ENV.ROOM_ID || 1);
const VEHICLE_ID = Number(__ENV.VEHICLE_ID || 1);

const created = new Counter('booking_created_201');
const conflicted = new Counter('booking_conflicted_409');
const otherStatus = new Counter('booking_other_status');

export const options = {
  scenarios: {
    race: {
      executor: 'shared-iterations',
      vus: Number(__ENV.RACERS || 25),
      iterations: Number(__ENV.RACERS || 25),
      maxDuration: '120s',
    },
  },
  insecureSkipTLSVerify: true,
};

export function setup() {
  const login = http.post(`${BASE_URL}/api/auth/login`,
    JSON.stringify({ username: __ENV.LOAD_USER || 'emp1', password: PASSWORD }),
    { headers: { 'Content-Type': 'application/json' } });
  if (login.status !== 200) throw new Error(`setup login failed: ${login.status}`);

  // ONE shared future window for the whole race — every racer collides here.
  const base = Date.now() + 3600_000 * 24 * 365; // far-future slot, unique per run
  return {
    token: login.json('token'),
    startAt: new Date(base).toISOString(),
    endAt: new Date(base + 3600_000).toISOString(),
  };
}

export default function (data) {
  const headers = {
    headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${data.token}` },
  };

  const roomRes = http.post(`${BASE_URL}/api/facilities/bookings`,
    JSON.stringify({
      roomId: ROOM_ID,
      startAt: data.startAt,
      endAt: data.endAt,
      title: `race ${__VU}`,
      participantCount: 2,
    }), headers);
  tally(roomRes);

  const vRes = http.post(`${BASE_URL}/api/fleet/bookings`,
    JSON.stringify({
      vehicleId: VEHICLE_ID,
      requesterEmployeeId: 1,
      startAt: data.startAt,
      endAt: data.endAt,
      purpose: `race ${__VU}`,
    }), headers);
  tally(vRes);

  sleep(0.2);
}

function tally(res) {
  if (res.status === 201) created.add(1);
  else if (res.status === 409) conflicted.add(1);
  else otherStatus.add(res.status === 0 ? -1 : res.status);
}

export function handleSummary(data) {
  const cnt = (name) => {
    const m = data.metrics[name];
    if (!m) return 0;
    return m.values ? (m.values.count || 0) : 0;
  };
  const c = cnt('booking_created_201');
  const x = cnt('booking_conflicted_409');
  const o = cnt('booking_other_status');

  // Room + vehicle races: exactly one winner per resource.
  const pass = o === 0 && c === 2 && x >= 2;
  return {
    stdout: `
=== BOOKING CONFLICT RACE SUMMARY ===
created (201):     ${c}   (expected 2: one room + one vehicle)
conflicted (409):  ${x}
unexpected status: ${o}
INVARIANT: ${pass ? 'PASS' : 'FAIL'}
`,
  };
}
