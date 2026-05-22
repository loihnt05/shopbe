import type { NextConfig } from "next";

import { dirname } from "path";
import { fileURLToPath } from "url";

const apiBaseUrl =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/+$/, "") ||
  "http://localhost:5072";

const { protocol, hostname, port } = new URL(apiBaseUrl);

// Ensure Turbopack resolves modules from this Next.js app's directory.
// When running from the monorepo root, `process.cwd()` may point outside `shopfend`,
// which breaks CSS imports like `@import "tailwindcss";`.
const appRoot = dirname(fileURLToPath(import.meta.url));

const nextConfig: NextConfig = {
  turbopack: {
    root: appRoot,
  },
  webpack: (config) => {
    // Ensure Webpack also resolves from the app root
    config.resolve.modules.push(appRoot);
    return config;
  },
  images: {
    remotePatterns: [
      {
        protocol: protocol.replace(":", "") as "http" | "https",
        hostname,
        port,
        pathname: "/uploads/**",
      },
      {
        protocol: "https",
        hostname: "dummyjson.com",
      },
      {
        protocol: "https",
        hostname: "cdn.dummyjson.com",
      },
    ],
  },
};

export default nextConfig;
