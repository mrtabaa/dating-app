namespace api.Models;

public class Photo
{
    public string Schema { get; set; } = string.Empty;
    required public string Url_165 { get; set; } = string.Empty; // navbar & thumbnail
    required public string Url_256 { get; set; } = string.Empty; // card
    required public string Url_enlarged { get; set; } = string.Empty; // enlarged photo up to ~300kb
    public bool IsMain { get; set; }
}