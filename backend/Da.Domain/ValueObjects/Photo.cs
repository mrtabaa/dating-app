namespace Da.Domain.ValueObjects;

public class Photo
{
    public string Schema { get; init; } = string.Empty;
    public required string Url165 { get; init; } = string.Empty; // navbar & thumbnail
    public required string Url256 { get; init; } = string.Empty; // card
    public required string UrlEnlarged { get; init; } = string.Empty; // enlarged photo up to ~300kb
    public bool IsMain { get; init; }
}