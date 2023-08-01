namespace api.DTOs
{
    public static class Mappers
    {
        #region Generator Methods
        public static AppUser GenerateAppUser(UserRegisterDto userInput)
        {
            // manually dispose HMACSHA512 after being done
            using var hmac = new HMACSHA512();

            return new AppUser(
                Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                Id: null,
                Name: userInput.Name.Trim(),
                Email: userInput.Email.ToLower().Trim(),
                Password: userInput.Password,
                PasswordHash: hmac.ComputeHash(Encoding.UTF8.GetBytes(userInput.Password!)),
                PasswordSalt: hmac.Key,
                ConfirmPassword: userInput.ConfirmPassword,
                DateOfBirth: userInput.DateOfBirth,
                KnownAs: userInput.KnownAs.Trim(),
                Created: DateTime.UtcNow,
                LastActive: DateTime.UtcNow,
                Gender: userInput.Gender,
                Introduction: userInput.Introduction.Trim(),
                LookingFor: userInput.LookingFor.Trim(),
                Interests: userInput.Interests.Trim(),
                City: userInput.City.Trim(),
                Country: userInput.Country.Trim(),
                Photos: userInput.Photos
            );
        }

        public static MemberDto? GenerateMemberDto(AppUser appUser)
        {
            if (!(appUser.Id is null || appUser.Schema is null))
                return new MemberDto(
                    Schema: appUser.Schema,
                    Id: appUser.Id,
                    Name: appUser.Name,
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

        #endregion Generator Methods

        #region Helper Functions
        // some Functions
        #endregion Helper Functions
    }
}