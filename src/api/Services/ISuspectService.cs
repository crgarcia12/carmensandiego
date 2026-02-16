using Api.Models;

namespace Api.Services;

public interface ISuspectService
{
    List<Suspect> GetAllSuspects();
    Suspect? GetSuspect(string suspectId);
}
