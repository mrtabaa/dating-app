namespace api.Extensions;

public static class ValidationsExtension
{
    public static OperationResult<bool> ValidateObjectId(ObjectId? objectId) =>
        new(
            objectId.HasValue && !objectId.Equals(ObjectId.Empty)
        );
}