﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace KubeUI.Services
{
    public class Updater
    {
        private HttpClient HttpClient;

        private GithubRelease githubRelease;

        private DateTime DateTime;

        public Updater()
        {
            HttpClient = new HttpClient();
        }

        public async Task<bool> UpdateRequired()
        {
            var version = Utilities.GetVersion();

            var release = await GetRelease();

            return release?.tag_name.TrimStart('v') != version;
        }

        public async Task<GithubRelease> GetRelease()
        {
            if (githubRelease == null || DateTime < DateTime.UtcNow.AddHours(-1))
            {
                DateTime = DateTime.UtcNow;
                HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.122 Safari/537.36 Edg/80.0.361.62");
                githubRelease = await HttpClient.GetFromJsonAsync<GithubRelease>("https://api.github.com/repos/IvanJosipovic/KubeUI/releases/latest");
            }

            return githubRelease;
        }

        public class GithubRelease
        {
            public string url { get; set; }
            public string assets_url { get; set; }
            public string upload_url { get; set; }
            public string html_url { get; set; }
            public int id { get; set; }
            public string node_id { get; set; }
            public string tag_name { get; set; }
            public string target_commitish { get; set; }
            public string name { get; set; }
            public bool draft { get; set; }
            public bool prerelease { get; set; }
            public DateTime created_at { get; set; }
            public DateTime published_at { get; set; }
            public Asset[] assets { get; set; }
            public string tarball_url { get; set; }
            public string zipball_url { get; set; }
            public string body { get; set; }

            public class Asset
            {
                public string url { get; set; }
                public int id { get; set; }
                public string node_id { get; set; }
                public string name { get; set; }
                public string label { get; set; }
                public string content_type { get; set; }
                public string state { get; set; }
                public int size { get; set; }
                public int download_count { get; set; }
                public DateTime created_at { get; set; }
                public DateTime updated_at { get; set; }
                public string browser_download_url { get; set; }
            }
        }
    }
}
