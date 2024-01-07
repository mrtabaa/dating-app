namespace api.Extensions.Validations;

/// <summary>
/// Set a minimum and maximum size for input files.
/// </summary>
/// <param name="minFileSize"></param>
/// <param name="maxFileSize"></param>
public class FileSizeAttribute(long minFileSize, long maxFileSize) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? values, ValidationContext validationContext)
    {
        var files = values as IEnumerable<IFormFile>;

        if (files is not null && files.Any())
            foreach (var file in files)
            {
                if (file is not null && file.Length > maxFileSize)
                {
                    return new ValidationResult($"One or more of the files is below the allowed minimum size of {minFileSize / 1_000} kilobytes.");
                }
                else if (file is not null && file.Length < minFileSize)
                {
                    return new ValidationResult($"One or more of the files is over the allowed maximum size of {maxFileSize / 1_000_000} megabytes.");
                }
            }

        return ValidationResult.Success;
    }
}