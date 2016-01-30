using TeaLeafed.Domain.Importer.Internal.Helpers.Absract;
using umbraco;
using Umbraco.Web;

namespace TeaLeafed.Domain.Importer.Internal.Helpers
{
    /// <summary>
    /// Search for content from Umbraco
    /// </summary>
    internal class MigrationHelper : IMigrationHelper
    {
        /// <summary>
        /// container for this class
        /// </summary>
        private UmbracoHelper _helper { get; set; }

        public MigrationHelper()
        {
            _helper = new UmbracoHelper(UmbracoContext.Current);
        }

        /// <summary>
        /// Find content by its url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="ignoreTopLevelNode"></param>
        /// <returns></returns>
        public int GetByRoute(string url, bool ignoreTopLevelNode = false) => uQuery.GetNodeIdByUrl("/" + url.TrimStart('/') + "/");
    }
}
