using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.Helpers;

internal static class CatalogValidation
{
    public static void Validate(object dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(dto, new ValidationContext(dto), errors, true))
            throw new ArgumentException(string.Join(" ", errors.Select(error => error.ErrorMessage)));
    }

    public static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static void Money(decimal value)
    {
        if (value < 0 || value > 9_999_999_999_999_999.99m || decimal.Round(value, 2) != value)
            throw new ArgumentException("Số tiền phải không âm, nằm trong giới hạn lưu trữ và có tối đa hai chữ số thập phân.");
    }
}
