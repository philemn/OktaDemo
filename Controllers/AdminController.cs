using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Okta.Sdk;
using OktaDemo.Models;

namespace OktaDemo.Controllers;

//[Authorize(Roles = "admin")]
public class AdminController : Controller
{
    private static IList<ApplicationModel>? Applications;
    private readonly string _adminGroupId;


    private readonly IOktaClient _client;
    private readonly ILogger<HomeController> _logger;

    public AdminController(IOktaClient oktaClient, ApplicationTemplate appTemplate, ILogger<HomeController> logger)
    {
        _client = oktaClient;
        _adminGroupId = appTemplate.AdminGroupId;
        _logger = logger;
    }

    private async Task<IList<ApplicationModel>> EnsureApplications()
    {
        if (Applications is null)
        {
            var applications = new List<ApplicationModel>();
            foreach (var application in await _client.Applications.ListApplications().ToListAsync())
            {
                if (application.Label.StartsWith("Okta "))
                {
                    continue;
                }

                var model = new ApplicationModel();
                model.Id = application.Id;
                model.Name = application.Label;

                var applicationUserSchema = await _client.UserSchemas.GetApplicationUserSchemaAsync(application.Id);
                var customSchema = applicationUserSchema.Definitions.Custom;

                foreach (var property in customSchema.Properties.GetData())
                {
                    var data = (IEnumerable<KeyValuePair<string, object>>)property.Value;
                    var dict = new Dictionary<string, object>(data);

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

    public async Task<IActionResult> Index()
    {
        var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? "notfound";

        var users = await _client.Users.ListUsers().ToListAsync();

        var model = new AdminModel(users, userId);

        return View(model);
    }

    public async Task<IActionResult> Users(string id)
    {
        var applications = await EnsureApplications();

        var userId = id;
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

            model.Applications.Add(appUserModel);
        }

        return View(model);
    }

    public IActionResult AddUser()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddUser([Required]string firstName, [Required]string lastName,
        [Required]string city, [Required]string state, [Required]string email, [Required]string memberId, bool admin)
    {
        IUser? createdUser = null;

        if (!ModelState.IsValid)
        {
            return View();
        }

        try
        {
            var profile = new UserProfile
            {
                Login = email,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                City = city,
                State = state
            };
            profile.SetProperty("memberId", memberId);

            createdUser = await _client.Users.CreateUserAsync(new CreateUserRequest
            {
                GroupIds = admin ? new List<string> { _adminGroupId } : new List<string> { },
                Profile = profile
            }, true);

            if (createdUser == null)
            {
                ModelState.AddModelError("", "Error encountered while attempting to create user");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.ToString());
        }

        if (!ModelState.IsValid)
        {
            return View();
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _client.Users.GetUserAsync(id);
        var admin = await user.Groups.AnyAsync(x => x.Id == _adminGroupId);

        if (user == null)
        {
            return NotFound();
        }

        var model = new EditUserModel(user, admin);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(string id, string firstName, string lastName, string city, string state, bool admin)
    {
        var user = await _client.Users.GetUserAsync(id);
        user.Profile.FirstName = firstName;
        user.Profile.LastName = lastName;
        user.Profile.City = city;
        user.Profile.State = state;

        await _client.Users.PartialUpdateUserAsync(user, id);

        var currentlyAdmin = await user.Groups.AnyAsync(x => x.Id == _adminGroupId);
        if (currentlyAdmin && !admin)
        {
            await _client.Groups.RemoveUserFromGroupAsync(_adminGroupId, id);
        }
        else if (!currentlyAdmin && admin)
        {
            await _client.Groups.AddUserToGroupAsync(_adminGroupId, id);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<ActionResult> RemoveUser(string id)
    {
        try
        {
            await _client.Users.DeactivateOrDeleteUserAsync(id);
            await _client.Users.DeactivateOrDeleteUserAsync(id);
        }
        catch (Exception)
        {
        }
        
        return RedirectToAction("Index");
    }
}
