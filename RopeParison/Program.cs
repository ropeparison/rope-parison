using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using RopeParison.Logic;
using RopeParison.Logic.Services;
using RopeParison.Data;
using RopeParison.Data.Services;
using Syncfusion.Blazor;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var defaultConnectionString = builder.Configuration.GetConnectionString("defaultConnection") ?? throw new NullReferenceException("No connection string in config");

//--------------------------------------------------------------------
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSyncfusionBlazor();


builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddTransient<IRopeDataService, RopeDataService>();
builder.Services.AddTransient<IRopeCalculatedParameterSetDataService, RopeCalculatedParameterSetDataService>();
builder.Services.AddTransient<IBrandDataService, BrandDataService>();
builder.Services.AddTransient<ICategoryDataService, CategoryDataService>();

builder.Services.AddTransient<IRopeService, RopeService>();
builder.Services.AddTransient<IBrandService, BrandService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();

builder.Services.AddDbContextFactory<Context>((DbContextOptionsBuilder options) => options.UseSqlServer(defaultConnectionString));
//--------------------------------------------------------------------

//-------------------
//Log
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();
Log.Information("Application starting.");
//-------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
