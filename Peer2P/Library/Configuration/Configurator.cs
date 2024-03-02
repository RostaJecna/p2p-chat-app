using Microsoft.Extensions.Configuration;

namespace Peer2P.Library.Configuration;

internal static class Configurator
{
    private static string WorkingDirectory { get; } = Directory.GetCurrentDirectory();

    public static string ConfigFileName { get; set; } = "appsettings.json";

    private static string GetValidatedConfigFilePath()
    {
        if (string.IsNullOrWhiteSpace(ConfigFileName))
            throw new ArgumentException("Configuration file name is invalid or not declared", nameof(ConfigFileName));

        return Path.Combine(WorkingDirectory, ConfigFileName);
    }

    public static IConfigurationBuilder InitBuilder()
    {
        string filePath = GetValidatedConfigFilePath();

        if (!File.Exists(filePath))
            throw new FileNotFoundException(
                $"The config file '{ConfigFileName}' does not exist at the specified path: {WorkingDirectory}");

        return new ConfigurationBuilder().AddJsonFile(ConfigFileName, true, false);
    }
}