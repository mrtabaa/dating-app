namespace api.Settings;
public class MyMongoDbSettings : IMyMongoDbSettings
{
    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }
}