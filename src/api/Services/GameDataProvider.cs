using Api.Models;

namespace Api.Services;

public class GameDataProvider
{
    private readonly List<City> _cities;
    private readonly List<Suspect> _suspects;

    public GameDataProvider()
    {
        _cities = InitCities();
        _suspects = InitSuspects();
    }

    public List<City> Cities => _cities;
    public List<Suspect> Suspects => _suspects;

    public City? GetCity(string id) => _cities.FirstOrDefault(c => c.Id == id);
    public Suspect? GetSuspect(string id) => _suspects.FirstOrDefault(s => s.Id == id);

    private static List<City> InitCities()
    {
        return new List<City>
        {
            new() { Id = "bangkok", Name = "Bangkok", Region = "Southeast Asia", Continent = "Asia", BackgroundKey = "bangkok_bg",
                Npcs = new List<NPC> {
                    new() { Id = "npc-somchai", Name = "Somchai", Role = "Street vendor" },
                    new() { Id = "npc-niran", Name = "Niran", Role = "Tuk-tuk driver" },
                    new() { Id = "npc-mali", Name = "Mali", Role = "Temple guide" }
                }},
            new() { Id = "tokyo", Name = "Tokyo", Region = "East Asia", Continent = "Asia", BackgroundKey = "tokyo_bg",
                Npcs = new List<NPC> {
                    new() { Id = "npc-yuki", Name = "Yuki", Role = "Sushi chef" },
                    new() { Id = "npc-kenji", Name = "Kenji", Role = "Train conductor" },
                    new() { Id = "npc-aiko", Name = "Aiko", Role = "Museum curator" }
                }},
            new() { Id = "paris", Name = "Paris", Region = "Western Europe", Continent = "Europe", BackgroundKey = "paris_bg",
                Npcs = new List<NPC> {
                    new() { Id = "npc-pierre", Name = "Pierre", Role = "Café owner" },
                    new() { Id = "npc-colette", Name = "Colette", Role = "Art dealer" },
                    new() { Id = "npc-jean", Name = "Jean", Role = "Bookshop keeper" }
                }},
            new() { Id = "cairo", Name = "Cairo", Region = "North Africa", Continent = "Africa", BackgroundKey = "cairo_bg",
                Npcs = new List<NPC> {
                    new() { Id = "npc-hassan", Name = "Hassan", Role = "Archaeologist" },
                    new() { Id = "npc-fatima", Name = "Fatima", Role = "Spice merchant" }
                }},
            new() { Id = "rio", Name = "Rio de Janeiro", Region = "South America", Continent = "South America", BackgroundKey = "rio_bg",
                Npcs = new List<NPC> {
                    new() { Id = "npc-carlos", Name = "Carlos", Role = "Samba musician" },
                    new() { Id = "npc-lucia", Name = "Lucia", Role = "Tour guide" }
                }},
            new() { Id = "new-york", Name = "New York", Region = "North America", Continent = "North America", BackgroundKey = "newyork_bg",
                Npcs = new List<NPC> {
                    new() { Id = "npc-mike", Name = "Mike", Role = "Hot dog vendor" },
                    new() { Id = "npc-sarah", Name = "Sarah", Role = "Taxi driver" }
                }},
            new() { Id = "london", Name = "London", Region = "Western Europe", Continent = "Europe", BackgroundKey = "london_bg",
                Npcs = new List<NPC> {
                    new() { Id = "npc-arthur", Name = "Arthur", Role = "Bobby" },
                    new() { Id = "npc-emma", Name = "Emma", Role = "Pub owner" }
                }},
            new() { Id = "sydney", Name = "Sydney", Region = "Oceania", Continent = "Oceania", BackgroundKey = "sydney_bg",
                Npcs = new List<NPC> {
                    new() { Id = "npc-bruce", Name = "Bruce", Role = "Surfer" },
                    new() { Id = "npc-sheila", Name = "Sheila", Role = "Zookeeper" }
                }},
            new() { Id = "mumbai", Name = "Mumbai", Region = "South Asia", Continent = "Asia", BackgroundKey = "mumbai_bg",
                Npcs = new List<NPC> {
                    new() { Id = "npc-priya", Name = "Priya", Role = "Bollywood actress" },
                    new() { Id = "npc-raj", Name = "Raj", Role = "Rickshaw driver" }
                }},
            new() { Id = "moscow", Name = "Moscow", Region = "Eastern Europe", Continent = "Europe", BackgroundKey = "moscow_bg",
                Npcs = new List<NPC> {
                    new() { Id = "npc-ivan", Name = "Ivan", Role = "Chess master" },
                    new() { Id = "npc-olga", Name = "Olga", Role = "Ballet dancer" }
                }},
            new() { Id = "nairobi", Name = "Nairobi", Region = "East Africa", Continent = "Africa", BackgroundKey = "nairobi_bg",
                Npcs = new List<NPC> {
                    new() { Id = "npc-amina", Name = "Amina", Role = "Safari guide" },
                    new() { Id = "npc-jomo", Name = "Jomo", Role = "Market trader" }
                }},
            new() { Id = "istanbul", Name = "Istanbul", Region = "Eurasia", Continent = "Europe", BackgroundKey = "istanbul_bg",
                Npcs = new List<NPC> {
                    new() { Id = "npc-mehmet", Name = "Mehmet", Role = "Carpet seller" },
                    new() { Id = "npc-ayse", Name = "Ayse", Role = "Baklava maker" }
                }},
            new() { Id = "mexico-city", Name = "Mexico City", Region = "Central America", Continent = "North America", BackgroundKey = "mexicocity_bg",
                Npcs = new List<NPC> {
                    new() { Id = "npc-diego", Name = "Diego", Role = "Muralist" },
                    new() { Id = "npc-rosa", Name = "Rosa", Role = "Taco vendor" }
                }},
            new() { Id = "beijing", Name = "Beijing", Region = "East Asia", Continent = "Asia", BackgroundKey = "beijing_bg",
                Npcs = new List<NPC> {
                    new() { Id = "npc-wei", Name = "Wei", Role = "Tea master" },
                    new() { Id = "npc-ling", Name = "Ling", Role = "Silk merchant" }
                }},
            new() { Id = "rome", Name = "Rome", Region = "Southern Europe", Continent = "Europe", BackgroundKey = "rome_bg",
                Npcs = new List<NPC> {
                    new() { Id = "npc-marco", Name = "Marco", Role = "Gelato maker" },
                    new() { Id = "npc-giulia", Name = "Giulia", Role = "Historian" }
                }},
        };
    }

    private static List<Suspect> InitSuspects()
    {
        return new List<Suspect>
        {
            new() { Id = "suspect-carmen", Name = "Carmen Sandiego", PhotoKey = "carmen_photo",
                Traits = new SuspectTraits { HairColor = "Black", EyeColor = "Brown", Hobby = "Hang gliding", FavoriteFood = "Paella", Vehicle = "Convertible", DistinguishingFeature = "Red trench coat" }},
            new() { Id = "suspect-vic", Name = "Vic the Slick", PhotoKey = "vic_photo",
                Traits = new SuspectTraits { HairColor = "Blonde", EyeColor = "Blue", Hobby = "Surfing", FavoriteFood = "Pizza", Vehicle = "Motorcycle", DistinguishingFeature = "Gold tooth" }},
            new() { Id = "suspect-patty", Name = "Patty Larceny", PhotoKey = "patty_photo",
                Traits = new SuspectTraits { HairColor = "Red", EyeColor = "Green", Hobby = "Mountain climbing", FavoriteFood = "Sushi", Vehicle = "Helicopter", DistinguishingFeature = "Scar on left cheek" }},
            new() { Id = "suspect-eartha", Name = "Eartha Brute", PhotoKey = "eartha_photo",
                Traits = new SuspectTraits { HairColor = "Brown", EyeColor = "Hazel", Hobby = "Weightlifting", FavoriteFood = "Steak", Vehicle = "Tank", DistinguishingFeature = "Muscular build" }},
            new() { Id = "suspect-double", Name = "Double Trouble", PhotoKey = "double_photo",
                Traits = new SuspectTraits { HairColor = "Black", EyeColor = "Brown", Hobby = "Acting", FavoriteFood = "Croissant", Vehicle = "Limousine", DistinguishingFeature = "Identical twin" }},
            new() { Id = "suspect-top", Name = "Top Grunge", PhotoKey = "top_photo",
                Traits = new SuspectTraits { HairColor = "Purple", EyeColor = "Gray", Hobby = "Skateboarding", FavoriteFood = "Tacos", Vehicle = "Skateboard", DistinguishingFeature = "Mohawk" }},
            new() { Id = "suspect-buggs", Name = "Buggs Zapper", PhotoKey = "buggs_photo",
                Traits = new SuspectTraits { HairColor = "Gray", EyeColor = "Blue", Hobby = "Electronics", FavoriteFood = "Ramen", Vehicle = "Drone", DistinguishingFeature = "Thick glasses" }},
            new() { Id = "suspect-contessa", Name = "Contessa", PhotoKey = "contessa_photo",
                Traits = new SuspectTraits { HairColor = "Silver", EyeColor = "Violet", Hobby = "Fencing", FavoriteFood = "Caviar", Vehicle = "Yacht", DistinguishingFeature = "Diamond necklace" }},
            new() { Id = "suspect-wonder", Name = "Wonder Rat", PhotoKey = "wonder_photo",
                Traits = new SuspectTraits { HairColor = "Brown", EyeColor = "Black", Hobby = "Tunneling", FavoriteFood = "Cheese", Vehicle = "Submarine", DistinguishingFeature = "Whiskers" }},
            new() { Id = "suspect-sarah", Name = "Sarah Nade", PhotoKey = "sarah_photo",
                Traits = new SuspectTraits { HairColor = "Auburn", EyeColor = "Green", Hobby = "Singing", FavoriteFood = "Curry", Vehicle = "Hot air balloon", DistinguishingFeature = "Musical laugh" }},
            new() { Id = "suspect-robo", Name = "Robocrook", PhotoKey = "robo_photo",
                Traits = new SuspectTraits { HairColor = "None", EyeColor = "Red", Hobby = "Programming", FavoriteFood = "Electricity", Vehicle = "Jetpack", DistinguishingFeature = "Metal body" }},
        };
    }
}
