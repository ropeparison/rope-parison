using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using RopeParison.Logic;
using RopeParison.Logic.Services;
using RopeParison.Data;
using RopeParison.Data.Services;
using Syncfusion.Blazor;
using Serilog;
using RopeParison.Security;
using RopeParison.GraphHelpers;

var builder = WebApplication.CreateBuilder(args);

var dataConnectionString = builder.Configuration.GetConnectionString("dataConnection") ?? throw new NullReferenceException("No dataConnection string in config");
var securityConnectionString = builder.Configuration.GetConnectionString("securityConnection") ?? throw new NullReferenceException("No securityConnection string in config");

//--------------------------------------------------------------------
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSyncfusionBlazor();
//Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBaFt8QHFqVkBrXVNbdV5dVGpAd0N3RGlcdlR1fUUmHVdTRHRcQ11hTn5XdE1jX39cd3Y=;Mgo+DSMBMAY9C3t2VlhhQlJCfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn9TdEJjXX1WcHJVQmJU;Mjc4MzI1OUAzMjMzMmUzMDJlMzBIOW8weHZCaFRFcXBXV1YrSVRGSXZsUmVWejllV0xHVlVNRTAremFua0xRPQ==");
//Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NAaF5cWWJCf0x0Qnxbf1x0ZFZMYFxbQXBPIiBoS35RckViW39fdXRVQ2ZUWUx3");
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzA2NzAzMkAzMjM0MmUzMDJlMzBaNm9oQjN3K290bVV6S1dnbmhiZHdUN0xWMkwxRThrczg3L2RzWTdkSC9jPQ==");


builder.Services.AddTransient<IRopeDataService, RopeDataService>();
builder.Services.AddTransient<IRopeCalculatedParameterSetDataService, RopeCalculatedParameterSetDataService>();
builder.Services.AddTransient<IRopeEditSuggestionDataService, RopeEditSuggestionDataService>();
builder.Services.AddTransient<IBrandDataService, BrandDataService>();
builder.Services.AddTransient<ICategoryDataService, CategoryDataService>();

builder.Services.AddTransient<IRopeService, RopeService>();
builder.Services.AddTransient<IBrandService, BrandService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();

builder.Services.AddTransient<IPasswordSecurityService, PasswordSecurityService>();
builder.Services.AddTransient<IPasswordService, PasswordService>();

builder.Services.AddTransient<IGraphService, GraphService>();

builder.Services.AddDbContextFactory<DataContext>((DbContextOptionsBuilder options) => options.UseSqlServer(dataConnectionString));
builder.Services.AddDbContextFactory<SecurityContext>((DbContextOptionsBuilder options) => options.UseSqlServer(securityConnectionString));
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
