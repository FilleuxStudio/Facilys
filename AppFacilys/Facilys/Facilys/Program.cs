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


//// Initialisez la base de données avec l'utilisateur par défaut
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

//// Créer la fenêtre Electron
//async Task CreateElectronWindow()
//{
//    // Création d'une nouvelle fenêtre avec les options spécifiées
//    var window = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
//    {
//        Width = 1200,
//        Height = 800,
//        Show = true, // Affiche la fenêtre directement
//        //WebPreferences = new WebPreferences
//        //{
//        //    ContextIsolation = false, // Permet une meilleure interaction avec les scripts JS
//        //    DevTools = true           // Activez si besoin de déboguer
//        //}
//    });

//    // Vider le cache et recharger
//    window.OnReadyToShow += () =>
//    {
//        var session = window.WebContents.Session;
//        session.ClearCacheAsync(); // Efface le cache
//    };

//    // Nettoyer le cache et recharger la fenêtre
//    await ClearCache();
//    ReloadWindow(window);

//    // Action lors de la fermeture de la fenêtre
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
//        // Efface le cache de session pour chaque fenêtre
//        await window.WebContents.Session.ClearCacheAsync();
//    }
//}

//// Vérifiez si l'application s'exécute dans Electron
//if (HybridSupport.IsElectronActive)
//{
//    // Crée le dossier avant de lancer la fenêtre Electron
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
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration Electron
builder.WebHost.UseElectron(args);
builder.Services.AddElectron();

// Configuration des services
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Ajout de la gestion des sessions et du cache en mémoire
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(30); // Durée de session de 30 jours
    options.Cookie.HttpOnly = true; // Empêche l'accès au cookie via JavaScript
    options.Cookie.IsEssential = true; // Le cookie est essentiel pour l'application
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Force HTTPS
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<UserConnectionService>();
// Configuration du service SSH
builder.Services.AddSingleton<SshTunnelService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<SshTunnelService>());

// Configuration flexible de la base de données
if (HybridSupport.IsElectronActive)
{
    // Mode Electron - SQLite + MariaDB dynamique
    builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var facilysPath = Path.Combine(documentsPath, "Facilys", "Database");
        Directory.CreateDirectory(facilysPath);
        options.UseSqlite($"Data Source={Path.Combine(facilysPath, "data.db")}");
    });
}
else
{
    // Mode Web - MariaDB principale avec connexion dynamique
    builder.Services.AddScoped<IDbContextFactory<ApplicationDbContext>, DynamicDbContextFactory>();
}


//// Configuration conditionnelle des bases de données
//if (HybridSupport.IsElectronActive)
//{
//    // Mode Electron - SQLite + MariaDB dynamique
//    builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
//    {
//        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
//        var facilysPath = Path.Combine(documentsPath, "Facilys", "Database");
//        Directory.CreateDirectory(facilysPath);
//        options.UseSqlite($"Data Source={Path.Combine(facilysPath, "data.db")}");
//    });

//    // Pour les connexions MariaDB secondaires
//    builder.Services.AddScoped<DynamicDbContextFactory>();
//}
//else
//{
//    // Mode Docker - MariaDB principale uniquement
//    var mariaDbConnection = builder.Configuration.GetConnectionString("MariaDB");
//    builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
//        options.UseMySql(
//            mariaDbConnection,
//            ServerVersion.AutoDetect(mariaDbConnection)
//        ));
//}


// Services personnalisés
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddHttpClient<APIWebSiteService>(client =>
{
    client.BaseAddress = new Uri("https://facilys.flixmail.fr");
});
builder.Services.AddSingleton<PageTitleService>();
builder.Services.AddScoped<VINDecoderService>();
//builder.Services.AddSingleton<DynamicMySQLService>();

// Notre service d'export
builder.Services.AddScoped<ExportService>();

// Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseRouting();
app.MapControllers();
app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseWebSockets();

if (HybridSupport.IsElectronActive)
{
    // Initialisation des bases de données
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        DbInitializer.Initialize(services);
    }
}

app.MapRazorComponents<Facilys.Components.App>()
    .AddInteractiveServerRenderMode();

// Gestion du mode Electron
if (HybridSupport.IsElectronActive)
{
    AuthService.EnsureApplicationFolderExists();

    await Task.Run(async () =>
    {
        await CreateElectronWindow();
    });

    await app.RunAsync(); // Démarrage ASP.NET Core après création fenêtre
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
        await Task.Delay(100); // Pause pour la stabilité
    }
}