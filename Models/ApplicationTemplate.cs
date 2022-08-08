namespace OktaDemo.Models;

public class ApplicationTemplate
{
    public string ApplicationName { get; set; } = "OktaDemo";
    public string Presenter { get; set; } = "Okta, Inc.";
    public string AdminGroupId { get; set; } = "";
    public string[] OktaApplications { get; set; } = new string[] {};
    public string[] FullProfileOktaApplications { get; set; } = new string[] {};
}