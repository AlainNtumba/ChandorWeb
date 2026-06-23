using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ChandorProject.Shared.Validation;

/// <summary>
/// Culture-invariant decimal range validation. Avoids <see cref="RangeAttribute"/> string bound parsing failures
/// on locales that use a comma decimal separator (e.g. fr-FR).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class DecimalRangeAttribute : ValidationAttribute
{
    public const string MinimumAmount = "0.01";
    public const string MaximumAmount = "79228162514264337593543950335";

    private readonly decimal _minimum;
    private readonly decimal _maximum;

    public DecimalRangeAttribute()
        : this(MinimumAmount, MaximumAmount)
    {
    }

    public DecimalRangeAttribute(string minimum, string maximum)
    {
        _minimum = decimal.Parse(minimum, NumberStyles.Number, CultureInfo.InvariantCulture);
        _maximum = decimal.Parse(maximum, NumberStyles.Number, CultureInfo.InvariantCulture);
        ErrorMessage = "The field value is out of range.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (!TryConvertToDecimal(value, out var amount))
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

        if (amount < _minimum || amount > _maximum)
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

        return ValidationResult.Success;
    }

    private static bool TryConvertToDecimal(object value, out decimal amount)
    {
        switch (value)
        {
            case decimal d:
                amount = d;
                return true;
            case int i:
                amount = i;
                return true;
            case long l:
                amount = l;
                return true;
            case double dbl:
                amount = Convert.ToDecimal(dbl, CultureInfo.InvariantCulture);
                return true;
            case float f:
                amount = Convert.ToDecimal(f, CultureInfo.InvariantCulture);
                return true;
            default:
                return decimal.TryParse(
                    Convert.ToString(value, CultureInfo.InvariantCulture),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out amount);
        }
    }
}
