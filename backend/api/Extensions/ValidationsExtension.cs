namespace api.Extensions;

public static class ValidationsExtension
{
    public static bool ValidateObjectId(ObjectId? objectId)
    {
        return objectId is not null && objectId.HasValue && !objectId.Equals(ObjectId.Empty);
    }
}
