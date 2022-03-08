using Okta.Sdk;

public class StatisticsModel
{
    private readonly List<ILogEvent> _events;
    private readonly List<ILogEvent> _successes;
    private readonly List<ILogEvent> _failures;

    public StatisticsModel()
    {
        Loading = true;
        _events = new();
        _successes = new();
        _failures = new();

        WeeklyVisits = new()
        {
            {DayOfWeek.Sunday, 0},
            {DayOfWeek.Monday, 0},
            {DayOfWeek.Tuesday, 0},
            {DayOfWeek.Wednesday, 0},
            {DayOfWeek.Thursday, 0},
            {DayOfWeek.Friday, 0},
            {DayOfWeek.Saturday, 0}
        };
        WeeklyEvents = new()
        {
            {DayOfWeek.Sunday, 0},
            {DayOfWeek.Monday, 0},
            {DayOfWeek.Tuesday, 0},
            {DayOfWeek.Wednesday, 0},
            {DayOfWeek.Thursday, 0},
            {DayOfWeek.Friday, 0},
            {DayOfWeek.Saturday, 0}
        };
        WeeklyFailures = new()
        {
            {DayOfWeek.Sunday, 0},
            {DayOfWeek.Monday, 0},
            {DayOfWeek.Tuesday, 0},
            {DayOfWeek.Wednesday, 0},
            {DayOfWeek.Thursday, 0},
            {DayOfWeek.Friday, 0},
            {DayOfWeek.Saturday, 0}
        };
    }

    public StatisticsModel(IList<ILogEvent> events, int dashboardUsers)
    {
        _events = events.Where(x => x.DisplayMessage != null && !x.DisplayMessage.Contains("Rate limit")).ToList();
        _successes = _events.Where(x => x.Outcome.Result == "SUCCESS").ToList();
        _failures = _events.Where(x => x.Outcome.Result != "SUCCESS").ToList();

        DashboardUsers = dashboardUsers;
        WeeklyVisits = new()
        {
            {DayOfWeek.Sunday, 0},
            {DayOfWeek.Monday, 0},
            {DayOfWeek.Tuesday, 0},
            {DayOfWeek.Wednesday, 0},
            {DayOfWeek.Thursday, 0},
            {DayOfWeek.Friday, 0},
            {DayOfWeek.Saturday, 0}
        };
        WeeklyEvents = new()
        {
            {DayOfWeek.Sunday, 0},
            {DayOfWeek.Monday, 0},
            {DayOfWeek.Tuesday, 0},
            {DayOfWeek.Wednesday, 0},
            {DayOfWeek.Thursday, 0},
            {DayOfWeek.Friday, 0},
            {DayOfWeek.Saturday, 0}
        };
        WeeklyFailures = new()
        {
            {DayOfWeek.Sunday, 0},
            {DayOfWeek.Monday, 0},
            {DayOfWeek.Tuesday, 0},
            {DayOfWeek.Wednesday, 0},
            {DayOfWeek.Thursday, 0},
            {DayOfWeek.Friday, 0},
            {DayOfWeek.Saturday, 0}
        };

        var weeklySessionsLookup = _events
            .Where(x => x.Published >= DateTime.Today.AddDays(-6))
            .ToLookup(x => x.Published.GetValueOrDefault().ToLocalTime().DayOfWeek);

        foreach (var grouping in weeklySessionsLookup)
        {
            WeeklyVisits[grouping.Key] = grouping.Where(x => x.Outcome.Result == "SUCCESS" && x.EventType == "user.session.start").Count();
            WeeklyEvents[grouping.Key] = grouping.Count();
            WeeklyFailures[grouping.Key] = grouping.Where(x => x.Outcome.Result != "SUCCESS").Count();
        }
    }

    public Dictionary<DayOfWeek, int> WeeklyVisits { get; }
    public Dictionary<DayOfWeek, int> WeeklyEvents { get; }
    public Dictionary<DayOfWeek, int> WeeklyFailures { get; }

    public bool Loading { get; }

    public int EventsToday => _events.Count(x => x.Published >= DateTime.Today);
    public int EventsYesterday => _events.Count(x => x.Published >= DateTime.Today.AddDays(-1));
    public decimal EventsPercentChange => 100 * (EventsToday - EventsYesterday) /
        (EventsYesterday > 0 ? EventsYesterday : 1);

    public List<char> SessionLabels => Enumerable.Range(1, 7).Select((x, i) => ((DayOfWeek)((int)(DateTime.Today.DayOfWeek + x) % 7)).ToString()[0]).ToList();
    public List<int> SessionData => Enumerable.Range(1, 7).Select((x, i) => WeeklyVisits[((DayOfWeek)((int)(DateTime.Today.DayOfWeek + x) % 7))]).ToList();
    
    public int SessionsToday => WeeklyVisits[DateTime.Today.DayOfWeek];
    public int SessionsYesterday => WeeklyVisits[(DayOfWeek)(((int)DateTime.Today.DayOfWeek + 6) % 7)];
    public decimal SessionsPercentChange => 100 * (SessionsToday - SessionsYesterday) /
        (SessionsYesterday > 0 ? SessionsYesterday : 1);

    public int FailuresToday => WeeklyFailures[DateTime.Today.DayOfWeek];
    public int FailuresYesterday => WeeklyFailures[(DayOfWeek)(((int)DateTime.Today.DayOfWeek + 6) % 7)];
    public decimal FailuresPercentChange => 100 * (FailuresToday - FailuresYesterday) /
        (FailuresYesterday > 0 ? FailuresYesterday : 1);
    
    public List<char> EventsLabels => Enumerable.Range(1, 7).Select((x, i) => ((DayOfWeek)((int)(DateTime.Today.DayOfWeek + x) % 7)).ToString()[0]).ToList();
    public List<int> EventsData => Enumerable.Range(1, 7).Select((x, i) => WeeklyEvents[((DayOfWeek)((int)(DateTime.Today.DayOfWeek + x) % 7))]).ToList();
    
    public List<char> FailuresLabels => Enumerable.Range(1, 7).Select((x, i) => ((DayOfWeek)((int)(DateTime.Today.DayOfWeek + x) % 7)).ToString()[0]).ToList();
    public List<int> FailuresData => Enumerable.Range(1, 7).Select((x, i) => WeeklyFailures[((DayOfWeek)((int)(DateTime.Today.DayOfWeek + x) % 7))]).ToList();
    
    public List<Tuple<string, int, decimal>> TopActivity => _events
        .ToLookup(x => x.DisplayMessage)
        .OrderByDescending(x => x.Count())
        .Select(x => Tuple.Create(x.Key, x.Count(), Math.Round((x.Count() * 100.0m / _events.Count))))
        .Take(7)
        .ToList();

    public List<Tuple<string, DateTimeOffset>> RecentActivity => _events.Take(7).Select(x => Tuple.Create(x.DisplayMessage, x.Published.GetValueOrDefault())).ToList();

    public List<Tuple<string, int>> ActivityMap => _events
        .Where(x => x.Published >= DateTime.Today.AddDays(-6))
        .Where(x => x.ClientInfo != null && x.ClientInfo.GeographicalContext != null && x.ClientInfo.GeographicalContext.State != null)
        .ToLookup(x => x.ClientInfo.GeographicalContext.State)
        .Select(x => Tuple.Create(x.Key, x.Count()))
        .ToList();
        
    public int DashboardUsers { get; }
}
