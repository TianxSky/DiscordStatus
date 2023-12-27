using Newtonsoft.Json;

namespace DiscordStatus
{
    internal static class ConfigManager
    {
        internal static string? FileDir;
        internal static string? FilePath;

        internal static void GetPath(string ModuleDirectory, string ModuleName)
        {
            string? parentDirectory = Directory.GetParent(path: Directory.GetParent(ModuleDirectory).FullName)?.FullName;
            FileDir = Path.Combine(parentDirectory, @$"configs/plugins/{ModuleName}");
            FilePath = Path.Combine(FileDir, $"{ModuleName}.json");
        }

        internal static async Task UpdateAsync(Globals globals)
        {
            try
            {
                var json = await File.ReadAllTextAsync(FilePath);
                var configData = JsonConvert.DeserializeObject<DSconfig>(json);

                // Update the properties in the provided Globals instance
                globals.Config = configData;
                globals.GConfig = configData.GeneralConfig;
                globals.WConfig = configData.WebhookConfig;
                globals.EConfig = configData.EmbedConfig;
                globals.ServerIP = configData.GeneralConfig.ServerIP;
                globals.MessageID = configData.WebhookConfig.StatusMessageID;
                globals.NameFormat = configData.EmbedConfig.NameFormat;
                globals.ConnectURL = string.Concat(configData.GeneralConfig.PHPURL, $"?ip={globals.ServerIP}");
                globals.HasCC = configData.EmbedConfig.NameFormat.Contains("{CC}") || configData.EmbedConfig.NameFormat.Contains("{FLAG}");
                globals.HasRC = configData.EmbedConfig.NameFormat.Contains("{RC}");
                DSLog.Log(1, "Read configuration data successfully.");
            }
            catch (JsonException ex)
            {
                DSLog.Log(2, $"Failed deserializing json: {ex.Message}");
            }
            catch (Exception ex)
            {
                DSLog.Log(2, $"Failed to read configuration data: {ex.Message}");
                throw;
            }
        }

        internal static async Task SaveAsync(string className, string propertyName, object propertyValue)
        {
            var json = File.ReadAllText(FilePath);
            var configData = JsonConvert.DeserializeObject<DSconfig>(json);

            // Use reflection to get the specified class instance within DSconfig
            var classProperty = typeof(DSconfig).GetProperty(className);
            if (classProperty == null)
            {
                DSLog.Log(2, $"Class '{className}' not found in DSconfig.");
                return;
            }

            // Get the current instance of the specified class
            var classInstance = classProperty.GetValue(configData);

            // Use reflection to set the specified property within the class
            var propertyInfo = classInstance.GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(classInstance, propertyValue);
            }
            else
            {
                DSLog.Log(2, $"'{propertyName}' is not found in the '{className}' class.");
                return;
            }

            var updatedJson = JsonConvert.SerializeObject(configData, Formatting.Indented);
            await File.WriteAllTextAsync(FilePath, updatedJson);
            DSLog.Log(1, $"Saved {propertyName} to {className} in DSconfig.");
        }

        internal static async Task RenameAsync(DSconfig Config)
        {
            var oldConfigName = Path.GetFileNameWithoutExtension(FilePath) + "(old).json";
            var oldConfigPath = Path.Combine(FileDir, oldConfigName);
            File.Move(FilePath, oldConfigPath);
            string? json = JsonConvert.SerializeObject(Config, Formatting.Indented);
            await File.WriteAllTextAsync(FilePath, json);
        }
    }
}