namespace TeaLeafed.Domain.Importer.Migrations.Models.Configuration
{
    /// <summary>
    /// Class used to map a field to be crawled
    /// </summary>
    public class PropertyMap
    {
        /// <summary>
        /// A optional default value t use if one is not found
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// The umbraco alias of the node property
        /// </summary>
        public string PropertyAlias { get; set; }

        /// <summary>
        /// The converter Func alias
        /// </summary>
        public string ConverterAlias { get; set; }

        /// <summary>
        /// Optional params to be pass to the converter (must be primitive types which can be serialised by json.net)
        /// </summary>
        public object[] ConverterArgs { get; set; }

        /// <summary>
        /// The output value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// An optional string.format to use when outputting a string
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// An optional importer Func alias
        /// </summary>
        public string ImporterAlias { get; set; }

        /// <summary>
        /// Optional argument to send to the importer (must be primitive types which can be serialised by json.net)
        /// </summary>
        public object[] ImporterArgs { get; set; }

        /// <summary>
        /// Use this field as the name of the node
        /// </summary>
        public bool UseAsName { get; set; }
    }
}
