using Peer2P;

namespace WebApp;

internal abstract class Program
{
    public static void Main(string[] args)
    {
        if (!Peer2PManager.TryInitialize())
        {
            Console.WriteLine("Failed to initialize the Peer2P library manager. Exiting...");
            return;
        }
        
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorPages();

        WebApplication app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}