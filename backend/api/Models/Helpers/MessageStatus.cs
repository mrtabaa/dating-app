namespace api.Models.Helpers;

public record MessageStatus(
    [Optional] bool IsSuccess,
    [Optional] bool IsReceiverNotFound
    // [Optional] AppUser SenderAppUser,
    // [Optional] AppUser ReceiverAppUser,
    // [Optional] PagedList<Message> PagedMessages
);
