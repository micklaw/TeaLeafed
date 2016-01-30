using System;
using System.Linq;
using System.Threading.Tasks;
using TeaLeafed.Domain.Importer.Imports.Services;
using TeaLeafed.Domain.Importer.Internal.Constants;
using TeaLeafed.Domain.Importer.Internal.Helpers;
using TeaLeafed.Domain.Importer.Internal.Request.Services;
using TeaLeafed.Domain.Importer.Internal.Services;
using TeaLeafed.Domain.Importer.Migrations.Models.Configuration;
using TeaLeafed.Domain.Importer.Migrations.Services;
using Umbraco.Web.WebApi;

namespace TeaLeafed.Domain.Importer.Migrations.Controllers.Api
{
    /// <summary>
    /// Controller used to interaction with the migration service
    /// </summary>
    public class MigrationController : UmbracoAuthorizedApiController
    {
        private MigrationService _migration { get; set; }
        private CommonHelper _common { get; set; }

        public MigrationController()
        {
            _common = new CommonHelper();
        }

        /// <summary>
        /// Try really hard to populate this from a string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private MigrationOptions SetMigrationService(string key)
        {
            var options = _common.GetFromDisk<MigrationOptions>(key, Files.Options);

            if (options != null)
            {
                _migration = new MigrationService(options, new RemoteRequestService());

                return options;
            }

            return null;
        }


        // CONFIGURE 

        /// <summary>
        /// Setup the options for the crawl from our Angulat views post
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public object Setup(MigrationOptions options)
        {
            //TODO: ML - Actually build a front end for this

            options.Key = Guid.NewGuid().ToString();

            _common.SerialiseToDisk(options, options.Key, Files.Options);

            return Json(new { key = options.Key });
        }

        // THEN DO

        #region Listing & Child Page

        /// <summary>
        /// Iterate the items of a listing page to pull its pages and content
        /// </summary>
        /// <param name="key">The key of the options</param>
        /// <param name="url">The urls to crawl</param>
        /// <returns></returns>
        public async Task<bool> GetListings(string key, string url)
        {
            var options = SetMigrationService(key);

            if (options != null)
            {
                _migration = new MigrationService(options, new RemoteRequestService());

                if (!string.IsNullOrWhiteSpace(url))
                {
                    _migration.AddInitialUrls(new []{ url });

                    var results = await _migration.ProcessListing();

                    return results != null;
                }
            }

            return false;
        }

        /// <summary>
        ///  Complete the pages pulled from the listing page and grab each pages individual details
        /// </summary>
        /// <param name="key">The key of the options</param>
        /// <returns></returns>
        public async Task<bool> GetListingsComplete(string key)
        {
            var options = SetMigrationService(key);

            if (options != null)
            {
                _migration = new MigrationService(options, new RemoteRequestService());

                _migration.LoadUrlsFromResults(key);

                var results = await _migration.ProcessDetails();

                return results != null;
            }

            return false;
        }

        #endregion Listing & Child Page

        // OR

        #region Child Pages only

        /// <summary>
        /// Get the content for the listing mappings on the import
        /// </summary>
        /// <param name="key">The key of the options</param>
        /// <param name="urls">The urls to crawl</param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public async Task<bool> GetDetails(string key, string[] urls)
        {
            var options = SetMigrationService(key);

            if (options != null)
            {
                _migration = new MigrationService(options, new RemoteRequestService());

                if (urls != null && urls.Any())
                {
                    _migration.AddInitialUrls(urls);

                    var results = await _migration.ProcessDetails();

                    return results != null;
                }
            }

            return false;
        }

        #endregion Child Pages only

        // THEN FINALLY

        #region Import To Umbraco

        /// <summary>
        /// Import all of the content to umbraco
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool GetImport(string key)
        {
            var options = SetMigrationService(key);

            if (options != null)
            {
                var importer = new ImporterService(options, new MigrationHelper(), new MigrationContentService(), new RemoteRequestService());

                importer.LoadUrlsFromResults(key);

                if (importer.HasResults)
                {
                    var ids = importer.ImportContent();

                    return ids != null;
                }
            }

            return false;
        }

        #endregion Import To Umbraco
    }
}
