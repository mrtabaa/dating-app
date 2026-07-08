using System.ComponentModel.DataAnnotations;

namespace api.Validations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class FileSizeAttribute(long minFileSize, long maxFileSize) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (minFileSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minFileSize), "Minimum file size cannot be negative.");
        }

        if (maxFileSize < minFileSize)
        {
            throw new ArgumentOutOfRangeException(nameof(maxFileSize), "Maximum file size cannot be less than minimum file size.");
        }

        IEnumerable<IFormFile>? files = value switch
        {
            IFormFile singleFile => [singleFile],
            IEnumerable<IFormFile> multipleFiles => multipleFiles,
            _ => null
        };

        if (files is null)
        {
            return ValidationResult.Success;
        }

        foreach (var file in files)
        {
            if (file is null)
            {
                continue;
            }

            var fileName = Path.GetFileName(file.FileName);

            if (file.Length < minFileSize)
            {
                return new ValidationResult(
                    $"File '{fileName}' is {ToKb(file.Length)} KB, which is below the allowed minimum size of {ToKb(minFileSize)} KB.",
                    GetMemberNames(validationContext));
            }

            if (file.Length > maxFileSize)
            {
                return new ValidationResult(
                    $"File '{fileName}' is {ToKb(file.Length)} KB, which exceeds the allowed maximum size of {ToKb(maxFileSize)} KB.",
                    GetMemberNames(validationContext));
            }
        }

        return ValidationResult.Success;
    }

    private static string ToKb(long bytes)
    {
        return (bytes / 1024.0).ToString("F2");
    }

    private static IEnumerable<string> GetMemberNames(ValidationContext context)
    {
        return string.IsNullOrWhiteSpace(context.MemberName)
            ? []
            : [context.MemberName];
    }
}