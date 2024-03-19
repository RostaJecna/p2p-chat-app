using Peer2P;
using Peer2P.Library.Console.Messaging;

namespace WebApp;

/// <summary>
/// Entry point for the Peer2P web application.
/// </summary>
internal abstract class Program
{
    private static readonly CancellationTokenSource ApplicationCancellation = new();
    
    /// <summary>
    /// Main method for starting the Peer2P web application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static void Main(string[] args)
    {
        // Subscribes the Console.WriteLine method to the LogDisplayed event of the Logger.
        Logger.LogDisplayed += Console.WriteLine;

        if (!Peer2PManager.TryInitialize(ApplicationCancellation.Token))
        {
            Logger.Log("Failed to initialize the Peer2P library. Exiting...").Type(LogType.Error).Display();
            ApplicationCancellation.Cancel();
            return;
        }

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorPages();
        builder.Services.AddControllers();

        WebApplication app = builder.Build();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapRazorPages();
        app.MapControllers();
        app.Run();
    }
}