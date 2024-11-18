using Facilys.Components;
using ElectronNET.API;
using Facilys.Components.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuer Electron
builder.WebHost.UseElectron(args);
builder.Services.AddElectron();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<AuthService>();


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

// Créer la fenêtre Electron
async Task CreateElectronWindow()
{
    var window = await Electron.WindowManager.CreateWindowAsync();
    window.OnClosed += () => Electron.App.Quit();
}

// Vérifiez si l'application s'exécute dans Electron
if (HybridSupport.IsElectronActive)
{
    Task.Run(async () => {
        var window = await Electron.WindowManager.CreateWindowAsync();
        window.OnClosed += () => {
            Electron.App.Quit();
        };
    });
}

app.Run();