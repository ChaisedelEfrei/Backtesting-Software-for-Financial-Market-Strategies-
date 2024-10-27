using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YourNamespace.Classes;


var builder = WebApplication.CreateBuilder(args);

// Ajouter le service HttpClient pour l'injection
builder.Services.AddHttpClient<AlphaVantageService>();
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers(); // Permet de mapper le contrôleur API

app.Run();
