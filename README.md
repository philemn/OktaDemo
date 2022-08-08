# OktaDemo
OktaDemo is an example administrative application which leverages Okta's .NET SDK.

## Instructions for use ...
Add a file named `appsettings.Development.json` to the root directory.  Replace the values in angle brackets:
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Template": {
    "ApplicationName": "<YOUR APPLICATION NAME>",
    "Presenter": "<YOUR NAME>",
    "AdminGroupId": "<ID OF THE GROUP ID FOR ADMINS | optional>",
    "OktaApplications": [
      <... list of application names ...>
    ],
    "FullProfileOktaApplications": [
      <... list of application names ...>
    ]
  },
  "Okta": {
    "OktaDomain": "https://<TENANT NAME>.okta.com",
    "ClientId": "<CLIENT ID>",
    "ClientSecret": "<CLIENT SECRET>",
    "AuthorizationServerId": "default",
    "Token": "<API TOKEN>"
  }
}

```
