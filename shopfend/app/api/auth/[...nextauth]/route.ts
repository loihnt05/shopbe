import NextAuth from "next-auth";
import KeycloakProvider from "next-auth/providers/keycloak";

const issuer = process.env.KEYCLOAK_ISSUER
  ? process.env.KEYCLOAK_ISSUER
  : `${process.env.KEYCLOAK_URL}/realms/${process.env.KEYCLOAK_REALM}`;

const handler = NextAuth({
  providers: [
    KeycloakProvider({
      clientId: process.env.KEYCLOAK_CLIENT_ID!,
      clientSecret: process.env.KEYCLOAK_CLIENT_SECRET!,
      issuer,
      authorization: {
        params: {
          scope: "openid profile email",
        },
      },
    }),
  ],
  callbacks: {
    async jwt({ token, account }) {
      if (account) token.accessToken = account.access_token;
      return token;
    },
    async session({ session, token }) {
      session.accessToken = token.accessToken;
      return session;
    },
  },
  secret: process.env.NEXTAUTH_SECRET,
});

export { handler as GET, handler as POST };