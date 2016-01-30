using Umbraco.Core.Models;

namespace TeaLeafed.Domain.Importer.Internal.Helpers.Absract
{
    /// <summary>
    /// Wrapper for searching for existing content by url
    /// </summary>
    public interface IMigrationContentService
    {
        /// <summary>
        /// Get a content item by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IContent GetById(int id);

        /// <summary>
        /// Creates a node initiall and returns it
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parentId"></param>
        /// <param name="documentTypeAlias"></param>
        /// <returns></returns>
        IContent CreateContent(string name, int parentId, string documentTypeAlias);

        /// <summary>
        /// Save a node
        /// </summary>
        /// <param name="content"></param>
        void Save(IContent content);

        /// <summary>
        /// Save a content and publish it
        /// </summary>
        /// <param name="content"></param>
        void SaveAndPublishWithStatus(IContent content);

        /// <summary>
        /// Get the nice url for a node
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string NiceUrl(int id);
    }
}
