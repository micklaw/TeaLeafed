using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace TeaLeafed.Domain.Importer.Migrations.Models.Configuration
{
    /// <summary>
    /// The options for setting up a new listing migration, must serialise and deserialise from json.net)
    /// </summary>
    public class MigrationOptions
    {
        /// <summary>
        /// Optional flag to output a url rewrite file
        /// </summary>
        public bool CreateRewriteFile { get; set; }

        /// <summary>
        /// The main lookup key of the job
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Optional description for the job
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The HTTP scheme to use
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// The domain of the URL being requested
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Formatted URL based on entered value
        /// </summary>
        [JsonIgnore]
        public string FullDomain => $"{Scheme}://{Domain}";

        /// <summary>
        /// Mandatory if using a listing page
        /// </summary>
        public string RepeatingItemXPath { get; set; }

        /// <summary>
        /// Mappings to be used in a listing search
        /// </summary>
        public List<PropertyMap> ListingMappings { get; set; } = new List<PropertyMap>();

        /// <summary>
        /// Mapping to be used in a details search 
        /// </summary>
        public List<PropertyMap> DetailsMappings { get; set; } = new List<PropertyMap>();

        /// <summary>
        /// The internal backing mappings so were not doing it all the time
        /// </summary>
        private List<PropertyMap> _mappings { get; } = new List<PropertyMap>();

        /// <summary>
        /// Get the Mappings collated 
        /// </summary>
        [JsonIgnore]
        public IList<PropertyMap> Mappings
        {
            get
            {

                if (!_mappings.Any())
                {
                    if (ListingMappings != null && ListingMappings.Any())
                    {
                        _mappings.AddRange(ListingMappings);
                    }

                    if (DetailsMappings != null && DetailsMappings.Any())
                    {
                        _mappings.AddRange(DetailsMappings);
                    }
                }

                return _mappings;
            }
        }

        /// <summary>
        /// The parent node with which to save
        /// </summary>
        public int ParentNodeId { get; set; }

        /// <summary>
        /// The document type alias of page to create
        /// </summary>
        public string DocumentTypeAlias { get; set; }
    }
}
