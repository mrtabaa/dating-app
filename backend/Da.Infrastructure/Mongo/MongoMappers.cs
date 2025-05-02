using Da.Domain.Entities;
using Da.Infrastructure.Mongo.Models;

namespace Da.Infrastructure.Mongo;

internal static class MongoMappers
{
    internal static MongoAppUser MapAppUserToMongoAppUser(AppUser appUser) =>
        new()
        {
            Schema = appUser.Schema,
            IdentifierHash = appUser.IdentifierHash,
            DateOfBirth = appUser.DateOfBirth,
            KnownAs = appUser.KnownAs,
            LastActive = appUser.LastActive,
            Gender = appUser.Gender,
            Introduction = appUser.Introduction,
            LookingFor = appUser.LookingFor,
            Interests = appUser.Interests,
            CountryAcr = appUser.CountryAcr,
            Country = appUser.Country,
            State = appUser.State,
            City = appUser.City,
            Photos = appUser.Photos,
            FollowingsCount = appUser.FollowingsCount,
            FollowersCount = appUser.FollowersCount,
            IsProfileCompleted = appUser.IsProfileCompleted,
            ConnectionsPresence = appUser.ConnectionsPresence,
            MessageGroups = appUser.MessageGroups
        };
}