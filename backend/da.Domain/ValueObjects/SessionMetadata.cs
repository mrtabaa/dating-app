namespace da.Domain.ValueObjects;

public record SessionMetadata(
    [Length(1, 64)] string DeviceType,
    [Length(1, 128)] string DeviceName,
    [Length(1, 512)] string UserAgent,
    [Length(1, 64)] string IpAddress,
    [Length(1, 64)] string Location
);