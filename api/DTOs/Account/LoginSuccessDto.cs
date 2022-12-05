namespace api.DTOs;
public class LoginSuccessDto {
    public string? Token { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public bool BadEmailPattern { get; set; }
}
