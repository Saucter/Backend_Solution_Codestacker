using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using PDF_Reader_APIs.Server;
using PDF_Reader_APIs.Server.AzureStorageServices;
using PDF_Reader_APIs.Server.Caching;
using PDF_Reader_APIs.Server.Authentication;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<Database>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); //Add daabase connection string
builder.Services.AddScoped<IAzureFileStorageService, AzureFileStorageService>(); //Add service DI
builder.Services.AddScoped<ICacheRepository, CacheRepository>(); //Add caching DI
builder.Services.AddScoped<IUserRepository, UserRepository>(); //Add authentication DI
builder.Services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null); //Enable basic authentication
builder.Services.AddMemoryCache(); //Enable caching

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization(); //Emable authentication

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
