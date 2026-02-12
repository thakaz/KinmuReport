using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

namespace KinmuReport.Services;

public class SharePointService
{
    private readonly GraphServiceClient _client;
    private readonly ILogger<SharePointService> _logger;
    private readonly string _siteUrl;
    private string? _driveId;

    public SharePointService(IConfiguration config, ILogger<SharePointService> logger)
    {
        _logger = logger;
        var tenantId = config["SharePoint:TenantId"]!;
        var clientId = config["SharePoint:ClientId"]!;
        var clientSecret = config["SharePoint:ClientSecret"]!;
        _siteUrl = config["SharePoint:SiteUrl"]!;

        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        _client = new GraphServiceClient(credential);
    }

    private async Task<string> GetDriveIdAsync()
    {
        if (_driveId != null) return _driveId;

        var uri = new Uri(_siteUrl);
        var site = await _client.Sites[$"{uri.Host}:{uri.AbsolutePath}"].GetAsync();
        var drive = await _client.Sites[site!.Id].Drive.GetAsync();
        _driveId = drive!.Id;
        return _driveId;
    }

    public async Task<int> UploadAsync(string folderPath, string fileName, Stream content)
    {
        var driveId = await GetDriveIdAsync();
        var path = $"{folderPath}/{fileName}".TrimStart('/');

        var item = await _client.Drives[driveId]
            .Root
            .ItemWithPath(path)
            .Content
            .PutAsync(content);

        var versions = await _client.Drives[driveId]
            .Items[item!.Id]
            .Versions
            .GetAsync();

        return versions?.Value?.Count ?? 1;
    }

    public async Task<Stream?> DownloadAsync(string folderPath, string fileName)
    {
        var driveId = await GetDriveIdAsync();
        var path = $"{folderPath}/{fileName}".TrimStart('/');

        return await _client.Drives[driveId]
            .Root
            .ItemWithPath(path)
            .Content
            .GetAsync();
    }

    public async Task<int?> GetVersionCountAsync(string folderPath, string fileName)
    {
        var driveId = await GetDriveIdAsync();
        var path = $"{folderPath}/{fileName}".TrimStart('/');

        try
        {
            var item = await _client.Drives[driveId]
                .Root
                .ItemWithPath(path)
                .GetAsync();

            if (item == null) return null;

            var versions = await _client.Drives[driveId]
                .Items[item.Id]
                .Versions
                .GetAsync();

            return versions?.Value?.Count;
        }
        catch (ODataError ex) when (ex.ResponseStatusCode == 404)
        {
            // ファイルが存在しない場合はnull
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SharePoint版数取得エラー: {Path}", path);
            throw;
        }
    }
}