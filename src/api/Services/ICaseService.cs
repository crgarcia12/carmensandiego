using Api.Models;

namespace Api.Services;

public interface ICaseService
{
    GameCase? GetCase(string caseId);
    GameCase CreateCase(string sessionId);
    string? GetActiveCaseId(string sessionId);
    City? GetCity(string cityId);
    List<City> GetAllCities();
    List<TravelOption> GetTravelOptions(GameCase gameCase);
    (bool success, string? error, string? code) Travel(GameCase gameCase, string cityId);
}
