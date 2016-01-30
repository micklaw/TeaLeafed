using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Cache;
using Archetype.Models;
using HtmlAgilityPack;
using TeaLeafed.Domain.Importer.Internal.Request.Services;
using TeaLeafed.Domain.Importer.Internal.Utils;
using TeaLeafed.Domain.Importer.Migrations.Models.Configuration;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace TeaLeafed.Domain.Importer.Imports.Converters
{

    /// <summary>
    /// Used for the common import functionality for converting values in the .json files to Umbraco
    /// </summary>
    public class ImporterConverters
    {
        /// <summary>
        /// Get or Create a media folder
        /// </summary>
        /// <param name="mediaName"></param>
        /// <param name="mediaService"></param>
        /// <param name="documentType"> </param>
        /// <param name="parentNodeId"></param>
        /// <returns></returns>
        public static IMedia GetOrCreateMediaItem(IMediaService mediaService, string documentType, int parentNodeId, string mediaName, byte[] file = null)
        {
            var childItems = mediaService.GetChildren(parentNodeId);

            IMedia childItem = null;

            var enumerable = childItems as IMedia[] ?? childItems.ToArray();

            if (enumerable.Any())
            {
                childItem = enumerable.FirstOrDefault(i => i.Name.ToLower().Equals(mediaName.ToLower()));
            }

            if (childItem == null)
            {
                childItem = mediaService.CreateMedia(mediaName, parentNodeId, documentType);

                if (file != null)
                {
                    childItem.SetValue("umbracoFile", mediaName, new MemoryStream(file));
                }

                mediaService.Save(childItem);
            }

            return childItem;
        }

        /// <summary>
        /// Creates the heirarchy of folder structures
        /// </summary>
        /// <param name="mediaService"></param>
        /// <param name="url"></param>
        /// <param name="parentMediaFolder"></param>
        /// <returns></returns>
        private static int CreateFoldersFromPath(IMediaService mediaService, Uri url, int parentMediaFolder)
        {
            if (parentMediaFolder < -1 || parentMediaFolder == 0)
            {
                parentMediaFolder = -1;
            }

            var segments = url.Segments.Select(i => i.TrimStart('/').TrimEnd('/')).Where(i => !string.IsNullOrWhiteSpace(i));

            foreach (var segment in segments)
            {
                var media = GetOrCreateMediaItem(mediaService, "folder", parentMediaFolder, segment);

                if (media != null && media.Id > 0)
                {
                    parentMediaFolder = media.Id;
                }
            }

            return parentMediaFolder;
        }

        /// <summary>
        /// Creates the heirarchy of folder structures
        /// </summary>
        /// <param name="mediaService"></param>
        /// <param name="docType"></param>
        /// <param name="url"></param>
        /// <param name="parentMediaFolder"></param>
        /// <returns></returns>
        private static int CreateMedia(IMediaService mediaService, string docType, Uri url, int parentMediaFolder)
        {
            if (parentMediaFolder < -1 || parentMediaFolder == 0)
            {
                parentMediaFolder = -1;
            }

            var file = new RemoteRequestService().GetImage(url.AbsoluteUri); // TODO: ML - Need to think if dependenices for common converters here

            if (file != null)
            {
                var media = GetOrCreateMediaItem(mediaService, docType, parentMediaFolder, Path.GetFileName(url.LocalPath), file);

                if (media != null)
                {
                    return media.Id;
                }
            }

            return 0;
        }

        private static int SaveMedia(ApplicationContext context, object value, string url, long parentFolderId, string docType)
        {
            if (value != null)
            {
                var mediaService = context.Services.MediaService;
                var newUri = new Uri(url);

                var folderId = CreateFoldersFromPath(mediaService, newUri, Convert.ToInt32(parentFolderId));

                if (value is string)
                {
                    return CreateMedia(mediaService, docType, new Uri(value.ToString()), folderId);
                }
            }

            return 0;
        }

        public static Func<ApplicationContext, object, string, long, int> GetImage = (context, value, url, parentFolderId) => SaveMedia(context, value, url, parentFolderId, "image");

        public static Func<ApplicationContext, object, string, long, string> GetFile = (context, value, url, parentFolderId) =>
        {
            var id = SaveMedia(context, value, url, parentFolderId, "file");

            if (id > 0)
            {
                var media = context.Services.MediaService.GetById(id);

                if (media != null)
                {
                    return new ArchetypeModel()
                    {
                        Fieldsets = new List<ArchetypeFieldsetModel>
                            {
                                new ArchetypeFieldsetModel()
                                {
                                    Alias = "fileDownloadBlock",
                                    Disabled = false,
                                    Properties = new List<ArchetypePropertyModel>
                                    {
                                        new ArchetypePropertyModel()
                                        {
                                            Alias = "file",
                                            Value = media.Id
                                        }
                                    }
                                }
                            }
                    }.SerializeForPersistence();
                }
            }

            return null;
        };
    }
}
