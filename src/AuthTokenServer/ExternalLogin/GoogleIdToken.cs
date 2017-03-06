namespace AuthTokenServer.ExternalLogin
{
    public class GoogleIdToken
    {
        public string Iss { get; set; }
        public string Iat { get; set; }
        public string Exp { get; set; }
        public string AtHash { get; set; }
        public string Aud { get; set; }
        public string Sub { get; set; }
        public string EmailVerified { get; set; }
        public string Azp { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string Locale { get; set; }
        public string Alg { get; set; }
        public string Kid { get; set; }
    }
}