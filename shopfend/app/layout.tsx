import "./globals.css";
import SessionProvider from "./components/SessionProvider";
import AppShell from "./components/AppShell";

export default function RootLayout({
  children,
  session,
}: {
  children: React.ReactNode;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  session?: any;
}) {
  return (
    <html lang="en">
      <body>
        <SessionProvider session={session}>
          <AppShell>{children}</AppShell>
        </SessionProvider>
      </body>
    </html>
  );
}