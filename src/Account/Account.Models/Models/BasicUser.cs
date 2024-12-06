using FluentValidation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Account.Models
{
    public class BasicUser
    {
        [Required]
        [DisplayName("User Name")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [DisplayName("Email")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Converts a basic user to a user object
        /// </summary>
        /// <returns></returns>
        public TGenericUser GetUser<TGenericUser>() where TGenericUser : User, new()
        {
            return new TGenericUser() {
                UserName = UserName,
                FirstName = FirstName,
                LastName = LastName,
                Email = UserEmail,
                Enabled = false,
                IsExternal = false,
                IsPublic = false
            };
        }
    }

    // TODO: Test if this works...
    public class BasicUserValidator<TGenericUser> : AbstractValidator<BasicUser> where TGenericUser : User, new()
    {
        public BasicUserValidator(IUserRepository<TGenericUser> _userRepository)
        {
            RuleFor(model => model.UserEmail)
                .EmailAddress()
                .WithMessage("Invalid Email Address")
                .MustAsync(async (userEmail, thread) => (await _userRepository.GetUserByEmailAsync(userEmail)).IsFailure)
                .WithMessage("User already exists with this email address.");
            RuleFor(model => model.UserName)
                .MustAsync(async (userName, thread) => (await _userRepository.GetUserAsync(userName)).IsFailure)
                .WithMessage("User already exists with this username.");
        }
    }
}