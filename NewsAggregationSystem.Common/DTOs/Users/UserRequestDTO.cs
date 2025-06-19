using NewsAggregationSystem.Common.Attributes;
using NewsAggregationSystem.Common.Constants;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewsAggregationSystem.Common.DTOs.Users
{
    [DefaultValue("")]
    public class UserRequestDTO
    {
        [StringLengthValidation]
        public string FirstName { get; set; } = string.Empty;

        [StringLengthValidation]
        public string LastName { get; set; } = string.Empty;

        [StringLengthValidation(MinLength: 5)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [RegularExpression(ApplicationConstants.EmailValidationRegex, ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [StringLengthValidation(MinLength: 8)]
        [RegularExpression(ApplicationConstants.PasswordFormatValidationRegex,
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; } = string.Empty;
    }
}
