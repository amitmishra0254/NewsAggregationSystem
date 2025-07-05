using NewsAggregationSystem.Common.Attributes;
using NewsAggregationSystem.Common.Constants;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewsAggregationSystem.Common.DTOs.Users
{
    [DefaultValue("")]
    public class UserRequestDTO
    {
        [DefaultValue("")]
        [StringLengthValidation]
        public string FirstName { get; set; } = string.Empty;

        [DefaultValue("")]
        [StringLengthValidation]
        public string LastName { get; set; } = string.Empty;

        [DefaultValue("username")]
        [StringLengthValidation(MinLength: 5)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [DefaultValue("username@example.com")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [RegularExpression(ApplicationConstants.EmailValidationRegex, ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [DefaultValue("password")]
        [StringLengthValidation(MinLength: 8)]
        [RegularExpression(ApplicationConstants.PasswordFormatValidationRegex,
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; } = string.Empty;
    }
}
