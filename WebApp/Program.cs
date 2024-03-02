using Peer2P;
using Peer2P.Library.Console.Messaging;

namespace WebApp;

internal abstract class Program
{
    public static void Main(string[] args)
    {
        Logger.LogDisplayed += Console.WriteLine;

        if (!Peer2PManager.TryInitialize())
        {
            Logger.Log("Failed to initialize the Peer2P library. Exiting...").Type(LogType.Error).Display();
            return;
        }

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorPages();

        WebApplication app = builder.Build();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapRazorPages();
        app.Run();
    }
}