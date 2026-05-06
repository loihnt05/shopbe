import type { NextConfig } from "next";

// Next.js may infer the wrong workspace root when multiple lockfiles exist.
// Setting Turbopack's root explicitly prevents warnings and inconsistent
// resolution. We derive the root from this config file location (works even
// when Next/Turbopack changes the process cwd).

const projectRoot = decodeURIComponent(new URL(".", import.meta.url).pathname);

const nextConfig: NextConfig = {
  turbopack: {
    root: projectRoot,
  },
};

export default nextConfig;
