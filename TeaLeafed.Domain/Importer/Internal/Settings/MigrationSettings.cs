using System;
using System.Configuration;

namespace TeaLeafed.Domain.Importer.Internal.Settings
{
    internal class MigrationSettings
    {
        private static readonly Lazy<MigrationSettings> _instance = new Lazy<MigrationSettings>(() => new MigrationSettings());

        public static MigrationSettings Instance => _instance.Value;

        protected MigrationSettings()
        {
            
        }

        /// <summary>
        /// Location used in saving the outputs
        /// </summary>
        public string SavePath => ConfigurationManager.AppSettings["migrator:savePath"] ?? "Migrations";
    }
}
