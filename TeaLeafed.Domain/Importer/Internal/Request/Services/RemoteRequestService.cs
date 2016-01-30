using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TeaLeafed.Domain.Importer.Internal.Request.Services.Abstract;

namespace TeaLeafed.Domain.Importer.Internal.Request.Services
{
    internal class RemoteRequestService : IRequestService
    {
        public async Task<string> GetHtml(string url)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get
                };

                var response = await client.SendAsync(request);

                var byteArray = response.Content.ReadAsByteArrayAsync().Result;

                return Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
            }
        }

        public byte[] GetImage(string url)
        {
            using (var client = new WebClient())
            {
                return client.DownloadData(url);
            }
        }
    }
}
