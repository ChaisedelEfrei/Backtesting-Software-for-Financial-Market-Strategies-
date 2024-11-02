using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing.Internal.Patterns;
using Microsoft.Extensions.Hosting;
using YourNamespace.Classes;


var builder = WebApplication.CreateBuilder(args);

// Ajouter le service HttpClient pour l'injection
builder.Services.AddHttpClient<AlphaVantageService>();
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();


app.MapControllerRoute(
    name : "default",
    pattern : "{controller=Home}/{action=Index}/{id?}"
); // Permet de mapper le contrôleur API



app.Run();
