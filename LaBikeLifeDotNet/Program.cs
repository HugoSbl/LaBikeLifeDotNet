using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LaBikeLifeDotNet.Data;
using LaBikeLifeDotNet.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// l'API principale : vPIC, gratuite et sans clé, pour les marques/modèles et le décodage VIN
builder.Services.AddHttpClient<INhtsaVpicService, NhtsaVpicService>(client =>
    client.BaseAddress = new Uri("https://vpic.nhtsa.dot.gov/api/"));

// Enregistrement du client HTTP typé dédié au service d'imagerie Wikipedia (API MediaWiki publique,
// gratuite et exempte de clé d'authentification). Conformément à la politique d'étiquette de la
// fondation Wikimedia, un en-tête User-Agent descriptif et identifiant est impérativement renseigné :
// son omission expose les requêtes à un rejet (HTTP 403) par l'infrastructure distante.
builder.Services.AddHttpClient<IWikipediaImageService, WikipediaImageService>(client =>
{
    client.BaseAddress = new Uri("https://en.wikipedia.org/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("LaBikeLifeDotNet/1.0 (educational project)");
});

// le calcul des entretiens, fait maison (y'a pas d'API gratuite pour ça)
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();