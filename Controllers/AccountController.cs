using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Okta.AspNetCore;
using Okta.Sdk;
using OktaDemo.Models;

namespace OktaDemo.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly IOktaClient _client;
    private readonly ILogger<HomeController> _logger;
    private static IList<ApplicationModel>? Applications;
    private static HashSet<string> ExcludedApplicationIds = new() { "0oagurdstB0048P2q696", "0oagurduuiytWTXta696", "0oagurdvgPA9k3E5a696", "0oaijmxvkxyOxY47L696", "0oaguvbt3UFdMBrgw696", "0oaguvbuySe9fcPQs696" };
    private static HashSet<string> ExcludedUserTypeIds = new() { "otygurdtbfAWrNOkA696", "otyguvbt8Keccnf3a696", "otyguvbv3fBlRdWb9696", "otyijmxvqimETOHc4696" };

    public AccountController(IOktaClient oktaClient, ILogger<HomeController> logger)
    {
        _client = oktaClient;
        _logger = logger;
    }

    private async Task<IList<ApplicationModel>> EnsureApplications()
    {
        if (Applications is null)
        {
            var applications = new List<ApplicationModel>();
            foreach (var application in await _client.Applications.ListApplications().ToListAsync())
            {
                if (ExcludedApplicationIds.Contains(application.Id))
                {
                    continue;
                }

                var model = new ApplicationModel();
                model.Id = application.Id;
                model.Name = application.Name;

                var applicationUserSchema = await _client.UserSchemas.GetApplicationUserSchemaAsync(application.Id);
                var customSchema = applicationUserSchema.Definitions.Custom;

                foreach (var property in customSchema.Properties.GetData())
                {
                    var data = (IEnumerable<KeyValuePair<string, object>>)property.Value;
                    var dict = new Dictionary<string, object>(data);

                    dynamic prop = property.Value;
                    model.Properties.Add(new ApplicationProfilePropertyModel
                    {
                        DisplayName = (string)dict["title"],
                        Id = property.Key
                    });
                }

                applications.Add(model);
            }

            Applications = applications;
        }

        return Applications;
    }

    public async Task<IActionResult> Profile()
    {
        var applications = await EnsureApplications();

        var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        var user = await _client.Users.GetUserAsync(userId);

        var model = new ProfileModel(user);

        foreach (var application in applications)
        {
            IAppUser appUser;

            try
            {
                appUser = await _client.Applications.GetApplicationUserAsync(application.Id, userId);
            }
            catch (OktaException)
            {
                continue;
            }

            var appUserModel = new ApplicationUserModel();
            appUserModel.ApplicationId = application.Id;
            appUserModel.ApplicationName = application.Name;

            foreach (var property in application.Properties)
            {
                var value = appUser.Profile.GetProperty<string>(property.Id);
                appUserModel.Profile.Add(property.DisplayName, value);
            }
        }

        var managerId = user.Profile.GetProperty<string>("managerId");
        var manager = !string.IsNullOrEmpty(managerId)
            ? await _client.Users.GetUserAsync(managerId)
            : null;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Profile(bool emailNewsletter, bool emailUpdates, bool emailFeatures, string phoneNumber)
    {
        var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        var user = await _client.Users.GetUserAsync(userId);
        var appUser = await _client.Applications.GetApplicationUserAsync("0oaijmxvkxyOxY47L696", userId);

        _logger.LogInformation($"Updating User: Newsletter ({emailNewsletter}), Updates ({emailUpdates}), Features ({emailFeatures}), Phone: ({phoneNumber})");
        if (appUser != null)
        {
            appUser.Profile["email_Subscribed"] = emailNewsletter;
            appUser.Profile["email_Updates"] = emailUpdates;
            appUser.Profile["email_Features"] = emailFeatures;

            var result = await _client.Applications.UpdateApplicationUserAsync(appUser, "0oaijmxvkxyOxY47L696", userId);
        }

        if (user != null)
        {
            user.Profile.MobilePhone = phoneNumber;
            await _client.Users.PartialUpdateUserAsync(user, userId, false);
        }

        return RedirectToAction("Profile");
    }

    public IActionResult Login(string redirectUrl = "")
    {
        if (User.Identity != null && !User.Identity.IsAuthenticated)
        {
            return Challenge(OktaDefaults.MvcAuthenticationScheme);
        }

        if (redirectUrl.IndexOf("://") >= 0)
        {
            redirectUrl = redirectUrl.Substring(redirectUrl.IndexOf("/", redirectUrl.IndexOf("://")));
        }
        else
        {
            redirectUrl = "/Dashboard";
        }

        return Redirect(redirectUrl);
    }

    public ActionResult Logout()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return new SignOutResult(
                new[]
                {
                     OktaDefaults.MvcAuthenticationScheme,
                     CookieAuthenticationDefaults.AuthenticationScheme,
                },
                new AuthenticationProperties { RedirectUri = "/" });
        }
        
        return Redirect("/");
    }

    public async Task<ActionResult> Debug()
    {
        var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        var appUser = await _client.Applications.GetApplicationUserAsync("0oa2r9xka7KsPpy5H5d7", userId);

        return View(appUser);
    }
}
