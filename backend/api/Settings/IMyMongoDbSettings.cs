namespace api.Settings;
public interface IMyMongoDbSettings
{
    string? ConnectionString { get; set; }
    string? DatabaseName { get; init; }
}