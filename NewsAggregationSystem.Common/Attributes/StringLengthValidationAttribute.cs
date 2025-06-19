using NewsAggregationSystem.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace NewsAggregationSystem.Common.Attributes
{
    /// <summary>
    /// Custom validation attribute to ensure the length of a string falls within a specified range and is it Required or not.
    /// The Default values of MinStringLength = 2, MaxStringLength = 50, IsRequired = true.
    /// </summary>
    public class StringLengthValidationAttribute : ValidationAttribute
    {
        private readonly int _MaxLength;
        private readonly int _MinLength;
        private readonly bool _IsRequired;
        private int Length;
        public StringLengthValidationAttribute(int MinLength = ApplicationConstants.MinStringLength, int MaxLength = ApplicationConstants.MaxStringLength, bool IsRequired = true)
        {
            _MaxLength = MaxLength;
            _MinLength = MinLength;
            _IsRequired = IsRequired;
            ErrorMessage = $"The {{0}} field must have a length between {_MinLength} and {_MaxLength}";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (_IsRequired)
            {
                var requiredAttribute = new RequiredAttribute();
                if (!requiredAttribute.IsValid(value))
                {
                    return new ValidationResult($"{validationContext.DisplayName} Field is required.");
                }
            }

            Length = value.ToString().Length;

            if (Length >= _MinLength && Length <= _MaxLength)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
        }
    }
}
