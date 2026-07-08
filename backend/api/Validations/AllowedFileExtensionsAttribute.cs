using System.ComponentModel.DataAnnotations;

namespace api.Validations;

public class AllowedFileExtensionsAttribute : ValidationAttribute
{
    private static readonly Dictionary<string, List<byte[]>> _fileSignatures = new()
    {
        {
            ".jpeg", new List<byte[]>
            {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xEE },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xDB },
            }
        },
        {
            ".jpg", new List<byte[]>
            {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xEE },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xDB },
            }
        },
        {
            ".png", new List<byte[]>
            {
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
            }
        },
        {
            ".webp", new List<byte[]>
            {
                // WebP is handled separately because bytes 4-7 are variable.
                // Actual pattern: RIFF ???? WEBP
                new byte[] { 0x52, 0x49, 0x46, 0x46 }
            }
        }
    };

    private static readonly int _maxSignatureLength = Math.Max(
        _fileSignatures.Max(m => m.Value.Max(n => n.Length)),
        16
    );

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            if (!IsFileValid(file))
                return GetValidationResult();
        }
        else if (value is IEnumerable<IFormFile> files)
        {
            foreach (var fileItem in files)
            {
                if (!IsFileValid(fileItem))
                    return GetValidationResult();
            }
        }

        return ValidationResult.Success;
    }

    private ValidationResult GetValidationResult()
    {
        var allowedExtensions = string.Join(", ", _fileSignatures.Keys);

        return new ValidationResult($"File type is not allowed. These extensions are allowed only: {allowedExtensions}");
    }

    public static bool IsFileValid(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!_fileSignatures.ContainsKey(extension))
            return false;

        using var reader = new BinaryReader(file.OpenReadStream());

        var headerBytes = reader.ReadBytes(_maxSignatureLength);

        if (extension == ".webp")
            return IsWebp(headerBytes);

        var signatures = _fileSignatures[extension];

        return signatures.Any(signature =>
            headerBytes.Take(signature.Length).SequenceEqual(signature)
        );
    }

    private static bool IsWebp(byte[] headerBytes)
    {
        if (headerBytes.Length < 12)
            return false;

        return headerBytes[0] == 0x52 && // R
               headerBytes[1] == 0x49 && // I
               headerBytes[2] == 0x46 && // F
               headerBytes[3] == 0x46 && // F

               // Bytes 4-7 are file size, so we skip them.

               headerBytes[8] == 0x57 && // W
               headerBytes[9] == 0x45 && // E
               headerBytes[10] == 0x42 && // B
               headerBytes[11] == 0x50;  // P
    }
}