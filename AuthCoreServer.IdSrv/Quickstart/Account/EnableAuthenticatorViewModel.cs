namespace AuthCoreServer.IdSrv.Quickstart.Account
{
    public class EnableAuthenticatorViewModel
    {
        public string SharedKey { get; set; }
        public string AuthenticatorUri { get; set; }
        public string Code { get; set; }

        public string QrCodeImageSource { get; set; }

        public string ReturnUrl { get; set; }
    }
}
