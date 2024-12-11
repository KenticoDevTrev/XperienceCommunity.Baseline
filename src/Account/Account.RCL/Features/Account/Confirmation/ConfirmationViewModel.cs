using Microsoft.AspNetCore.Identity;

namespace Account.Features.Account.Confirmation
{
    public record ConfirmationViewModel
    {
        public ConfirmationViewModel(IdentityResult result, bool isEditMode)
        {
            Result = result;
            IsEditMode = isEditMode;
        }

        public Maybe<string> LoginUrlMaybe { get; init; }
        public IdentityResult Result { get; init; }
        public bool IsEditMode { get; init; }
    }
}
