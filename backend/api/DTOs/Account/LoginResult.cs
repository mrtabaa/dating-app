namespace api.DTOs.Account;

public record LoginResult(
    LoggedInDto LoggedInDto,
    [Optional] TokenDto TokenDto
);