using Newtonsoft.Json.Linq;
using Sudoverse.SudokuModel;
using Sudoverse.Util;
using System;
using System.IO;

namespace Sudoverse
{
    /// <summary>
    /// Handles loading and saving of the config file.
    /// </summary>
    public static class Config
    {
        private static readonly string ConfigFileName = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "config.json");

        private static PencilmarkType pencilmarkType = PencilmarkType.CenterBorderPencilmarkType;

        public static PencilmarkType PencilmarkType
        {
            get => pencilmarkType;
            set
            {
                pencilmarkType = value;
                StoreConfig();
            }
        }

        static Config()
        {
            if (File.Exists(ConfigFileName))
            {
                var json = File.ReadAllText(ConfigFileName);
                var configToken = JToken.Parse(json);

                if (!(configToken is JObject configObject))
                    throw new ParseJsonException();

                if (configObject.TryGetValue("pencilmark_type", out JToken pencilmarkTypeToken))
                {
                    if (pencilmarkTypeToken.Type != JTokenType.String)
                        throw new ParseJsonException(pencilmarkTypeToken.Type, JTokenType.String);

                    pencilmarkType = PencilmarkType.FromIdentifier((string)pencilmarkTypeToken);
                }
            }
        }

        private static void StoreConfig()
        {
            var configObject = new JObject()
            {
                { "pencilmark_type", PencilmarkType.GetIdentifier() }
            };
            var json = configObject.ToString();
            File.WriteAllText(ConfigFileName, json);
        }
    }
}
