using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Humanizer;
using TeaLeafed.Domain.Importer.Internal.Constants;
using TeaLeafed.Domain.Importer.Internal.Request.Services.Abstract;
using TeaLeafed.Domain.Importer.Internal.Utils;
using TeaLeafed.Domain.Importer.Migrations.Models;
using TeaLeafed.Domain.Importer.Migrations.Models.Configuration;
using TeaLeafed.Domain.Importer.Migrations.Services.Base;

// ReSharper disable LoopCanBeConvertedToQuery

namespace TeaLeafed.Domain.Importer.Migrations.Services
{
    public class MigrationService : CommonService
    {
        /// <summary>
        /// Request service to external Urls
        /// </summary>
        public IRequestService _requestService { get; }

        /// <summary>
        /// Constructor which creates the json configuration for the import and also a request service for the grabing the remote html
        /// </summary>
        /// <param name="options"></param>
        /// <param name="requestService"></param>
        public MigrationService(MigrationOptions options, IRequestService requestService)
        {
            Options = options.NotNull();

            _requestService = requestService.NotNull();
        }

        /// <summary>
        /// Add urls to the collection to be crawled for the first time of a listing or as details urls
        /// </summary>
        /// <param name="urls"></param>
        public void AddInitialUrls(string[] urls)
        {
            if (urls?.Length > 0)
            {
                Urls.AddRange(urls);
            }
        }

        /// <summary>
        /// Get the html document and then populate it via the function mapping method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="populateModel"></param>
        /// <returns></returns>
        public async Task<T> GetHtml<T>(string url, Func<HtmlDocument, T> populateModel)
        {
            url = url.NotNull();
            populateModel = populateModel.NotNull();

            var html = await _requestService.GetHtml(url);

            html = html.NotNull();

            var document = new HtmlDocument();

            document.LoadHtml(html);

            return populateModel(document);
        }

        /// <summary>
        /// Pull content from
        /// </summary>
        /// <param name="repeater"></param>
        /// <param name="mappings"></param>
        /// <returns></returns>
        private async Task<IList<PageModel>> GetPages(string repeater, IList<PropertyMap> mappings)
        {
            var tasks = new List<Task>();

            var newPages = new List<PageModel>();

            // [ML] - Iterate over all urls

            foreach (var url in Urls)
            {
                // [ML] - Add each request to a list of Task so we can do this over multiple threads

                tasks.Add(GetHtml(url, (document) =>
                {
                    // [ML] - Get all nodes or just use the main document

                    var nodes = !string.IsNullOrWhiteSpace(repeater) ? (document.DocumentNode.SelectNodes(repeater) ?? new HtmlNodeCollection(null)).ToList() : new List<HtmlNode> { document.DocumentNode };

                    var pages = new List<PageModel>();

                    foreach (var node in nodes)
                    {
                        // [ML] - Get the url of the page from either this or from the listing node

                        var pageUrl = url;

                        var urlMap = mappings.FirstOrDefault(i => i.PropertyAlias.ToLower().Equals("url"));

                        // [ML] - If we have an actual map for the url though, then use that

                        if (urlMap != null)
                        {
                            var inPageUrl = ConvertValue(Options, urlMap, document, node) as string;

                            if (!string.IsNullOrWhiteSpace(inPageUrl))
                            {
                                pageUrl = inPageUrl;
                            }
                        }

                        var pageModel = Results.FirstOrDefault(i => i.Url == pageUrl) ?? new PageModel
                        {
                            Url = pageUrl
                        };

                        if (string.IsNullOrWhiteSpace(pageModel.Name))
                        {
                            pageModel.Name = url.Split('/').Last().Split('.')[0].Replace("-", " ").Replace("  ", " ").Humanize(LetterCasing.Title);
                        }

                        // [ML] - Map the rest of all the properties, except the url one (if matched)

                        foreach (var map in mappings)
                        {
                            if (map != urlMap)
                            {
                                var value = ConvertValue(Options, map, document, node);

                                pageModel.Properties[map.PropertyAlias] = value;

                                // [ML] - Set this as the node name if need be

                                if (map.UseAsName && value != null)
                                {
                                    pageModel.Name = value.ToString();
                                }
                            }
                        }

                        pages.Add(pageModel);
                    }

                    return pages;

                }).ContinueWith(continuation =>
                {
                    // [ML] - Update the collection above with the new pages found

                    if (continuation.IsFaulted && continuation.Exception != null)
                    {
                        throw continuation.Exception.Flatten().InnerException;
                    }

                    var pages = continuation.Result ?? new List<PageModel>();

                    if (pages.Any())
                    {
                        newPages.AddRange(pages);
                    }
                }));
            }

            // [ML] - Wait for them all to complete and then add all the new pages to the old list

            await Task.WhenAll(tasks);

            Results = newPages;

            SerialiseToDisk(Options, Options.Key, Files.Options);
            SerialiseToDisk(Results, Options.Key, Files.Results);

            return Results;
        }

        /// <summary>
        /// Process all the urls based on the listings settings
        /// </summary>
        public async Task<IList<PageModel>> ProcessListing()
        {
            return await GetPages(Options.RepeatingItemXPath, Options.ListingMappings);
        }

        /// <summary>
        /// Process all the urls based on the details settings
        /// </summary>
        public async Task<IList<PageModel>> ProcessDetails()
        {
            return await GetPages(null, Options.DetailsMappings);
        }

        /// <summary>
        /// Convert the value based on the ConverterResolvers saved
        /// </summary>
        /// <param name="options"></param>
        /// <param name="map"></param>
        /// <param name="document"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual object ConvertValue(MigrationOptions options, PropertyMap map, HtmlDocument document, HtmlNode node)
        {
            var convert = ConverterResolver.Get(map.ConverterAlias);

            var value = map.DefaultValue;

            // [ML] - If we have a converter method then invoke it

            if (convert != null)
            {
                // [ML] - Add the mandatory params

                var parameters = new List<object> { options, document, node };

                if (map.ConverterArgs?.Length > 0)
                {
                    // [ML] - Add optional config params

                    parameters.AddRange(map.ConverterArgs);
                }

                // [ML] - Get the value, if its not null then populate it

                var converted = convert.DynamicInvoke(parameters.ToArray()); // TODO: Look to use instance types as well as the static ones in our container

                if (converted != null)
                {
                    value = converted;
                }
            }

            // [ML] - If we have a format, then string it up in this format

            if (value != null && !string.IsNullOrWhiteSpace(map.Format))
            {
                value = string.Format(map.Format, value);
            }

            return value;
        }
    }
}
