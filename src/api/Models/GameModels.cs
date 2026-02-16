namespace Api.Models;

public class GameSession
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
    public string? ActiveCaseId { get; set; }
}

public class GameCase
{
    public string Id { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Briefing { get; set; } = string.Empty;
    public StolenTreasure StolenTreasure { get; set; } = new();
    public string CurrentCity { get; set; } = string.Empty;
    public int RemainingSteps { get; set; } = 10;
    public string Status { get; set; } = "active";
    public List<string> Trail { get; set; } = new();
    public string CorrectSuspectId { get; set; } = string.Empty;
    public int CurrentCityIndex { get; set; }
    public List<string> VisitedCities { get; set; } = new();
    public bool WarrantIssued { get; set; }
    public Warrant? Warrant { get; set; }
    public List<string> TravelOptions { get; set; } = new();
}

public class City
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Continent { get; set; } = string.Empty;
    public string BackgroundKey { get; set; } = string.Empty;
    public List<NPC> Npcs { get; set; } = new();
}

public class NPC
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class Suspect
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PhotoKey { get; set; } = string.Empty;
    public SuspectTraits Traits { get; set; } = new();
}

public class SuspectTraits
{
    public string HairColor { get; set; } = string.Empty;
    public string EyeColor { get; set; } = string.Empty;
    public string Hobby { get; set; } = string.Empty;
    public string FavoriteFood { get; set; } = string.Empty;
    public string Vehicle { get; set; } = string.Empty;
    public string DistinguishingFeature { get; set; } = string.Empty;
}

public class StolenTreasure
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class Warrant
{
    public string SuspectId { get; set; } = string.Empty;
    public string CityId { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
}

public class TravelOption
{
    public string CityId { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class NpcChatMessage
{
    public string NpcId { get; set; } = string.Empty;
    public string NpcName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class NpcChatHistory
{
    public List<NpcChatMessage> Messages { get; set; } = new();
}
