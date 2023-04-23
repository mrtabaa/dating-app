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
                Name: userInput.Name,
                Email: userInput.Email.ToLower(),
                Password: userInput.Password,
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
                Photos: userInput.Photos
            );
        }

        public static MemberDto? GenerateMemberDto(AppUser appUser)
        {
            if (!(appUser.Id == null || appUser.Schema == null))
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
                    Photos: GeneratePhotoDtos(appUser.Photos)
                );

            return null;
        }

        #endregion Generator Methods

        #region Helper Functions
        private static IEnumerable<PhotoDto> GeneratePhotoDtos(IEnumerable<Photo> photos)
        {
            List<PhotoDto> photoDtos = new();

            foreach (Photo photo in photos)
            {
                photoDtos.Add(new PhotoDto(
                    Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                    Url: photo.Url,
                    IsMain: photo.IsMain
                ));
            }

            return photoDtos;
        }

        #endregion Helper Functions
    }
}