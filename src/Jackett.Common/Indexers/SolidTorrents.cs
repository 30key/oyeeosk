using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jackett.Common.Models;
using Jackett.Common.Models.IndexerConfig;
using Jackett.Common.Services.Interfaces;
using Jackett.Common.Utils;
using Jackett.Common.Utils.Clients;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Jackett.Common.Indexers
{
    public class SolidTorrents : BaseWebIndexer
    {
        private string SearchUrl => $"{SiteLink}api/v1/search";

        private readonly Dictionary<string, string> _apiHeaders = new Dictionary<string, string>
        {
            {"Accept", "application/json, text/plain, */*"}
        };

        private readonly int _maxResultsPerPage = 20;
        private readonly int _maxSearchPageLimit = 3; // 20 items per page, 60

        private ConfigurationData ConfigData
        {
            get => configData;
            set => configData = value;
        }

        public SolidTorrents(IIndexerConfigurationService configService, WebClient wc, Logger l, IProtectionService ps) :
            base(
                "Solid Torrents", description: "SolidTorrents is a Public torrent meta-search engine",
                link: "https://solidtorrents.net/", caps: new TorznabCapabilities(), configService: configService,
                client: wc, logger: l, p: ps, configData: new ConfigurationData())
        {
            Encoding = Encoding.UTF8;
            Language = "en-us";
            Type = "public";
            AddCategoryMapping("Audio", TorznabCatType.Audio);
            AddCategoryMapping("Video", TorznabCatType.Movies);
            AddCategoryMapping("Image", TorznabCatType.OtherMisc);
            AddCategoryMapping("Document", TorznabCatType.BooksComics);
            AddCategoryMapping("eBook", TorznabCatType.BooksEbook);
            AddCategoryMapping("Program", TorznabCatType.PC0day);
            AddCategoryMapping("Android", TorznabCatType.PCPhoneAndroid);
            AddCategoryMapping("Archive", TorznabCatType.Other);
            AddCategoryMapping("Diskimage", TorznabCatType.PCISO);
            AddCategoryMapping("Sourcecode", TorznabCatType.MoviesOther);
            AddCategoryMapping("Database", TorznabCatType.MoviesDVD);
            AddCategoryMapping("Unknown", TorznabCatType.Other);
        }

        public override async Task<IndexerConfigurationStatus> ApplyConfiguration(JToken configJson)
        {
            base.LoadValuesFromJson(configJson);
            var releases = await PerformQuery(new TorznabQuery());
            await ConfigureIfOkAsync(
                string.Empty, releases.Any(), () => throw new Exception("Could not find release from this URL."));
            return IndexerConfigurationStatus.Completed;
        }

        private JArray CheckResponse(WebClientStringResult result)
        {
            try
            {
                var json = JsonConvert.DeserializeObject<dynamic>(result.Content);
                if (!(json is JObject) || !(json["results"] is JArray) || json["results"] == null)
                    throw new Exception("Server error");
                return (JArray)json["results"];
            }
            catch (Exception e)
            {
                logger.Error("CheckResponse() Error: ", e.Message);
                throw new ExceptionWithConfigData(result.Content, ConfigData);
            }
        }

        private async Task<JArray> SendSearchRequestAsync(string searchString, string category, int page)
        {
            var queryCollection = new NameValueCollection
            {
                {"q", searchString},
                {"category", category},
                {"skip", (page * _maxResultsPerPage).ToString()},
                {"sort", "date"},
                {"fuv", "no"}
            };
            var fullSearchUrl = $"{SearchUrl}?{queryCollection.GetQueryString()}";
            var result = await RequestStringWithCookiesAndRetryAsync(fullSearchUrl, null, null, _apiHeaders);
            return CheckResponse(result);
        }

        protected override async Task<IEnumerable<ReleaseInfo>> PerformQuery(TorznabQuery query)
        {
            var releases = new List<ReleaseInfo>();
            var searchString = query.GetQueryString();
            var page = 0;
            var cats = MapTorznabCapsToTrackers(query);
            var category = cats.Count > 0 ? string.Join(",", cats) : "all";
            var isLatestSearch = string.IsNullOrWhiteSpace(searchString);
            bool isLastPage;
            do
            {
                var result = await SendSearchRequestAsync(searchString, category, page);
                try
                {
                    foreach (var torrent in result)
                        releases.Add(MakeRelease(torrent));
                }
                catch (Exception ex)
                {
                    OnParseError(result.ToString(), ex);
                }

                isLastPage = result.Count < _maxResultsPerPage;
                page++; // update page number
            } while (!isLatestSearch && !isLastPage && page < _maxSearchPageLimit);

            return releases;
        }

        private ReleaseInfo MakeRelease(JToken torrent)
        {
            var release = new ReleaseInfo
            {
                Title = (string)torrent["title"],

                // https://solidtorrents.net/view/5e10885d651df640a70ee826
                Comments = new Uri($"{SiteLink}view/{(string)torrent["_id"]}")
            };
            release.Guid = release.Comments;
            release.PublishDate = DateTime.Now;
            if (torrent["imported"] != null)
                release.PublishDate = DateTime.Parse((string)torrent["imported"]);
            release.Category = MapTrackerCatToNewznab((string)torrent["category"]);
            release.Size = (long)torrent["size"];
            var swarm = torrent["swarm"];
            release.Seeders = (int)swarm["seeders"];
            release.Peers = release.Seeders + (int)swarm["leechers"];
            release.Grabs = (long)swarm["downloads"];
            release.InfoHash = (string)torrent["infohash"];
            release.MagnetUri = new Uri((string)torrent["magnet"]);
            release.MinimumRatio = 1;
            release.MinimumSeedTime = 172800; // 48 hours
            release.DownloadVolumeFactor = 0;
            release.UploadVolumeFactor = 1;
            return release;
        }
    }
}
