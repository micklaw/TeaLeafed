using System.IO;
using System.Web;
using Newtonsoft.Json;
using TeaLeafed.Domain.Importer.Internal.Settings;

namespace TeaLeafed.Domain.Importer.Internal.Services
{
    public class CommonHelper // TODO: ML - Interface this as nasty dependencies don't play with unit tests
    {
        /// <summary>
        /// Get the filename format
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private string FilePathAndName(string key, string type)
        {
            var folder = HttpContext.Current.Server.MapPath($"~/app_data/{MigrationSettings.Instance.SavePath}/{key}");
            
            Directory.CreateDirectory(folder);

            return $"{folder}/{type}.json";
        }

        /// <summary>
        /// Save the object to disk
        /// </summary>
        /// <param name="item"></param>
        /// <param name="key"></param>
        /// <param name="fileType"></param>
        public void SerialiseToDisk(object item, string key, string fileType)
        {
            if (item != null)
            {
                SerialiseToDisk(JsonConvert.SerializeObject(item), key, fileType);
            }
        }

        /// <summary>
        /// Save the object to disk
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="key"></param>
        /// <param name="fileType"></param>
        public void SerialiseToDisk(string contents, string key, string fileType)
        {
            var path = FilePathAndName(key, fileType);

            File.WriteAllText(path, contents);
        }

        /// <summary>
        /// Get the text content of a file
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public string GetFromDisk(string key, string fileType)
        {
            var path = FilePathAndName(key, fileType);

            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            return string.Empty;
        }

        /// <summary>
        /// Return the object previously serialised to disk
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public T GetFromDisk<T>(string key, string fileType)
        {
            var json = GetFromDisk(key, fileType);

            return !string.IsNullOrWhiteSpace(json) ? JsonConvert.DeserializeObject<T>(json) : default(T);
        }
    }
}
