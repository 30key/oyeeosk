using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Jackett.Common.Indexers.Abstract;
using Jackett.Common.Models;
using Jackett.Common.Services.Interfaces;
using Jackett.Common.Utils.Clients;
using NLog;

namespace Jackett.Common.Indexers
{
    [ExcludeFromCodeCoverage]
    public class Seals : GazelleTracker
    {
        public Seals(IIndexerConfigurationService configService, WebClient wc, Logger l, IProtectionService ps,
            ICacheService cs)
            : base(id: "seals",
                   name: "Seals",
                   description: "Seals is a CHINESE Private site for MOVIES / TV",
                   link: StringUtil.FromBase64("aHR0cHM6Ly9ncmVhdHBvc3RlcndhbGwuY29tLw=="),
                   caps: new TorznabCapabilities
                   {
                       TvSearchParams = new List<TvSearchParam>
                       {
                           TvSearchParam.Q, TvSearchParam.Season, TvSearchParam.Ep
                       },
                       MovieSearchParams = new List<MovieSearchParam>
                       {
                           MovieSearchParam.Q, MovieSearchParam.ImdbId
                       }
                   },
                   configService: configService,
                   client: wc,
                   logger: l,
                   p: ps,
                   cs: cs,
                   supportsFreeleechTokens: true,
                   imdbInTags: false,
                   has2Fa: true,
                   useApiKey: false,
                   usePassKey: false,
                   instructionMessageOptional: null
                  )
        {
            Language = "zh-CN";
            Type = "private";

            // Seals does not have categories so these are just to tag the results with both for torznab apps.
            AddCategoryMapping(1, TorznabCatType.Movies, "Movies 电影");
            AddCategoryMapping(2, TorznabCatType.TV, "TV 电视");
        }

        protected override async Task<IEnumerable<ReleaseInfo>> PerformQuery(TorznabQuery query)
        {
            // Seals does not support categories so drop cat filtering.
            query.Categories = new int[0];

            // Seals uses imdbid in the searchstr so prevent cataloguenumber or taglist search.
            if (query.IsImdbQuery)
            {
                query.SearchTerm = query.ImdbID;
                query.ImdbID = null;
            }

            var releases = await base.PerformQuery(query);
            foreach (var release in releases)
            {
                release.MinimumRatio = 1;
                release.MinimumSeedTime = 172800; // 48 hours
                // tag each results with both Movie and TV cats.
                release.Category = new List<int> { TorznabCatType.Movies.ID, TorznabCatType.TV.ID };
            }
            return releases;
        }
    }
}
