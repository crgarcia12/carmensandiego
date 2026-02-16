using Api.Models;

namespace Api.Services;

public class SuspectService : ISuspectService
{
    private readonly GameDataProvider _dataProvider;

    public SuspectService(GameDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public List<Suspect> GetAllSuspects() => _dataProvider.Suspects;

    public Suspect? GetSuspect(string suspectId) => _dataProvider.GetSuspect(suspectId);
}
