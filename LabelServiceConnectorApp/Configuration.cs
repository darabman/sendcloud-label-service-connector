using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelServiceConnector
{
    public static class Configuration
    {
        private static IConfigurationRoot _root;

        public static IConfigurationSection App => _root.GetSection("App");

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
