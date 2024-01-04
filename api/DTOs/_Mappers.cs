namespace api.DTOs
{
    public static class Mappers
    {
        #region Generator Methods
        public static AppUser ConvertUserRegisterDtoToAppUser(UserRegisterDto userInput)
        {
            // manually dispose HMACSHA512 after being done
            using var hmac = new HMACSHA512();

            return new AppUser(
                Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                Id: null,
                Email: userInput.Email.ToLower().Trim(),
                PasswordHash: hmac.ComputeHash(Encoding.UTF8.GetBytes(userInput.Password)),
                PasswordSalt: hmac.Key,
                DateOfBirth: userInput.DateOfBirth,
                KnownAs: userInput.KnownAs.Trim(),
                Created: DateTime.UtcNow,
                LastActive: DateTime.UtcNow,
                Gender: userInput.Gender.ToLower(),
                Introduction: userInput.Introduction?.Trim(),
                LookingFor: userInput.LookingFor?.Trim(),
                Interests: userInput.Interests?.Trim(),
                City: userInput.City.Trim(),
                Country: userInput.Country.Trim(),
                Photos: []
            );
        }

        public static MemberDto? ConvertAppUserToMemberDto(AppUser appUser)
        {
            if (!(appUser.Id is null || appUser.Schema is null))
                return new MemberDto(
                    Schema: appUser.Schema,
                    Id: appUser.Id,
                    Email: appUser.Email,
                    Age: DateTimeExtenstions.CalculateAge(appUser.DateOfBirth),
                    KnownAs: appUser.KnownAs,
                    Created: appUser.Created,
                    LastActive: DateTime.UtcNow,
                    Gender: appUser.Gender,
                    Introduction: appUser.Introduction,
                    LookingFor: appUser.LookingFor,
                    Interests: appUser.Interests,
                    City: appUser.City,
                    Country: appUser.Country,
                    Photos: appUser.Photos
                );

            return null;
        }

        public static LoggedInDto? ConvertAppUserToLoggedInDto(AppUser appUser, string token)
        {
            if (!(appUser.Id is null || appUser.Schema is null))
            {
                return new LoggedInDto(
                    Id: appUser.Id,
                    Token: token,
                    KnownAs: appUser.KnownAs,
                    Email: appUser.Email,
                    ProfilePhotoUrl: appUser.Photos.FirstOrDefault(photo => photo.IsMain)?.Url_128
                );
            }

            return null;
        }

        #endregion Generator Methods

        #region Helper Functions
        // some Functions
        #endregion Helper Functions
    }
}