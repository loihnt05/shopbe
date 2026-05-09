import type { NextConfig } from "next";

const apiBaseUrl =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/+$/, "") ||
  "http://localhost:5072";

const { protocol, hostname, port } = new URL(apiBaseUrl);

const nextConfig: NextConfig = {
  images: {
	remotePatterns: [
	  {
		protocol: protocol.replace(":", "") as "http" | "https",
		hostname,
		port,
		pathname: "/uploads/**",
	  },
	],
  },
};

export default nextConfig;
