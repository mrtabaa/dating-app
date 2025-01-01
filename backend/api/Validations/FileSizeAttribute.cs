namespace api.Validations;

/// <summary>
///     Set a minimum and maximum size for input files.
/// </summary>
/// <param name="minFileSize"></param>
/// <param name="maxFileSize"></param>
public class FileSizeAttribute(long minFileSize, long maxFileSize) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? values, ValidationContext validationContext)
    {
        if (values is not IFormFile file) return ValidationResult.Success;

        if (file.Length < minFileSize) return new ValidationResult($"One or more of the files is below the allowed minimum size of {minFileSize / 1_000} kilobytes.");

        if (file.Length > maxFileSize) return new ValidationResult($"One or more of the files is over the allowed maximum size of {maxFileSize / 1_000_000} megabytes.");

        return ValidationResult.Success;
    }
}