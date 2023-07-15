using System.Net.Http.Json;

namespace KubeUI.Core.Services;

public class Updater : IDisposable
{
    private HttpClient HttpClient;

    private GithubRelease[] githubRelease;

    private DateTime DateTime;

    public Updater(IHttpClientFactory clientFactory)
    {
        HttpClient = clientFactory.CreateClient();
    }

    public async Task<bool> UpdateRequired()
    {
        var version = Utilities.GetVersion();

        var releases = await GetReleases();

        return releases.FirstOrDefault()?.tag_name.TrimStart('v') != version;
    }

    public async Task<GithubRelease[]> GetReleases(bool showPrerelease = true)
    {
        if (githubRelease == null || DateTime < DateTime.UtcNow.AddHours(-1))
        {
            DateTime = DateTime.UtcNow;
            HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("KubeUI");
            githubRelease = await HttpClient.GetFromJsonAsync<GithubRelease[]>("https://api.github.com/repos/IvanJosipovic/KubeUI/releases");
        }

        if (showPrerelease)
        {
            return githubRelease.Where(x => !x.draft).ToArray();
        }

        return githubRelease.Where(x => !x.prerelease && !x.draft).ToArray();
    }

    public void Dispose()
    {
        HttpClient?.Dispose();
    }

    public class GithubRelease
    {
        public string url { get; set; }
        public string html_url { get; set; }
        public string assets_url { get; set; }
        public string upload_url { get; set; }
        public string tarball_url { get; set; }
        public string zipball_url { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string tag_name { get; set; }
        public string target_commitish { get; set; }
        public string name { get; set; }
        public string body { get; set; }
        public bool draft { get; set; }
        public bool prerelease { get; set; }
        public DateTime created_at { get; set; }
        public DateTime published_at { get; set; }
        public Asset[] assets { get; set; }

        public class Asset
        {
            public string url { get; set; }
            public string browser_download_url { get; set; }
            public int id { get; set; }
            public string node_id { get; set; }
            public string name { get; set; }
            public string label { get; set; }
            public string state { get; set; }
            public string content_type { get; set; }
            public int size { get; set; }
            public int download_count { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
        }
    }
}