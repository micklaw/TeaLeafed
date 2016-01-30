using System;
using HtmlAgilityPack;
using TeaLeafed.Domain.Importer.Internal.Utils;
using TeaLeafed.Domain.Importer.Migrations.Models.Configuration;

namespace TeaLeafed.Domain.Importer.Migrations.Convertors
{
    /// <summary>
    /// Used for the common methods in for data retrieval in scraping pages
    /// </summary>
    public class MigrationConverters
    {
        /// <summary>
        /// Validate the field passed in
        /// </summary>
        /// <param name="document"></param>
        /// <param name="node"></param>
        /// <param name="useRelative"></param>
        private static HtmlNode ValidateDefaults(HtmlDocument document, HtmlNode node, bool useRelative)
        {
            document = document.NotNull();

            if (useRelative)
            {
                node = node.NotNull();
            }

            return useRelative ? node : document.DocumentNode;
        }

        /// <summary>
        /// Gets the inner Html of an element
        /// </summary>
        public static Func<MigrationOptions, HtmlDocument, HtmlNode, bool, string, string> InnerHtml = (options, document, node, useRelative, xPath) =>
        {
            var root = ValidateDefaults(document, node, useRelative);

            var element = root.SelectSingleNode(xPath);

            if (element != null)
            {
                return (element.InnerHtml ?? string.Empty).Trim('\r', '\n', ' ');
            }

            return null;
        };

        /// <summary>
        /// Gets the text inside an element
        /// </summary>
        public static Func<MigrationOptions, HtmlDocument, HtmlNode, bool, string, string> InnerText = (options, document, node, useRelative, xPath) =>
        {
            var root = ValidateDefaults(document, node, useRelative);

            var element = root.SelectSingleNode(xPath);

            if (element != null)
            {
                var value = element.InnerText;

                return (value ?? string.Empty).Trim('\r', '\n', ' ');
            }

            return null;
        };

        /// <summary>
        /// Gets the text of an attribute
        /// </summary>
        public static Func<MigrationOptions, HtmlDocument, HtmlNode, bool, string, string, string> AttributeValue = (options, document, node, useRelative, xPath, attribute) =>
        {
            var root = ValidateDefaults(document, node, useRelative);

            var element = root.SelectSingleNode(xPath);

            if (element != null)
            {
                if (!string.IsNullOrWhiteSpace(attribute))
                {
                    var attributeElement = element.Attributes[attribute];

                    var value = (attributeElement != null ? attributeElement.Value : string.Empty);

                    return (value ?? string.Empty).Trim('\r', '\n', ' ');
                } 
            }

            return null;
        };
    }
}
