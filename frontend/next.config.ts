import type { NextConfig } from "next";
import path from "path";

// Phase 4D.1 — browser-facing security headers.
//
// Phase 4D left CSP ownership to the frontend but no policy was actually
// configured on the Next.js surface; these live response headers close that
// gap for every route (HTML documents and static assets alike).
//
// Notes:
// - script-src keeps 'unsafe-inline' because Next.js injects inline bootstrap
//   scripts into every page (hydration payload); removing it requires per-request
//   nonces via middleware. There is NO 'unsafe-eval': production Next.js does
//   not need it (dev mode does, which is out of scope here).
// - style-src needs 'unsafe-inline' because components use inline style props.
// - connect-src includes the API origin from NEXT_PUBLIC_API_URL when the
//   browser talks cross-origin; in the canonical TLS topology the API is
//   reached same-origin through the reverse proxy ('self' already covers it).
// - frame-ancestors 'none' replaces X-Frame-Options for modern browsers.
const apiOrigin = process.env.NEXT_PUBLIC_API_URL;
const connectSrc = ["'self'"];
if (apiOrigin && /^https?:\/\//i.test(apiOrigin)) {
  try {
    connectSrc.push(new URL(apiOrigin).origin);
  } catch {
    // ignore malformed value at build time
  }
}

const csp = [
  "default-src 'self'",
  `script-src 'self' 'unsafe-inline'`,
  "style-src 'self' 'unsafe-inline'",
  "img-src 'self' data: blob:",
  "font-src 'self' data:",
  `connect-src ${connectSrc.join(" ")}`,
  "object-src 'none'",
  "base-uri 'self'",
  "form-action 'self'",
  "frame-ancestors 'none'",
].join("; ");

const securityHeaders = [
  { key: "Content-Security-Policy", value: csp },
  { key: "X-Content-Type-Options", value: "nosniff" },
  { key: "Referrer-Policy", value: "strict-origin-when-cross-origin" },
  { key: "Permissions-Policy", value: "camera=(), microphone=(), geolocation=()" },
];

const nextConfig: NextConfig = {
  /* config options here */
  reactCompiler: true,
  turbopack: {
    root: path.resolve(__dirname),
  },
  // Phase 6e — standalone output for lightweight Docker image.
  // Trims node_modules to only what's needed at runtime.
  output: process.env.BUILD_STANDALONE === "1" ? "standalone" : undefined,
  async headers() {
    return [
      {
        source: "/(.*)",
        headers: securityHeaders,
      },
    ];
  },
};

export default nextConfig;
