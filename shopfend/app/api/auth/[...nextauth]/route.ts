import NextAuth from "next-auth";
import KeycloakProvider from "next-auth/providers/keycloak";
import GoogleProvider from "next-auth/providers/google";
import GitHubProvider from "next-auth/providers/github";

const issuer = process.env.KEYCLOAK_ISSUER
  ? process.env.KEYCLOAK_ISSUER
  : `${process.env.KEYCLOAK_URL}/realms/${process.env.KEYCLOAK_REALM}`;

const keycloakClientSecret = process.env.KEYCLOAK_CLIENT_SECRET;

const handler = NextAuth({
  providers: [
    KeycloakProvider({
      clientId: process.env.KEYCLOAK_CLIENT_ID!,
      issuer,
      authorization: {
        params: {
          scope: "openid profile email",
        },
      },
      ...(keycloakClientSecret ? { clientSecret: keycloakClientSecret } : {}),
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any),
    ...(process.env.GOOGLE_CLIENT_ID && process.env.GOOGLE_CLIENT_SECRET
      ? [
          GoogleProvider({
            clientId: process.env.GOOGLE_CLIENT_ID,
            clientSecret: process.env.GOOGLE_CLIENT_SECRET,
          }),
        ]
      : []),
    ...(process.env.GITHUB_CLIENT_ID && process.env.GITHUB_CLIENT_SECRET
      ? [
          GitHubProvider({
            clientId: process.env.GITHUB_CLIENT_ID,
            clientSecret: process.env.GITHUB_CLIENT_SECRET,
          }),
        ]
      : []),
  ],
  callbacks: {
    async jwt({ token, account }) {
      if (account) {
        token.accessToken = account.access_token;
        token.idToken = account.id_token;
      }
      return token;
    },
    async session({ session, token }) {
      session.accessToken = token.accessToken as string | undefined;
      session.idToken = token.idToken as string | undefined;
      return session;
    },
  },
  secret: process.env.NEXTAUTH_SECRET,
});

export { handler as GET, handler as POST };
