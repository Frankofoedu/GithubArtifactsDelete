
// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

var configuration = new ConfigurationBuilder()
     .SetBasePath(Directory.GetCurrentDirectory())
     .AddJsonFile($"appsettings.json");

var config = configuration.Build();
var appsettings = config.Get<AppSettings>();

using var client = new HttpClient();

client.DefaultRequestHeaders.Add("Authorization", $"{appsettings.Token}");
client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
client.DefaultRequestHeaders.Add("User-Agent", $"{appsettings.UserName}");

using var getArtificatsMessage = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{appsettings.Owner}/{appsettings.Repo}/actions/artifacts");

var getArtificatsresponse = await client.SendAsync(getArtificatsMessage);
getArtificatsresponse.EnsureSuccessStatusCode();
var resp = await getArtificatsresponse.Content.ReadAsStringAsync();

var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};
var artifacts = JsonSerializer.Deserialize<Rootobject>(resp, options);
if (artifacts is null || artifacts.Artifacts is null || artifacts.Total_count < 1)
{
    Console.WriteLine("No artifacts found");
    Console.WriteLine("Press any key to exit");
    Console.ReadLine();

    return;
}
Console.WriteLine($"{artifacts.Total_count} artifacts found!");

Console.WriteLine("Start deleting process!");
foreach (var artifact in artifacts.Artifacts)
{
    using var deleteArtificatsMessage = new HttpRequestMessage(HttpMethod.Delete, $"https://api.github.com/repos/{appsettings.Owner}/{appsettings.Repo}/actions/artifacts/{artifact.Id}");
    var deleteResponseMessage = await client.SendAsync(deleteArtificatsMessage);
    deleteResponseMessage.EnsureSuccessStatusCode();
    Console.WriteLine($"Deleted artifact with Id: {artifact.Id}");
    Console.WriteLine();
}

Console.WriteLine("Done deleting artifacts");

public class AppSettings
{
    public string Token { get; set; }
    public string Owner { get; set; }
    public string Repo { get; set; }
    public string UserName { get; set; }
};
public record ArtifactsVM(int Total_count, Artifact[]? Artifacts);
public record Artifact(int Id);



public class Rootobject
{
    public int Total_count { get; set; }
    public MyArtifact[] Artifacts { get; set; }
}

public class MyArtifact
{
    public int Id { get; set; }
}

