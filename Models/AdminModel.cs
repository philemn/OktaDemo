using Okta.Sdk;

namespace OktaDemo.Models;

public class AdminModel
{
    public List<User> Admins { get; } = new List<User>();
    public List<User> Users { get; } = new List<User>();

    public AdminModel(IList<IUser> users, string currentUserId)
    {
        foreach (var user in users)
        {
            if (user.Profile.GetArrayProperty<string>("applicationRoles").Any(x => x == "admin"))
            {
                Admins.Add(new User(user, currentUserId));
            }
            else
            {
                Users.Add(new User(user, currentUserId));
            }
        }
    }

    public class User
    {
        static Random Random = new();

        public string Id { get; }
        public string ProfileId { get; }
        public string EmployeeNumber { get; }
        public string Name { get; }
        public string Email { get; }
        public string State { get; }
        public string City { get; }
        public UserStatus Status { get; }
        public DateTimeOffset? LastUpdated { get; } 
        public bool IsCurrentUser { get; }
        
        public User(IUser user, string currentUserId)
        {
            var profile = user.Profile;
            var employeeNumber = profile.GetProperty<string>("employeeNumber");

            Id = user.Id;
            ProfileId = Random.Next(2, 5).ToString();
            EmployeeNumber = string.IsNullOrEmpty(employeeNumber) ? Random.Next(13000, 26000).ToString() : employeeNumber.Trim();
            Name = profile.GetProperty<string>("firstName") + " " + profile.GetProperty<string>("lastName");
            Email = profile.GetProperty<string>("email");
            State = string.IsNullOrEmpty(profile.GetProperty<string>("locality")) ? "Unknown" : profile.GetProperty<string>("locality");
            City = profile.GetProperty<string>("region");
            LastUpdated = user.LastUpdated;
            Status = user.Status;
            IsCurrentUser = user.Id == currentUserId;
        }
    }
}