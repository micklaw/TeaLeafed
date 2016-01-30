using TeaLeafed.Domain.Importer.Internal.Helpers.Absract;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace TeaLeafed.Domain.Importer.Internal.Helpers
{
    /// <summary>
    /// Content service for Umbraco
    /// </summary>
    internal class MigrationContentService : IMigrationContentService
    {
        /// <summary>
        /// container for the content service
        /// </summary>
        private IContentService _content { get; set; }

        public MigrationContentService()
        {
            _content = ApplicationContext.Current.Services.ContentService;
        }

        /// <summary>
        /// Get a IContent by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IContent GetById(int id) => _content.GetById(id);

        /// <summary>
        /// Create content initial
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parentId"></param>
        /// <param name="documentTypeAlias"></param>
        /// <returns></returns>
        public IContent CreateContent(string name, int parentId, string documentTypeAlias) => _content.CreateContent(name, parentId, documentTypeAlias);

        /// <summary>
        /// Save an updated content
        /// </summary>
        /// <param name="content"></param>
        public void Save(IContent content) => _content.Save(content);

        /// <summary>
        /// Save and publish
        /// </summary>
        /// <param name="content"></param>
        public void SaveAndPublishWithStatus(IContent content) => _content.SaveAndPublishWithStatus(content);

        /// <summary>
        /// Gets the nice url from a content model
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string NiceUrl(int id)
        {
            return library.NiceUrl(id);
        }
    }
}
