namespace TeaLeafed.Domain.Importer.Internal.Helpers.Absract
{
    /// <summary>
    /// Wrapper for searching for existing content by url
    /// </summary>
    public interface IMigrationHelper
    {
        /// <summary>
        /// Get a content item by its url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="ignoreTopLevelNode"></param>
        /// <returns></returns>
        int GetByRoute(string url, bool ignoreTopLevelNode = false);
    }
}
