namespace StadiumOps.Infrastructure.Integrations;

public sealed class VertexAiOptions
{
    public string? ProjectId { get; set; }
    public string Location { get; set; } = "us-central1";
    public string Model { get; set; } = "gemini-2.0-flash";
}

public sealed class FirebaseOptions
{
    public string? ServiceAccountPath { get; set; }
    public string? ProjectId { get; set; }
}

public sealed class MapsOptions
{
    public string? BrowserApiKey { get; set; }
}
