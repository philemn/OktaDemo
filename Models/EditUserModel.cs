using Okta.Sdk;

namespace OktaDemo.Models;

public class EditUserModel
{
    static Random Random = new();

    public string? Id { get; }
    public string? EmployeeNumber { get; }
    public string? FirstName { get; }
    public string? LastName { get; }
    public string? Email { get; }
    public string? State { get; }
    public string? City { get; }
    public string? MemberId { get; }
    public UserStatus? Status { get; }
    public bool Admin { get; }
    
    public EditUserModel(){}

    public EditUserModel(IUser user, bool admin)
    {
        var profile = user.Profile;
        Id = user.Id;
        EmployeeNumber = string.IsNullOrEmpty(profile.EmployeeNumber) ? Random.Next(2, 5).ToString() : profile.EmployeeNumber.Trim();
        FirstName = profile.FirstName;
        LastName = profile.LastName;
        Email = profile.Email;
        State = profile.State;
        City = profile.City;
        MemberId = profile.GetProperty<string>("memberId");
        Status = user.Status;
        Admin = admin;
    }
}