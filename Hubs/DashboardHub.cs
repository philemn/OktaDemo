using Microsoft.AspNetCore.SignalR;
using Okta.Sdk;

namespace OktaDemo.Hubs;

public class DashboardHub : Hub
{
    private static object _lock = new();
    private static bool _isInitialized = false;
    private static int _dashboardUsers = 0;
    private readonly IOktaClient _client;
    private readonly IHubContext<DashboardHub> _hubContext;
    private readonly ILogger<DashboardHub> _logger;
    
    public static StatisticsModel Current { get; private set; } = new StatisticsModel();
    
    public DashboardHub(IOktaClient oktaClient, IHubContext<DashboardHub> hubContext, ILogger<DashboardHub> logger)
    {
        _client = oktaClient;
        _hubContext = hubContext;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        Interlocked.Increment(ref _dashboardUsers);

        await base.OnConnectedAsync();

        if (!_isInitialized)
        {
            lock (_lock)
            {
                if (!_isInitialized)
                {
                    var initializationTask = Initialize(_hubContext);
                }
            }
        }
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Interlocked.Decrement(ref _dashboardUsers);

        return base.OnDisconnectedAsync(exception);
    }

    private async Task Initialize(IHubContext<DashboardHub> context)
    {
        _isInitialized = true;
        while (true)
        {
            if (_dashboardUsers > 0)
            {
                _logger.LogInformation("Fetching statistics");

                try
                {
                    var now = DateTime.UtcNow;
                    var eventsQuery = _client.Logs.GetLogs(
                        since: now.AddDays(-6).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        sortOrder: "DESCENDING",
                        limit: 1000);

                    var events = await eventsQuery.ToListAsync();
                    var model = new StatisticsModel(events, _dashboardUsers);

                    Current = model;

                    _logger.LogInformation($"Successfully retrieved {events.Count} log events");
                    _logger.LogInformation("Publishing statistics data");

                    await context.Clients.All.SendAsync("UpdateStatistics", model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }

                await Task.Delay(20000);
            }
        }
    }
}