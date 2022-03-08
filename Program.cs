using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Okta.AspNetCore;
using Okta.Sdk;
using Okta.Sdk.Configuration;
using OktaDemo.Hubs;
using OktaDemo.Models;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

var configuration = builder.Configuration;
var oktaMvcOptions = configuration.GetSection("Okta").Get<OktaMvcOptions>();
var applicationTemplate = configuration.GetSection("Template").Get<ApplicationTemplate>();

// Add services to the container.
services.AddSingleton(applicationTemplate);
services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddAuthentication(options => 
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    var events = new OpenIdConnectEvents();
    OpenIdConnectOptionsHelper.ConfigureOpenIdConnectOptions(oktaMvcOptions, events, options);
    options.CallbackPath = "/authorization-code/callback";
    options.SignedOutCallbackPath = "/signout/callback";
    //options.TokenValidationParameters.RoleClaimType = "applicationRoles";
    options.SaveTokens = true;

    options.GetClaimsFromUserInfoEndpoint = true;
    options.UsePkce = true;
    options.UseTokenLifetime = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.ResponseType = "code id_token";
});

services.AddControllersWithViews();
services.AddSignalR();

var oktaClientConfiguration = configuration.GetSection("Okta").Get<OktaClientConfiguration>();
services.AddScoped<IOktaClient>(x => new OktaClient(oktaClientConfiguration));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<DashboardHub>("/signalr/dashboard");

app.Run();
