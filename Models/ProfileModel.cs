using Okta.Sdk;

namespace OktaDemo.Models;

public class ProfileModel
{
    static Random Random = new();

    public string? Id { get; }
    public string? ProfileId { get; }
    public string? FirstName { get; }
    public string? LastName { get; }
    public string? Email { get; }
    public string? SecondEmail { get; }
    public string? Title { get; }
    public string? Department { get; }
    public string? State { get; }
    public string? City { get; }
    public string? PhoneNumber { get; }
    public string? FacebookUrl { get; }
    public string? TwitterUrl { get; }
    public string? Description { get; }
    public string? EmployeeNumber { get; }
    public DateTimeOffset? LastLogin { get; } 
    public bool EmailNewsletter { get; }
    public bool EmailUpdates { get; }
    public bool EmailFeatures { get; }
    public string AccountManagerName { get; }
    public string AccountManagerPhoneNumber { get; }
    public string AccountManagerEmail { get; }
    public string? NetworkId { get; }
    public string? FavoriteIceCreamFlavor { get;}

    public List<ApplicationUserModel> Applications { get; } = new();
    
    public ProfileModel()
    {
        AccountManagerName = "";
        AccountManagerPhoneNumber = "";
        AccountManagerEmail = "";
    }

    public ProfileModel(IUser user)
    {
        var profile = user.Profile;

        Id = user.Id;
        ProfileId = Random.Next(2, 5).ToString();
        FirstName = profile.FirstName;
        LastName = profile.LastName;
        Email = profile.Email;
        SecondEmail = profile.SecondEmail;
        Title = profile.Title;
        Department = profile.Department;
        State = profile.State;
        City = profile.City;
        PhoneNumber = profile.MobilePhone;
        FacebookUrl = profile.GetProperty<string>("facebookUrl");
        TwitterUrl = profile.GetProperty<string>("twitterUrl");
        EmailNewsletter = false;
        EmailUpdates = false;
        EmailFeatures = false;

        NetworkId = profile.GetProperty<string>("network_id");
        FavoriteIceCreamFlavor = profile.GetProperty<string>("favorite_ice_cream");

        AccountManagerName = "";
        AccountManagerPhoneNumber = "";
        AccountManagerEmail = "";

        EmployeeNumber = string.IsNullOrEmpty(profile.EmployeeNumber) ? Random.Next(2, 5).ToString() : profile.EmployeeNumber.Trim();
    }
}

public class ApplicationUserModel
{
    public string ApplicationId { get; set; } = "";
    public string ApplicationName { get; set; } = "";
    public Dictionary<string, string> Profile { get; } = new();
}

public class ApplicationModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public List<ApplicationProfilePropertyModel> Properties { get; } = new();
}

public class ApplicationProfilePropertyModel
{
    public string Id { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Type { get; set; } = "";
}