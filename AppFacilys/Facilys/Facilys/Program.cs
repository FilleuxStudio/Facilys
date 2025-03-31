//using ElectronNET.API;
//using ElectronNET.API.Entities;
//using Facilys.Components.Data;
//using Facilys.Components.Services;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddDbContext<ApplicationDbContext>(async options =>
//{
//    //var documentsPath = await Electron.App.GetPathAsync(PathName.Documents);
//    var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
//    var facilysPath = Path.Combine(documentsPath, "Facilys");
//    var databasePath = Path.Combine(facilysPath, "Database");
//    var dbFilePath = Path.Combine(databasePath, "data.db");

//    Directory.CreateDirectory(databasePath);

//    options.UseSqlite($"Data Source={dbFilePath}");
//});

//// Configuer Electron
//builder.WebHost.UseElectron(args);
////builder.WebHost.UseEnvironment("Development");
//builder.Services.AddElectron();

//// Add services to the container.
//builder.Services.AddRazorComponents()
//    .AddInteractiveServerComponents();

//builder.Services.AddHttpContextAccessor();

//builder.Services.AddScoped<AuthService>();
//builder.Services.AddHttpClient<APIWebSiteService>(client =>
//{
//    client.BaseAddress = new Uri("https://facilys.flixmail.fr");
//});

//builder.Services.AddSingleton<PageTitleService>();
//builder.Services.AddScoped<VINDecoderService>();

//// Ajouter le DbContextFactory
//builder.Services.AddDbContextFactory<ApplicationDbContext>();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error", createScopeForErrors: true);
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}


//// Initialisez la base de donn�es avec l'utilisateur par d�faut
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    DbInitializer.Initialize(services);
//}

//app.UseHttpsRedirection();

//app.UseStaticFiles();
//app.UseAntiforgery();

//app.MapRazorComponents<Facilys.Components.App>()
//    .AddInteractiveServerRenderMode();

//// Cr�er la fen�tre Electron
//async Task CreateElectronWindow()
//{
//    // Cr�ation d'une nouvelle fen�tre avec les options sp�cifi�es
//    var window = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
//    {
//        Width = 1200,
//        Height = 800,
//        Show = true, // Affiche la fen�tre directement
//        //WebPreferences = new WebPreferences
//        //{
//        //    ContextIsolation = false, // Permet une meilleure interaction avec les scripts JS
//        //    DevTools = true           // Activez si besoin de d�boguer
//        //}
//    });

//    // Vider le cache et recharger
//    window.OnReadyToShow += () =>
//    {
//        var session = window.WebContents.Session;
//        session.ClearCacheAsync(); // Efface le cache
//    };

//    // Nettoyer le cache et recharger la fen�tre
//    await ClearCache();
//    ReloadWindow(window);

//    // Action lors de la fermeture de la fen�tre
//    window.OnClosed += () => Electron.App.Quit();
//}

//void ReloadWindow(BrowserWindow? mainWindow)
//{
//    mainWindow?.Reload();
//}

//async Task ClearCache()
//{
//    var browserWindows = Electron.WindowManager.BrowserWindows;

//    foreach (var window in browserWindows)
//    {
//        // Efface le cache de session pour chaque fen�tre
//        await window.WebContents.Session.ClearCacheAsync();
//    }
//}

//// V�rifiez si l'application s'ex�cute dans Electron
//if (HybridSupport.IsElectronActive)
//{
//    // Cr�e le dossier avant de lancer la fen�tre Electron
//    await AuthService.EnsureApplicationFolderExistsAsync();

//    await Task.Run(async () =>
//    {
//        await CreateElectronWindow();
//    });
//}

//app.Run();

using ElectronNET.API;
using ElectronNET.API.Entities;
using Facilys.Components.Data;
using Facilys.Components.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration Electron
builder.WebHost.UseElectron(args);
builder.Services.AddElectron();

// Configuration des services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

// Configuration de la base de donn�es
builder.Services.AddDbContextFactory<ApplicationDbContext>((services, options) =>
{
    var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    var facilysPath = Path.Combine(documentsPath, "Facilys", "Database");
    Directory.CreateDirectory(facilysPath);
    options.UseSqlite($"Data Source={Path.Combine(facilysPath, "data.db")}");
});

// Services personnalis�s
builder.Services.AddScoped<AuthService>();
builder.Services.AddHttpClient<APIWebSiteService>(client =>
{
    client.BaseAddress = new Uri("https://facilys.flixmail.fr");
});
builder.Services.AddSingleton<PageTitleService>();
builder.Services.AddScoped<VINDecoderService>();
builder.Services.AddSingleton<DynamicMySQLService>();

var app = builder.Build();

// Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<Facilys.Components.App>()
    .AddInteractiveServerRenderMode();

// Initialisation de la base de donn�es
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.EnsureCreatedAsync();
    DbInitializer.Initialize(scope.ServiceProvider);
}

// Gestion du mode Electron
if (HybridSupport.IsElectronActive)
{
    AuthService.EnsureApplicationFolderExists();

    await Task.Run(async () =>
    {
        await CreateElectronWindow();
    });

    await app.RunAsync(); // D�marrage ASP.NET Core apr�s cr�ation fen�tre
}
else
{
    await app.RunAsync();
}

async Task CreateElectronWindow()
{
    var mainWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
    {
        Width = 1200,
        Height = 800,
        Show = true,
        AutoHideMenuBar = true,
        Icon = "wwwroot/icon-256.ico",
        Title = "Facilys Application",
        WebPreferences = new WebPreferences
        {
            ContextIsolation = false
        }
    });

    mainWindow.OnReadyToShow += () =>
    {
        mainWindow.WebContents.Session.ClearCacheAsync();
    };

    await ClearCache();

    mainWindow.OnClosed += () =>
    {
        Electron.App.Quit();
    };

    Electron.App.WindowAllClosed += () =>
    {
        app.StopAsync().GetAwaiter().GetResult();
    };

    mainWindow.Reload();
}

async Task ClearCache()
{
    foreach (var window in Electron.WindowManager.BrowserWindows)
    {
        await window.WebContents.Session.ClearCacheAsync();
        await Task.Delay(100); // Pause pour la stabilit�
    }
}