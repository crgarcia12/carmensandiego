using System.Collections.Concurrent;
using Api.Models;

namespace Api.Services;

public class CaseService : ICaseService
{
    private readonly ConcurrentDictionary<string, GameCase> _cases = new();
    private readonly ConcurrentDictionary<string, string> _sessionActiveCases = new();
    private readonly GameDataProvider _dataProvider;
    private readonly Random _random = new();

    public CaseService(GameDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
        SeedTestData();
    }

    private void SeedTestData()
    {
        var testSessionId = "sess-00000000-0000-0000-0000-000000000001";

        // case-win-001: completed/won case (for summary tests)
        _cases["case-win-001"] = new GameCase
        {
            Id = "case-win-001",
            SessionId = testSessionId,
            Title = "The Case of the Missing Mona Lisa",
            Briefing = "The Mona Lisa has been stolen from the Louvre!",
            StolenTreasure = new StolenTreasure { Name = "Mona Lisa", Description = "Famous painting by Leonardo da Vinci" },
            CurrentCity = "paris",
            RemainingSteps = 7,
            Status = "won",
            Trail = new List<string> { "bangkok", "tokyo", "cairo", "paris" },
            CorrectSuspectId = "suspect-carmen",
            CurrentCityIndex = 3,
            VisitedCities = new List<string> { "bangkok", "tokyo", "cairo", "paris" },
            WarrantIssued = true,
            Warrant = new Warrant { SuspectId = "suspect-carmen", CityId = "paris", IssuedAt = DateTime.UtcNow }
        };

        // case-lose-001: lost case (steps exhausted, no warrant)
        _cases["case-lose-001"] = new GameCase
        {
            Id = "case-lose-001",
            SessionId = testSessionId,
            Title = "The Case of the Stolen Samurai Sword",
            Briefing = "A priceless samurai sword has vanished from Tokyo National Museum!",
            StolenTreasure = new StolenTreasure { Name = "Samurai Sword", Description = "Ancient katana from the Edo period" },
            CurrentCity = "istanbul",
            RemainingSteps = 0,
            Status = "lost",
            Trail = new List<string> { "new-york", "london", "cairo", "tokyo", "paris" },
            CorrectSuspectId = "suspect-carmen",
            CurrentCityIndex = 2,
            VisitedCities = new List<string> { "new-york", "london", "cairo", "tokyo", "mumbai", "sydney", "rio", "moscow", "nairobi", "istanbul" },
            WarrantIssued = false
        };

        // case-done-001: completed case (for rejected actions test)
        _cases["case-done-001"] = new GameCase
        {
            Id = "case-done-001",
            SessionId = testSessionId,
            Title = "The Case of the Pilfered Pyramids",
            Briefing = "Someone stole the Great Pyramid's capstone!",
            StolenTreasure = new StolenTreasure { Name = "Pyramid Capstone", Description = "Golden capstone from the Great Pyramid" },
            CurrentCity = "cairo",
            RemainingSteps = 5,
            Status = "won",
            Trail = new List<string> { "london", "paris", "cairo" },
            CorrectSuspectId = "suspect-vic",
            CurrentCityIndex = 2,
            VisitedCities = new List<string> { "london", "paris", "cairo" },
            WarrantIssued = true,
            Warrant = new Warrant { SuspectId = "suspect-vic", CityId = "cairo", IssuedAt = DateTime.UtcNow }
        };

        // case-active-001: active case (for "summary not available" test)
        _cases["case-active-001"] = new GameCase
        {
            Id = "case-active-001",
            SessionId = testSessionId,
            Title = "The Case of the Vanishing Violin",
            Briefing = "A Stradivarius violin has disappeared from Vienna!",
            StolenTreasure = new StolenTreasure { Name = "Stradivarius Violin", Description = "Priceless violin crafted by Antonio Stradivari" },
            CurrentCity = "bangkok",
            RemainingSteps = 8,
            Status = "active",
            Trail = new List<string> { "bangkok", "tokyo", "paris", "cairo", "rome" },
            CorrectSuspectId = "suspect-carmen",
            CurrentCityIndex = 0,
            VisitedCities = new List<string> { "bangkok" },
            WarrantIssued = false,
            TravelOptions = new List<string> { "tokyo", "london", "rio" }
        };

        // case-travel-001: active case for travel tests (in bangkok, with travel options including tokyo)
        _cases["case-travel-001"] = new GameCase
        {
            Id = "case-travel-001",
            SessionId = testSessionId,
            Title = "The Case of the Golden Buddha",
            Briefing = "The Golden Buddha statue has been stolen from Wat Traimit!",
            StolenTreasure = new StolenTreasure { Name = "Golden Buddha", Description = "Solid gold Buddha statue from Bangkok" },
            CurrentCity = "bangkok",
            RemainingSteps = 10,
            Status = "active",
            Trail = new List<string> { "bangkok", "tokyo", "paris", "cairo", "rome" },
            CorrectSuspectId = "suspect-carmen",
            CurrentCityIndex = 0,
            VisitedCities = new List<string> { "bangkok" },
            WarrantIssued = false,
            TravelOptions = new List<string> { "tokyo", "rio", "sydney" }
        };

        // case-chat-001: active case for NPC chat tests (in bangkok)
        _cases["case-chat-001"] = new GameCase
        {
            Id = "case-chat-001",
            SessionId = testSessionId,
            Title = "The Case of the Jade Dragon",
            Briefing = "A priceless jade dragon has been taken from the Bangkok National Museum!",
            StolenTreasure = new StolenTreasure { Name = "Jade Dragon", Description = "Ancient jade dragon figurine" },
            CurrentCity = "bangkok",
            RemainingSteps = 9,
            Status = "active",
            Trail = new List<string> { "bangkok", "tokyo", "paris", "cairo" },
            CorrectSuspectId = "suspect-carmen",
            CurrentCityIndex = 0,
            VisitedCities = new List<string> { "bangkok" },
            WarrantIssued = false,
            TravelOptions = new List<string> { "tokyo", "london", "moscow" }
        };

        // case-warrant-001: active case for warrant tests (at final trail city paris, correct suspect is carmen)
        _cases["case-warrant-001"] = new GameCase
        {
            Id = "case-warrant-001",
            SessionId = testSessionId,
            Title = "The Case of the Eiffel Tower Heist",
            Briefing = "The tip of the Eiffel Tower has been stolen!",
            StolenTreasure = new StolenTreasure { Name = "Eiffel Tower Tip", Description = "The iconic tip of the Eiffel Tower" },
            CurrentCity = "paris",
            RemainingSteps = 6,
            Status = "active",
            Trail = new List<string> { "bangkok", "tokyo", "cairo", "paris" },
            CorrectSuspectId = "suspect-carmen",
            CurrentCityIndex = 3,
            VisitedCities = new List<string> { "bangkok", "tokyo", "cairo", "paris" },
            WarrantIssued = false,
            TravelOptions = new List<string>()
        };
    }

    public GameCase? GetCase(string caseId) =>
        _cases.TryGetValue(caseId, out var c) ? c : null;

    public string? GetActiveCaseId(string sessionId) =>
        _sessionActiveCases.TryGetValue(sessionId, out var caseId) ? caseId : null;

    public GameCase CreateCase(string sessionId)
    {
        // Pick a random trail of 4-6 cities
        var allCities = _dataProvider.Cities.Select(c => c.Id).ToList();
        var trailLength = _random.Next(4, 7);
        var trail = new List<string>();
        var used = new HashSet<string>();

        // First city
        var startIndex = _random.Next(allCities.Count);
        trail.Add(allCities[startIndex]);
        used.Add(allCities[startIndex]);

        // Remaining trail cities
        while (trail.Count < trailLength)
        {
            var available = allCities.Where(c => !used.Contains(c)).ToList();
            if (available.Count == 0) break;
            var next = available[_random.Next(available.Count)];
            trail.Add(next);
            used.Add(next);
        }

        var startCity = trail[0];

        // Generate travel options for starting city
        var travelOptions = GenerateTravelOptions(trail, 0, used);

        var gameCase = new GameCase
        {
            Id = $"case-{Guid.NewGuid()}",
            SessionId = sessionId,
            Title = "The Case of the Missing Crown Jewels",
            Briefing = "The Crown Jewels have been stolen from the Tower of London! Your mission is to track down the thief across the globe.",
            StolenTreasure = new StolenTreasure
            {
                Name = "Crown Jewels",
                Description = "The priceless Crown Jewels of England, including the Imperial State Crown"
            },
            CurrentCity = startCity,
            RemainingSteps = 10,
            Status = "active",
            Trail = trail,
            CorrectSuspectId = "suspect-carmen",
            CurrentCityIndex = 0,
            VisitedCities = new List<string> { startCity },
            WarrantIssued = false,
            TravelOptions = travelOptions
        };

        _cases[gameCase.Id] = gameCase;
        _sessionActiveCases[sessionId] = gameCase.Id;
        return gameCase;
    }

    public City? GetCity(string cityId) => _dataProvider.GetCity(cityId);

    public List<City> GetAllCities() => _dataProvider.Cities;

    public List<TravelOption> GetTravelOptions(GameCase gameCase)
    {
        if (gameCase.Status != "active") return new List<TravelOption>();

        // If at or beyond final city, no travel options
        if (gameCase.CurrentCityIndex >= gameCase.Trail.Count - 1)
            return new List<TravelOption>();

        var options = new List<TravelOption>();
        foreach (var cityId in gameCase.TravelOptions)
        {
            var city = _dataProvider.GetCity(cityId);
            if (city != null)
            {
                options.Add(new TravelOption
                {
                    CityId = city.Id,
                    CityName = city.Name,
                    Description = $"Travel to {city.Name}, {city.Region}"
                });
            }
        }
        return options;
    }

    public (bool success, string? error, string? code) Travel(GameCase gameCase, string cityId)
    {
        if (gameCase.Status != "active")
            return (false, "Case is already completed", "CASE_COMPLETED");

        if (gameCase.RemainingSteps <= 0)
            return (false, "No remaining steps", "NO_STEPS");

        if (cityId == gameCase.CurrentCity)
            return (false, "Already in this city", "SAME_CITY");

        if (!gameCase.TravelOptions.Contains(cityId))
            return (false, "Invalid travel destination", "INVALID_DESTINATION");
        // Move to the city
        gameCase.CurrentCity = cityId;
        gameCase.RemainingSteps--;
        gameCase.VisitedCities.Add(cityId);

        // Check if this is the correct next trail city
        var nextTrailIndex = gameCase.CurrentCityIndex + 1;
        if (nextTrailIndex < gameCase.Trail.Count && gameCase.Trail[nextTrailIndex] == cityId)
        {
            gameCase.CurrentCityIndex = nextTrailIndex;
        }

        // Generate new travel options
        var usedCities = new HashSet<string>(gameCase.VisitedCities);
        if (gameCase.CurrentCityIndex < gameCase.Trail.Count - 1)
        {
            gameCase.TravelOptions = GenerateTravelOptions(gameCase.Trail, gameCase.CurrentCityIndex, usedCities);
        }
        else
        {
            gameCase.TravelOptions = new List<string>();
        }

        // Check lose condition
        if (gameCase.RemainingSteps <= 0)
        {
            gameCase.Status = "lost";
        }

        return (true, null, null);
    }

    private List<string> GenerateTravelOptions(List<string> trail, int currentIndex, HashSet<string> usedCities)
    {
        var options = new List<string>();

        // Add the correct next trail city
        if (currentIndex + 1 < trail.Count)
        {
            options.Add(trail[currentIndex + 1]);
        }

        // Add 2 decoy cities
        var allCityIds = _dataProvider.Cities.Select(c => c.Id).ToList();
        var decoyPool = allCityIds
            .Where(c => !options.Contains(c) && c != trail[currentIndex])
            .OrderBy(_ => _random.Next())
            .Take(2)
            .ToList();

        options.AddRange(decoyPool);

        return options;
    }
}
