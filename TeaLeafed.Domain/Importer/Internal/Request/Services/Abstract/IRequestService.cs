using System.Threading.Tasks;

namespace TeaLeafed.Domain.Importer.Internal.Request.Services.Abstract
{
    /// <summary>
    /// Request methods
    /// </summary>
    public interface IRequestService
    {
        /// <summary>
        /// Gets the HTML of a remote url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task<string> GetHtml(string url);

        /// <summary>
        /// Gets a byte array from a given media url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        byte[] GetImage(string url);
    }
}