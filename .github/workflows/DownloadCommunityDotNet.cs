#!
#:property PublishAot=false

using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

const int maxAttempts = 6;
const string apiUrl = "https://api.github.com";

var repository = args[0];
var architecture = args[1];
var output = Path.GetFullPath(args[2]);

Directory.CreateDirectory(Path.GetDirectoryName(output)!);

using var client = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = true }) { Timeout = Timeout.InfiniteTimeSpan };
client.DefaultRequestHeaders.UserAgent.ParseAdd("LinuxCore-CI");

JsonSerializerOptions jsonOptions = new() { RespectNullableAnnotations = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

List<GitHubRelease> releases = [];
for (var page = 1; ; ++page)
{
    var pageReleases = await RetryAsync(async cancellationToken =>
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{apiUrl}/repos/{repository}/releases?per_page=100&page={page}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GitHubRelease[]>(jsonOptions, cancellationToken) ?? throw new JsonException("Expected an array of GitHub releases");
    });

    releases.AddRange(pageReleases);
    if (pageReleases.Length < 100)
        break;
}

Regex assetName = new($@"^dotnet-sdk-(?<version>10\.0\.(?<patch>0|[1-9][0-9]*))-linux-{Regex.Escape(architecture)}\.tar\.gz$", RegexOptions.CultureInvariant);
Candidate[] candidates = [.. releases.Where(release => !release.Draft)
                                     .SelectMany(release => release.Assets)
                                     .Where(asset => asset.State == "uploaded" && asset.Id > 0 && asset.Size > 0)
                                     .Select(asset => (Asset: asset, Match: assetName.Match(asset.Name)))
                                     .Where(item => item.Match.Success && int.TryParse(item.Match.Groups["patch"].Value, out _))
                                     .Select(item => new Candidate(item.Match.Groups["version"].Value,
                                                                   int.Parse(item.Match.Groups["patch"].Value, CultureInfo.InvariantCulture),
                                                                   item.Asset.Name,
                                                                   item.Asset.Url,
                                                                   item.Asset.Id,
                                                                   item.Asset.Size))
                                     .Distinct()];

var latestPatch = candidates.Length == 0
    ? throw new InvalidOperationException("No stable .NET 10 SDK asset found")
    : candidates.Max(candidate => candidate.Patch);
Candidate[] latest = [.. candidates.Where(candidate => candidate.Patch == latestPatch)];
if (latest.Length != 1)
    throw new InvalidOperationException("Multiple assets found for the latest .NET 10 SDK");

var sdk = latest[0];
var expectedName = $"dotnet-sdk-{sdk.Version}-linux-{architecture}.tar.gz";
var expectedAssetUrl = $"{apiUrl}/repos/{repository}/releases/assets/{sdk.Id}";
if (sdk.Name != expectedName || sdk.Url != expectedAssetUrl)
    throw new InvalidOperationException($"Invalid SDK release metadata for {repository} {sdk.Version}");

Console.WriteLine($"Downloading .NET SDK {sdk.Version} for {architecture} from {repository}");

await RetryAsync(async cancellationToken =>
{
    using HttpRequestMessage request = new(HttpMethod.Get, sdk.Url);
    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

    using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    response.EnsureSuccessStatusCode();
    await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
    await using FileStream destination = new(Path.GetTempFileName(), FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.DeleteOnClose);
    await source.CopyToAsync(destination, cancellationToken);
    await destination.FlushAsync(cancellationToken);
    if (destination.Length != sdk.Size)
        throw new IOException("Downloaded SDK archive does not match release metadata");
    File.Copy(destination.Name, output, true);
    return true;
});

return;

static async Task<T> RetryAsync<T>(Func<CancellationToken, Task<T>> action)
{
    Exception? lastException = null;
    for (var attempt = 0; attempt < maxAttempts; ++attempt)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromMinutes(5));
        try
        {
            return await action(timeout.Token);
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException or JsonException ||
                                          exception is OperationCanceledException && timeout.IsCancellationRequested)
        {
            lastException = exception;
            if (attempt + 1 < maxAttempts)
                await Task.Delay(TimeSpan.FromSeconds(attempt + 1));
        }
    }

    throw lastException!;
}

internal sealed record Candidate(string Version, int Patch, string Name, string Url, long Id, long Size);

internal sealed class GitHubRelease
{
    public required bool Draft { get; init; }
    public required GitHubAsset[] Assets { get; init; }
}

internal sealed class GitHubAsset
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required string State { get; init; }
    public required string Url { get; init; }
    public required long Size { get; init; }
}
