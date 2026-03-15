import SessionProvider from "./components/SessionProvider";

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
        <SessionProvider session={session}>{children}</SessionProvider>
      </body>
    </html>
  );
}