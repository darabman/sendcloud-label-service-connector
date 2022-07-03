using Microsoft.Extensions.Configuration;
using System.IO;

namespace LabelServiceConnector
{
    public static class Configuration
    {
        private static IConfigurationRoot _root;

        public static IConfigurationSection Config => _root.GetSection("Config");

        public static IConfigurationSection Api => _root.GetSection("Api");

        public static IConfigurationSection FieldMapping => _root.GetSection("FieldMapping");

        public static IConfigurationSection Logging => _root.GetSection("Logging");


        static Configuration()
        {
            _root = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path: "appsettings.json", optional: true, reloadOnChange: true)
                .Build();
        }
    }
}
