using System.IO;
using System.Xml.Serialization;

namespace AGFProjects.windowEnteringDuration
{
    public class Config
    {
        public float EnteringAreaDuration = 3.0f;
        private static string configPath;

        public static Config Instance { get; private set; } = new Config();

        static Config()
        {
            // Try to resolve config path relative to the DLL location
            var dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var modFolder = Path.GetDirectoryName(dllPath);
            configPath = Path.Combine(modFolder, "Config", "windowEnteringDuration.xml");
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(configPath))
                {
                    using (var stream = File.OpenRead(configPath))
                    {
                        var serializer = new XmlSerializer(typeof(Config));
                        Instance = (Config)serializer.Deserialize(stream);
                    }
                }
                else
                {
                    Save();
                }
            }
            catch
            {
                Instance = new Config();
            }
        }

        public static void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(configPath));
            using (var stream = File.Create(configPath))
            {
                var serializer = new XmlSerializer(typeof(Config));
                serializer.Serialize(stream, Instance);
            }
        }
    }
}
