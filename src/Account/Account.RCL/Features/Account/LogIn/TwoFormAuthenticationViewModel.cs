using System.ComponentModel.DataAnnotations;

namespace Account.Features.Account.LogIn
{
    public record TwoFormAuthenticationViewModel
    {
        public string UserName { get; init; } = string.Empty;
        public string? RedirectUrl { get; init; } = string.Empty;
        [Display(Name = "Code")]
        [Required(ErrorMessage = "Must provide code")]
        public string TwoFormCode { get; init; } = string.Empty;
        public bool StayLoggedIn { get; init; }
        [Display(Name = "Remember Device")]
        public bool RememberComputer { get; init; } = false;
        public bool Failure { get; set; } = false;
        public string LoginUrl { get; init; } = string.Empty;
    }
}
