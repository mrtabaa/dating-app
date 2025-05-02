namespace Da.Infrastructure.Mongo.Settings;

public interface IMyMongoDbSettings
{
    string? ConnectionString { get; init; }
    string? DatabaseName { get; init; }
}