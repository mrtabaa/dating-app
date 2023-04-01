namespace api.DTOs
{
    public static class Mappers
    {
        public static AppUser AppUser(UserRegisterDto userInput)
        {

            // manually dispose HMACSHA512 after being done
            using var hmac = new HMACSHA512();

            return new AppUser(
                Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                Id: null,
                Name: userInput.Name,
                Email: userInput.Email,
                PasswordHash: hmac.ComputeHash(Encoding.UTF8.GetBytes(userInput.Password!)),
                PasswordSalt: hmac.Key,
                DateOfBirth: userInput.DateOfBirth,
                KnownAs: userInput.KnownAs,
                Created: DateTime.UtcNow,
                LastActive: DateTime.UtcNow,
                Gender: userInput.Gender,
                Introduction: userInput.Introduction,
                LookingFor: userInput.LookingFor,
                Interests: userInput.Interests,
                City: userInput.City,
                Country: userInput.Country,
                Photos: new List<Photo>() { }
            );
        }

        public static MemberDto? MemberDto(AppUser appUser)
        {
            if (appUser.Id != null)
                return new MemberDto(
                    Schema: AppVariablesExtensions.AppVersions.Last<string>(),
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
                    Photos: new List<PhotoDto> { }
                );

            return null;
        }

        public static PhotoDto? PhotoDto(Photo photo)
        {
            if (photo.Id != null)
                return new PhotoDto(
                    Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                    Id: photo.Id,
                    Url: photo.Url,
                    IsMain: photo.IsMain
                );

            return null;
        }
    }
}