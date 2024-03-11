using Microsoft.Extensions.Configuration;

namespace Peer2P.Library.Configuration;

/// <summary>
///     Provides functionality to configure the application using Microsoft.Extensions.Configuration.
/// </summary>
internal static class Configurator
{
    private static string WorkingDirectory { get; } = Directory.GetCurrentDirectory();

    /// <summary>
    ///     Gets or sets the name of the configuration file (default is "appsettings.json").
    /// </summary>
    public static string ConfigFileName { get; set; } = "appsettings.json";


    /// <summary>
    ///     Validates the configuration file path and returns the full path.
    /// </summary>
    /// <returns>The validated full path of the configuration file.</returns>
    /// <exception cref="ArgumentException">Thrown if the configuration file name is invalid or not declared.</exception>
    private static string GetValidatedConfigFilePath()
    {
        if (string.IsNullOrWhiteSpace(ConfigFileName))
            throw new ArgumentException("Configuration file name is invalid or not declared", nameof(ConfigFileName));

        return Path.Combine(WorkingDirectory, ConfigFileName);
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="IConfigurationBuilder" /> based on the configuration file.
    /// </summary>
    /// <returns>The initialized configuration builder.</returns>
    /// <exception cref="FileNotFoundException">
    ///     Thrown if the configuration file does not exist at the specified path.
    /// </exception>
    public static IConfigurationBuilder InitBuilder()
    {
        string filePath = GetValidatedConfigFilePath();

        if (!File.Exists(filePath))
            throw new FileNotFoundException(
                $"The config file '{ConfigFileName}' does not exist at the specified path: {WorkingDirectory}");

        return new ConfigurationBuilder().AddJsonFile(ConfigFileName, true, false);
    }
}