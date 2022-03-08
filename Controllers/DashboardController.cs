using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Okta.Sdk;
using OktaDemo.Hubs;
using OktaDemo.Models;

namespace OktaDemo.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IOktaClient _client;
    private readonly ILogger<HomeController> _logger;

    public DashboardController(IOktaClient oktaClient, ILogger<HomeController> logger)
    {
        _client = oktaClient;
        _logger = logger;
    }

    public ActionResult Index()
    {
        return View(DashboardHub.Current);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
