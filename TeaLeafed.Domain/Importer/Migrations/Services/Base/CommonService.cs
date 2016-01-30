using System.Collections.Generic;
using System.Linq;
using TeaLeafed.Domain.Importer.Internal.Constants;
using TeaLeafed.Domain.Importer.Internal.Services;
using TeaLeafed.Domain.Importer.Migrations.Models;
using TeaLeafed.Domain.Importer.Migrations.Models.Configuration;

namespace TeaLeafed.Domain.Importer.Migrations.Services.Base
{
    public class CommonService : CommonHelper
    {
        /// <summary>
        /// The pages created by the liting process
        /// </summary>
        protected List<PageModel> Results { get; set; } = new List<PageModel>();

        /// <summary>
        /// Urls to search for
        /// </summary>
        public List<string> Urls { get; set; } = new List<string>();

        /// <summary>
        /// The primary domain being searched
        /// </summary>
        protected MigrationOptions Options { get; set; }

        /// <summary>
        /// Show if we have any results
        /// </summary>
        public bool HasResults => Results != null && Results.Any();

        /// <summary>
        /// Pre load the details page url from listings results if they exists
        /// </summary>
        /// <param name="key"></param>
        public void LoadUrlsFromResults(string key)
        {
            Results = GetFromDisk<List<PageModel>>(key, Files.Results);

            if (Results != null)
            {
                Urls = new List<string>();

                foreach (var pageModel in Results)
                {
                    Urls.Add(pageModel.Url);
                }
            }
        }
    }
}
