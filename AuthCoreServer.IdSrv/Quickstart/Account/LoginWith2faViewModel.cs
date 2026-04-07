using System.ComponentModel.DataAnnotations;

namespace AuthCoreServer.IdSrv.Quickstart.Account
{
    public class LoginWith2faViewModel
    {
        [Required]
        [Display(Name = "Código do autenticador")]
        public string TwoFactorCode { get; set; }

        [Display(Name = "Lembrar este dispositivo")]
        public bool RememberMachine { get; set; }

        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}
