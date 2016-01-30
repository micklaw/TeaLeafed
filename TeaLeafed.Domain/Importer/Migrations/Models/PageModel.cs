using System.Collections.Generic;

namespace TeaLeafed.Domain.Importer.Migrations.Models
{
    /// <summary>
    /// Saves a representation of crawled page
    /// </summary>
    public class PageModel
    {
        /// <summary>
        /// The url of the page
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The name of the page
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The properties created from the map
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}
