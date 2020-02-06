using System.Collections.Generic;
using System.Linq;

namespace Jackett.Common.Models
{
    public static partial class TorznabCatType
    {
        public static bool QueryContainsParentCategory(int[] queryCats, ICollection<int> releaseCats)
        {
            foreach (var releaseCat in releaseCats)
            {
                var cat = AllCats.FirstOrDefault(c => c.ID == releaseCat);
                if (cat != null && queryCats != null)
                    return cat.SubCategories.Any(c => queryCats.Contains(c.ID));
            }

            return false;
        }

        public static string GetCatDesc(int newznabcat)
        {
            var cat = AllCats.FirstOrDefault(c => c.ID == newznabcat);
            return cat != null ? cat.Name : string.Empty;
        }

        public static string NormalizeCatName(string name) => name.Replace(" ", "").ToLower();

        public static TorznabCategory GetCatByName(string name)
        {
            var cat = AllCats.FirstOrDefault(c => NormalizeCatName(c.Name) == NormalizeCatName(name));
            return cat;
        }
    }
}
