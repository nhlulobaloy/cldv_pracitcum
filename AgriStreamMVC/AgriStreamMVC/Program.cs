using AgriStreamMVC.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Set the port for Azure Linux
var port = Environment.GetEnvironmentVariable("WEBSITES_PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// 1. Add Database Context (SQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add HTTP Client (for calling Azure Functions)
builder.Services.AddHttpClient();

// 3. Add MVC Services
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Set the default route to go directly to the Dashboard
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();