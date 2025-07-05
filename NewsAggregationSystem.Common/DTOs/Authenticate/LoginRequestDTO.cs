using NewsAggregationSystem.Common.Attributes;
using NewsAggregationSystem.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace NewsAggregationSystem.Common.DTOs.Authenticate
{
    public class LoginRequestDTO
    {
        [Required]
        [RegularExpression(ApplicationConstants.EmailValidationRegex, ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [StringLengthValidation(MinLength: 8)]
        [RegularExpression(ApplicationConstants.PasswordFormatValidationRegex,
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; }
    }
}
