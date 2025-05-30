//using ElectronNET.API;
//using ElectronNET.API.Entities;
//using Facilys.Components.Data;
//using Facilys.Components.Services;
//using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
//using Microsoft.EntityFrameworkCore;
//using System.Net.Http.Headers;

//var builder = WebApplication.CreateBuilder(args);

//// Configuration Electron
//builder.WebHost.UseElectron(args);
//builder.Services.AddElectron();

//// Configuration des services
//builder.Services.AddRazorComponents().AddInteractiveServerComponents();

//// Ajout de la gestion des sessions et du cache en mémoire
//builder.Services.AddDistributedMemoryCache();
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromDays(30); // Durée de session de 30 jours
//    options.Cookie.HttpOnly = true; // Empêche l'accès au cookie via JavaScript
//    options.Cookie.IsEssential = true; // Le cookie est essentiel pour l'application
//    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Force HTTPS
//});

//builder.Services.AddMemoryCache();
//builder.Services.AddHttpContextAccessor();

//builder.Services.AddScoped<UserConnectionService>();
//// Configuration du service SSH
//builder.Services.AddSingleton<SshTunnelService>();
//builder.Services.AddHostedService(provider => provider.GetRequiredService<SshTunnelService>());

//// Configuration flexible de la base de données
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
//}
//else
//{
//    // Mode Web - MariaDB principale avec connexion dynamique
//    builder.Services.AddScoped<IDbContextFactory<ApplicationDbContext>, DynamicDbContextFactory>();
//}


////// Configuration conditionnelle des bases de données
////if (HybridSupport.IsElectronActive)
////{
////    // Mode Electron - SQLite + MariaDB dynamique
////    builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
////    {
////        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
////        var facilysPath = Path.Combine(documentsPath, "Facilys", "Database");
////        Directory.CreateDirectory(facilysPath);
////        options.UseSqlite($"Data Source={Path.Combine(facilysPath, "data.db")}");
////    });

////    // Pour les connexions MariaDB secondaires
////    builder.Services.AddScoped<DynamicDbContextFactory>();
////}
////else
////{
////    // Mode Docker - MariaDB principale uniquement
////    var mariaDbConnection = builder.Configuration.GetConnectionString("MariaDB");
////    builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
////        options.UseMySql(
////            mariaDbConnection,
////            ServerVersion.AutoDetect(mariaDbConnection)
////        ));
////}


//// Services personnalisés
//builder.Services.AddScoped<ProtectedLocalStorage>();
//builder.Services.AddScoped<AuthService>();
//builder.Services.AddHttpClient<APIWebSiteService>(client =>
//{
//    client.BaseAddress = new Uri("https://facilys.flixmail.fr");
//});
//builder.Services.AddSingleton<PageTitleService>();
//builder.Services.AddScoped<VINDecoderService>();

//if (HybridSupport.IsElectronActive)
//{
//    builder.Services.AddHttpClient("SyncApi", client =>
//{
//    client.BaseAddress = new Uri("https://facilys.flixmail.fr");
//    client.DefaultRequestHeaders.Accept
//          .Add(new MediaTypeWithQualityHeaderValue("application/json"));
//});
//}
//builder.Services.AddScoped<SyncServiceAPISQL>();


//// Notre service d'export
//builder.Services.AddScoped<ExportService>();

//// Controllers
//builder.Services.AddControllers();

//var app = builder.Build();

//// Pipeline HTTP
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error", createScopeForErrors: true);
//    app.UseHsts();
//}

//app.UseRouting();
//app.MapControllers();
//app.UseSession();

//app.UseHttpsRedirection();
//app.UseStaticFiles();
//app.UseAntiforgery();
//app.UseWebSockets();

//if (HybridSupport.IsElectronActive)
//{
//    // Initialisation des bases de données
//    using (var scope = app.Services.CreateScope())
//    {
//        var services = scope.ServiceProvider;
//        var dbContext = services.GetRequiredService<ApplicationDbContext>();
//        await dbContext.Database.EnsureCreatedAsync();
//        //DbInitializer.Initialize(services); //test
//    }
//}

//app.MapRazorComponents<Facilys.Components.App>()
//    .AddInteractiveServerRenderMode();

//// Gestion du mode Electron
//if (HybridSupport.IsElectronActive)
//{
//    AuthService.EnsureApplicationFolderExists();

//    await Task.Run(async () =>
//    {
//        await CreateElectronWindow();
//    });

//    await app.RunAsync(); // Démarrage ASP.NET Core après création fenêtre
//}
//else
//{
//    await app.RunAsync();
//}

//async Task CreateElectronWindow()
//{
//    var mainWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
//    {
//        Width = 1200,
//        Height = 800,
//        Show = true,
//        AutoHideMenuBar = true,
//        Icon = "wwwroot/icon-256.ico",
//        Title = "Facilys Application",
//        WebPreferences = new WebPreferences
//        {
//            ContextIsolation = false
//        }
//    });

//    mainWindow.OnReadyToShow += () =>
//    {
//        mainWindow.WebContents.Session.ClearCacheAsync();
//    };

//    await ClearCache();

//    mainWindow.OnClosed += () =>
//    {
//        Electron.App.Quit();
//    };

//    Electron.App.WindowAllClosed += () =>
//    {
//        app.StopAsync().GetAwaiter().GetResult();
//    };

//    mainWindow.Reload();
//}

//async Task ClearCache()
//{
//    foreach (var window in Electron.WindowManager.BrowserWindows)
//    {
//        await window.WebContents.Session.ClearCacheAsync();
//        await Task.Delay(100); // Pause pour la stabilité
//    }
//}

using ElectronNET.API;
using ElectronNET.API.Entities;
using Facilys.Components.Data;
using Facilys.Components.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// SOLUTION CLOUD RUN: Configuration des ports dynamique
if (HybridSupport.IsElectronActive)
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        // Mode Electron - ports fixes locaux
        serverOptions.ListenLocalhost(5000, listenOptions =>
        {
            listenOptions.UseHttps(httpsOptions =>
            {
                httpsOptions.ServerCertificate = CreateSelfSignedCertificate();
            });
        });

        serverOptions.ListenLocalhost(5001, listenOptions =>
        {
            listenOptions.UseHttps(httpsOptions =>
            {
                httpsOptions.ServerCertificate = CreateSelfSignedCertificate();
            });
        });
        // serverOptions.ListenLocalhost(5100); // HTTP
        //serverOptions.ListenLocalhost(5001, listenOptions =>
        //{
        //    listenOptions.UseHttps(); // HTTPS
        //});
    });

    ServicePointManager.ServerCertificateValidationCallback =
        (sender, certificate, chain, sslPolicyErrors) => true;
    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
}
else
{
    // MODE CLOUD RUN: Configuration dynamique des ports
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    var urls = $"http://0.0.0.0:{port}";

    builder.WebHost.UseUrls(urls);

    // Configuration Kestrel pour Cloud Run
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ListenAnyIP(int.Parse(port), listenOptions =>
        {
            // Pas de HTTPS en mode Cloud Run (géré par le load balancer)
            listenOptions.UseConnectionLogging();
        });

        // Optimisations pour Cloud Run
        serverOptions.Limits.MaxConcurrentConnections = 100;
        serverOptions.Limits.MaxConcurrentUpgradedConnections = 100;
        serverOptions.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
        serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
        serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
    });
}


// Configuration Electron
builder.WebHost.UseElectron(args);
builder.Services.AddElectron();

// Configuration des services Blazor
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// OPTIMISATION: Gestion avancée des sessions et cache
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    if (HybridSupport.IsElectronActive)
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    else
        // Cloud Run gère HTTPS au niveau du load balancer
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

    options.Cookie.SameSite = SameSiteMode.Strict;
});

// OPTIMISATION: Configuration cache mémoire avec limites et éviction
builder.Services.AddMemoryCache(options =>
{
    var memoryLimit = Environment.GetEnvironmentVariable("MEMORY_LIMIT") ?? "500";
    options.SizeLimit = int.Parse(memoryLimit);
    options.CompactionPercentage = 0.20; // Compactage plus agressif
    options.TrackStatistics = true; // Pour monitoring
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserConnectionService>();

// OPTIMISATION: Service SSH avec gestion des connexions poolées
builder.Services.AddSingleton<OptimizedSshTunnelService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<OptimizedSshTunnelService>());

// OPTIMISATION: Configuration DbContext avec pooling et optimisations EF Core
if (HybridSupport.IsElectronActive)
{
    // Mode Electron - SQLite optimisé avec pooling
    builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var facilysPath = Path.Combine(documentsPath, "Facilys", "Database");
        Directory.CreateDirectory(facilysPath);

        options.UseSqlite($"Data Source={Path.Combine(facilysPath, "data.db")}")
               // Optimisations SQLite
               .EnableSensitiveDataLogging(false)
               .EnableServiceProviderCaching(true)
               .EnableDetailedErrors(false)
               .ConfigureWarnings(warnings => warnings.Ignore())
               .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    });

    // Factory pour connexions MariaDB dynamiques
    //builder.Services.AddScoped<IDbContextFactory<ApplicationDbContext>, OptimizedDynamicDbContextFactory>();
}
else
{
    builder.Services.AddScoped<IDbContextFactory<ApplicationDbContext>, OptimizedDynamicDbContextFactory>();
}

// Services personnalisés
builder.Services.AddScoped<EnvironmentApp>();
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<AuthService>();

// OPTIMISATION: HttpClient avec configuration réseau avancée
builder.Services.AddHttpClient<APIWebSiteService>(client =>
{
    client.BaseAddress = new Uri("https://facilys.flixmail.fr");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.ConnectionClose = false; // Keep-Alive
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
{
    MaxConnectionsPerServer = 20, // Augmenté pour parallélisme
    UseProxy = false, // Évite proxy si pas nécessaire
});

builder.Services.AddSingleton<PageTitleService>();
builder.Services.AddScoped<VINDecoderService>();

// OPTIMISATION: HttpClient pour synchronisation avec retry et circuit breaker
if (HybridSupport.IsElectronActive)
{
    builder.Services.AddHttpClient("SyncApi", client =>
    {
        client.BaseAddress = new Uri("https://facilys.flixmail.fr");
        client.Timeout = TimeSpan.FromSeconds(45); // Timeout plus long pour sync
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.ConnectionClose = false;
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
    {
        MaxConnectionsPerServer = 10,
        UseProxy = false,
    });
}

builder.Services.AddScoped<SyncServiceAPISQL>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddControllers();

// OPTIMISATION: Configuration logging pour performance
builder.Logging.ClearProviders();
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}
else
{
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
}

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; // Optional: enable for HTTPS
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});


var app = builder.Build();

// OPTIMISATION: Middleware optimisé
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// OPTIMISATION: Compression des réponses
app.UseResponseCompression();

app.UseRouting();
app.MapControllers();
app.UseSession();

app.UseHttpsRedirection();

// OPTIMISATION: Cache statique avec en-têtes optimisés
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache plus agressif en production (Cloud Run)
        var maxAge = app.Environment.IsDevelopment() ? 3600 : 31536000; // 1 heure vs 1 an
        ctx.Context.Response.Headers.CacheControl = $"public,max-age={maxAge}";

        if (!app.Environment.IsDevelopment())
        {
            ctx.Context.Response.Headers.Expires = DateTime.UtcNow.AddYears(1).ToString("R");
        }
    }
});

app.UseAntiforgery();
app.UseWebSockets();

// OPTIMISATION: Initialisation asynchrone et différée des bases de données
if (HybridSupport.IsElectronActive)
{
    // Démarrage en arrière-plan sans bloquer l'application
    _ = Task.Run(async () =>
    {
        try
        {
            await Task.Delay(1000); // Petite attente pour laisser l'app démarrer
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Initialisation de la base de données SQLite...");
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            // OPTIMISATION: Pré-chargement des données critiques en cache
            await PreloadCriticalData(services);

            logger.LogInformation("Base de données initialisée avec succès");
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetService<ILogger<Program>>();
            logger?.LogError(ex, "Erreur lors de l'initialisation de la base de données");
        }
    });
}

app.MapRazorComponents<Facilys.Components.App>()
    .AddInteractiveServerRenderMode();

// Gestion du mode Electron avec optimisations
if (HybridSupport.IsElectronActive)
{
    AuthService.EnsureApplicationFolderExists();

    // OPTIMISATION: Création fenêtre en arrière-plan pour démarrage plus rapide
    var windowCreationTask = Task.Run(async () =>
    {
        try
        {
            await CreateOptimizedElectronWindow();
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetService<ILogger<Program>>();
            logger?.LogError(ex, "Erreur lors de la création de la fenêtre Electron");
        }
    });

    // Démarrage de l'application sans attendre la fenêtre
    var appTask = app.RunAsync();

    // Attendre que les deux tâches se terminent
    await Task.WhenAll(windowCreationTask, appTask);
}
else
{
    await app.RunAsync();
}

// Création d'un certificat auto-signé pour ElectronNET
static X509Certificate2 CreateSelfSignedCertificate()
{
    using var rsa = System.Security.Cryptography.RSA.Create(2048);
    var req = new System.Security.Cryptography.X509Certificates.CertificateRequest(
        "CN=localhost", rsa, System.Security.Cryptography.HashAlgorithmName.SHA256,
        System.Security.Cryptography.RSASignaturePadding.Pkcs1);

    // Construction correcte du Subject Alternative Name pour .NET 8
    var sanBuilder = new System.Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder();
    sanBuilder.AddDnsName("localhost");
    sanBuilder.AddIpAddress(System.Net.IPAddress.Loopback); // 127.0.0.1
    sanBuilder.AddIpAddress(System.Net.IPAddress.IPv6Loopback); // ::1

    req.CertificateExtensions.Add(sanBuilder.Build());

    // Ajouter l'extension Key Usage
    req.CertificateExtensions.Add(new System.Security.Cryptography.X509Certificates.X509KeyUsageExtension(
        System.Security.Cryptography.X509Certificates.X509KeyUsageFlags.DigitalSignature |
        System.Security.Cryptography.X509Certificates.X509KeyUsageFlags.KeyEncipherment, false));

    // Ajouter l'extension Enhanced Key Usage pour TLS Server
    req.CertificateExtensions.Add(new System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension(
        new System.Security.Cryptography.OidCollection
        {
            new System.Security.Cryptography.Oid("1.3.6.1.5.5.7.3.1") // Server Authentication
        }, false));

    var cert = req.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(1));

    return new X509Certificate2(cert.Export(X509ContentType.Pfx), "", X509KeyStorageFlags.Exportable);
}

// OPTIMISATION: Création fenêtre Electron optimisée
async Task CreateOptimizedElectronWindow()
{
    var mainWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
    {
        Width = 1200,
        Height = 800,
        Show = false, // Ne pas afficher immédiatement
        AutoHideMenuBar = false,
        Icon = "wwwroot/icon-256.ico",
        Title = "Facilys Application",
        WebPreferences = new WebPreferences
        {
            ContextIsolation = false,
            NodeIntegration = false, // Sécurité
            EnableRemoteModule = false, // Performance
        },
        // OPTIMISATION: Options de performance
        UseContentSize = true,
    });
    mainWindow.LoadURL("https://localhost:5001");

    // OPTIMISATION: Gestionnaires d'événements optimisés
    mainWindow.OnReadyToShow += async () =>
    {
        try
        {
            // Nettoyage cache avant affichage
            await mainWindow.WebContents.Session.ClearCacheAsync();

            // Maintenant on peut afficher la fenêtre
            mainWindow.Show();
            mainWindow.Focus();
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetService<ILogger<Program>>();
            logger?.LogError(ex, "Erreur lors de l'affichage de la fenêtre");
        }
    };

    mainWindow.OnClosed += () =>
    {
        try
        {
            Electron.App.Quit();
        }
        catch
        {
            // Ignorer les erreurs de fermeture
        }
    };

    Electron.App.WindowAllClosed += () =>
    {
        try
        {
            app.StopAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // Ignorer les erreurs d'arrêt
        }
    };

    // OPTIMISATION: Rechargement optimisé
    await Task.Delay(500); // Laisser le temps à la fenêtre de se stabiliser
    mainWindow.Reload();
}

// OPTIMISATION: Pré-chargement des données critiques
async Task PreloadCriticalData(IServiceProvider services)
{
    try
    {
        var cache = services.GetRequiredService<IMemoryCache>();
        var dbContextFactory = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

        using var context = await dbContextFactory.CreateDbContextAsync();

        // Exemple : pré-charger les données de configuration
        // var configs = await context.Configurations.AsNoTracking().ToListAsync();
        // cache.Set("app_configurations", configs, TimeSpan.FromHours(1));

        // Exemple : pré-charger les utilisateurs actifs
        // var activeUsers = await context.Users.AsNoTracking().Where(u => u.IsActive).ToListAsync();
        // cache.Set("active_users", activeUsers, TimeSpan.FromMinutes(30));
    }
    catch (Exception ex)
    {
        var logger = services.GetService<ILogger<Program>>();
        logger?.LogWarning(ex, "Erreur lors du pré-chargement des données critiques");
    }
}