namespace api.Extensions.Validations;

public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly long _maxFileSize;
    public MaxFileSizeAttribute(long maxFileSize)
    {
        _maxFileSize = maxFileSize;
    }

    protected override ValidationResult? IsValid(object? values, ValidationContext validationContext)
    {
        var files = values as IEnumerable<IFormFile>;

        if (files is not null && files.Any())
            foreach (var file in files)
            {
                if (file is not null && file.Length > _maxFileSize)
                {
                    return new ValidationResult($"One or more of the files is over the maximum size of {_maxFileSize / 1_000_000} megabytes.");
                }
            }

        return ValidationResult.Success;
    }
}