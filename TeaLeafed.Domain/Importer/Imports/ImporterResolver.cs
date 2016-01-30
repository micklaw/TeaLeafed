using System;
using System.Collections.Concurrent;
using TeaLeafed.Domain.Importer.Imports.Converters;
using TeaLeafed.Domain.Importer.Internal.Utils;

namespace TeaLeafed.Domain.Importer.Imports
{
    /// <summary>
    /// Site wide cached container for common methods for importing values from the .json file to Umbraco
    /// </summary>
    public class ImporterResolver
    {
        /// <summary>
        /// Cache
        /// </summary>
        private static ConcurrentDictionary<string, Delegate> _converters { get; } = new ConcurrentDictionary<string, Delegate>();

        /// <summary>
        /// Register the common converters
        /// </summary>
        public static void Init()
        {
            Register("image", ImporterConverters.GetImage);
            Register("blockFile", ImporterConverters.GetFile);
        }

        /// <summary>
        /// Add a new mapping to the list
        /// </summary>
        /// <param name="key"></param>
        /// <param name="func"></param>
        public static void Register(string key, Delegate func)
        {
            key = key.NotNull();

            _converters.TryAdd(key.ToLower(), func);
        }

        /// <summary>
        /// Get a mapping from the list
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Delegate Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            Delegate func;

            if (!_converters.TryGetValue(key.ToLower(), out func))
            {
                throw new IndexOutOfRangeException($"No Delegate found for key '{key}'");
            }

            return func;
        }

        /// <summary>
        /// Remove a mapping from the list
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key)
        {
            Delegate func;

            _converters.TryRemove(key.ToLower(), out func);
        }

        /// <summary>
        /// Clear the list
        /// </summary>
        internal static void Clear()
        {
            _converters.Clear();
        }
    }
}
