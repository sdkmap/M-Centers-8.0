using System;
using System.IO;
using System.Xml.Serialization;
public enum ModOptions{ DllMethodOnline,DllMethodAutoPatch,DllMethodAutoPatchNonPermanent,MCenters5,HookMethod}

public class Config
{
    private static readonly string ConfigFilePath = "C:\\ProgramData\\MCenters\\config.xml"; // Change to your desired file path

    public ModOptions SelectedMod { get; set; }
    private static Config ConfigCache;

    // Save the current configuration to the config file
    public void Save()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Config));
        using (StreamWriter writer = new StreamWriter(ConfigFilePath))
        {
            serializer.Serialize(writer, this);
        }
    }

    // Load the configuration from the config file
    public static Config Load()
    {
        if(ConfigCache != null) return ConfigCache;
        if (!File.Exists(ConfigFilePath))
        {
            ConfigCache=new Config(); // Return default config if file does not exist
            return ConfigCache;
        }

        XmlSerializer serializer = new XmlSerializer(typeof(Config));
        using (StreamReader reader = new StreamReader(ConfigFilePath))
        {
            ConfigCache= (Config)serializer.Deserialize(reader);
            return ConfigCache;
        }
    }
}
