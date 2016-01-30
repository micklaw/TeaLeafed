using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Archetype.Models;
using TeaLeafed.Domain.Importer.Imports.Converters;
using TeaLeafed.Domain.Importer.Internal.Constants;
using TeaLeafed.Domain.Importer.Internal.Helpers.Absract;
using TeaLeafed.Domain.Importer.Internal.Request.Services.Abstract;
using TeaLeafed.Domain.Importer.Internal.Utils;
using TeaLeafed.Domain.Importer.Migrations.Models;
using TeaLeafed.Domain.Importer.Migrations.Models.Configuration;
using TeaLeafed.Domain.Importer.Migrations.Services.Base;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace TeaLeafed.Domain.Importer.Imports.Services
{
    /// <summary>
    /// Imports the content of a results.json file in to Ubraco
    /// </summary>
    public class ImporterService : CommonService
    {
        /// <summary>
        /// Get remote files
        /// </summary>
        private IRequestService _request { get; set; }

        /// <summary>
        /// A Shared umbraco Helper
        /// </summary>
        private IMigrationHelper _helper { get; }

        /// <summary>
        /// 
        /// </summary>
        private IMigrationContentService _content { get; }

        /// <summary>
        /// Constructor which creates the json configuration for the import and also a request service for the grabing the remote html
        /// </summary>
        /// <param name="options"></param>
        /// <param name="helper"></param>
        /// <param name="content"></param>
        /// <param name="request"></param>
        public ImporterService(MigrationOptions options, IMigrationHelper helper, IMigrationContentService content, IRequestService request)
        {
            Options = options.NotNull();

            _helper = helper.NotNull();
            _content = content.NotNull();
            _request = request.NotNull();
        }

        

        /// <summary>
        /// Generate a url slug from a string
        /// </summary>
        /// <param name="phrase"></param>
        /// <returns></returns>
        private string GenerateSlug(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
            {
                return phrase;
            }

            string slug = RemoveAccent(phrase).ToLower();

            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", " ").Trim();
            slug = slug.Substring(0, slug.Length <= 45 ? slug.Length : 45).Trim();
            slug = Regex.Replace(slug, @"\s", "-"); // hyphens  

            return slug;
        }


        /// <summary>
        /// Remove any accent characters form the url
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        private string RemoveAccent(string txt)
        {
            byte[] bytes = Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return Encoding.ASCII.GetString(bytes);
        }


        /// <summary>
        /// Import all the pages
        /// </summary>
        public List<int> ImportContent()
        {
            var ids = new List<int>();

            if (Options.CreateRewriteFile)
            {
                SerialiseToDisk(string.Empty, Options.Key, Files.Rewrite);    
            }

            if (Results != null && Results.Any())
            {
                foreach (var page in Results)
                {
                    var id = CreateContent(page, Options.ParentNodeId);

                    if (id.HasValue)
                    {
                        ids.Add(id.Value);
                    }
                }
            }

            return ids;
        }

        

        /// <summary>
        /// Creates a page from the page model
        /// </summary>
        /// <param name="page"></param>
        /// <param name="parentId"></param>
        /// <returns>
        /// returns the id of the new page
        /// </returns>
        private int? CreateContent(PageModel page, int parentId = -1)
        {
            // [ML] - Validate inputs

            parentId = parentId == 0 || parentId < -1 ? -1 : parentId;
            page = page.NotNull();

            var uri = new Uri(page.Url);

            var oldUrl = uri.PathAndQuery;

            IContent content = null;

            // [ML] - Chick if this new page exists from its name

            var nodeId = _helper.GetByRoute(GenerateSlug(page.Name));

            if (nodeId > 0)
            {
                content = _content.GetById(nodeId);
            }

            // [ML] - If we dont fid the original node then create one

            if (content == null)
            {
                content = _content.CreateContent(page.Name, parentId, Options.DocumentTypeAlias);
            }

            if (page.Properties != null)
            {
                foreach (var property in page.Properties)
                {
                    // [ML] - If there is a map for this field, get it and try conversion

                    var map = Options.Mappings.FirstOrDefault(i => i.PropertyAlias == property.Key);

                    if (!string.IsNullOrWhiteSpace(map?.ImporterAlias))
                    {
                        // [ML] - Get the converter for the importer

                        var converter = ImporterResolver.Get(map.ImporterAlias);

                        if (converter != null)
                        {
                            // [ML] - Add the mandatory params

                            var parameters = new List<object> { ApplicationContext.Current, property.Value, page.Url };

                            if (map.ImporterArgs?.Length > 0)
                            {
                                // [ML] - Add optional config params

                                parameters.AddRange(map.ImporterArgs);
                            }

                            // [ML] - Convert the value, if neccessary not null then populate it

                            var converted = converter.DynamicInvoke(parameters.ToArray());

                            if (converted != null)
                            {
                                content.SetValue(property.Key, converted);

                                continue;
                            }
                        }
                    }

                    // [ML] - If the conversion fails then set the original value

                    content.SetValue(property.Key, property.Value);
                }
            }

            _content.Save(content);

            // [ML] - If this is valid, publish it

            if (content.IsValid())
            {
                _content.SaveAndPublishWithStatus(content);
            }

            // [ML] - If we are creating a rewrite file, then do it

            if (content.Id > 0 && Options.CreateRewriteFile)
            {
                var newUrl = _content.NiceUrl(content.Id);

                AddToReWriteFile(oldUrl, newUrl);
            }

            return content.Id;
        }

        /// <summary>
        /// Creates and appends to a UrlReWrite text file, the contents of which are to be copied into the UrlReWrite config
        /// </summary>
        /// <param name="oldUrl"> The old url of the blog </param>
        /// <param name="newUrl"> The new url of the blog </param>
        /// <returns></returns>
        private void AddToReWriteFile(string oldUrl, string newUrl)
        {
            var contents = GetFromDisk(Options.Key, Files.Rewrite);

            contents += $"<add name =\"{Guid.NewGuid().ToString()}\" redirect=\"Application\" redirectMode=\"Permanent\" ignoreCase=\"true\" rewriteUrlParameter=\"IncludeQueryStringForRewrite\" virtualURl=\"~{oldUrl}\" destinationUrl=\"~{newUrl}\"/>\n";

            SerialiseToDisk(contents, Options.Key, Files.Rewrite);
        }
    }
}
