using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KubeUI.Core.Client
{
    public class HelmClient
    {
        private V1Secret secret;

        private HelmRelease? release;

        public HelmClient(V1Secret secret)
        {
            this.secret = secret;
        }

        public string Title()
        {
            return GetHelmRelease().name;
        }

        public string Version()
        {
            return GetHelmRelease().chart.metadata.version;
        }

        public string AppVersion()
        {
            return GetHelmRelease().chart.metadata.appVersion;
        }

        public string Namespace()
        {
            return GetHelmRelease()._namespace;
        }

        private static byte[] DecompressString(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }

        public HelmRelease GetHelmRelease()
        {
            if (release == null)
            {
                var data = Convert.FromBase64String(Encoding.UTF8.GetString(secret.Data["release"]));
                release = JsonSerializer.Deserialize<HelmRelease>(DecompressString(data));
            }

            return release;
        }
    }

    [KubernetesEntity(ApiVersion = "v1", Group = "helm.sh", Kind = "release", PluralName = "releases")]
    public class HelmRelease
    {
        public string name { get; set; }
        public Info info { get; set; }
        public Chart chart { get; set; }
        public JsonObject config { get; set; }
        public string manifest { get; set; }
        public int version { get; set; }

        [JsonPropertyName("namespace")]
        public string _namespace { get; set; }

        public class Info
        {
            public string first_deployed { get; set; }
            public string last_deployed { get; set; }
            public string deleted { get; set; }
            public string description { get; set; }
            public string status { get; set; }
            public string notes { get; set; }
        }

        public class Chart
        {
            public Metadata metadata { get; set; }
            public Lock _lock { get; set; }
            public Template[] templates { get; set; }
            public JsonObject values { get; set; }
            public object schema { get; set; }
            public File[] files { get; set; }
        }

        public class Metadata
        {
            public string name { get; set; }
            public string home { get; set; }
            public string[] sources { get; set; }
            public string version { get; set; }
            public string description { get; set; }
            public string[] keywords { get; set; }
            public Maintainer[] maintainers { get; set; }
            public string icon { get; set; }
            public string apiVersion { get; set; }
            public string appVersion { get; set; }
            public string kubeVersion { get; set; }
            public Dependency[] dependencies { get; set; }
        }

        public class Maintainer
        {
            public string name { get; set; }
            public string email { get; set; }
        }

        public class Dependency
        {
            public string name { get; set; }
            public string version { get; set; }
            public string repository { get; set; }
            public bool enabled { get; set; }
        }

        public class Lock
        {
            public string generated { get; set; }
            public string digest { get; set; }
            public LockDependency[] dependencies { get; set; }
        }

        public class LockDependency
        {
            public string name { get; set; }
            public string version { get; set; }
            public string repository { get; set; }
        }

        public class Template
        {
            public string name { get; set; }
            public string data { get; set; }
        }

        public class File
        {
            public string name { get; set; }
            public string data { get; set; }
        }
    }
}
