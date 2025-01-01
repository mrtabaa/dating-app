namespace api.Extensions;

public static class ValidationsExtension
{
    public static bool ValidateObjectId(ObjectId? objectId) => objectId.HasValue && !objectId.Equals(ObjectId.Empty);
}