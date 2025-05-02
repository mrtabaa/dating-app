namespace da.Application.DTOs.Account;

public record LoginResult(
    LoggedInDto LoggedInDto,
    [Optional] TokenDto TokenDto
);