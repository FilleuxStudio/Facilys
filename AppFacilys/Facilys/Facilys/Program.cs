using Facilys.Components;
using ElectronNET.API;
using Facilys.Components.Services;
using Facilys.Components.Services;
using ElectronNET.API.Entities;
using Facilys.Components.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Facilys", "Database", "data.db")}"));

// Configuer Electron
builder.WebHost.UseElectron(args);
builder.WebHost.UseEnvironment("Development");
builder.Services.AddElectron();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<AuthService>();
builder.Services.AddHttpClient<APIWebSiteService>(client =>
{
    client.BaseAddress = new Uri("https://filleuxstudio.fr/");
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<Facilys.Components.App>()
    .AddInteractiveServerRenderMode();

// Cr�er la fen�tre Electron
async Task CreateElectronWindow()
{
    // Cr�ation d'une nouvelle fen�tre avec les options sp�cifi�es
    var window = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
    {
        Width = 1200,
        Height = 800,
        Show = true // Affiche la fen�tre directement
    });

    // Nettoyer le cache et recharger la fen�tre
    await ClearCache();
    ReloadWindow(window);

    // Action lors de la fermeture de la fen�tre
    window.OnClosed += () => Electron.App.Quit();
}

void ReloadWindow(BrowserWindow? mainWindow)
{
    if (mainWindow != null)
    {
        mainWindow.Reload();
    }
}

async Task ClearCache()
{
    var browserWindows = Electron.WindowManager.BrowserWindows;

    foreach (var window in browserWindows)
    {
        // Efface le cache de session pour chaque fen�tre
        await window.WebContents.Session.ClearCacheAsync();
    }
}

// V�rifiez si l'application s'ex�cute dans Electron
if (HybridSupport.IsElectronActive)
{
    // Cr�e le dossier avant de lancer la fen�tre Electron
    await AuthService.EnsureApplicationFolderExistsAsync();

    await Task.Run(async () =>
    {
        await CreateElectronWindow();
    });
}

app.Run();