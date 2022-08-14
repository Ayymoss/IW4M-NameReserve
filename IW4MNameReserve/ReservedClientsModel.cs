using SharedLibraryCore.Interfaces;

namespace IW4MNameReserve;

public class ReservedClientsModel
{
    public string Guid { get; set; }
    public List<string> Names { get; set; }
}

public class ReservedClientsConfiguration : IBaseConfiguration
{
    public string KickMessage { get; set; } = "Name reserved. Change your name";

    public List<ReservedClientsModel> ReservedClients { get; set; } = new()
    {
        new ReservedClientsModel
        {
            Guid = "1995102c6e610ff7",
            Names = new List<string> {":emp:ayymoss", "ayymoss", "amos"}
        },
        new ReservedClientsModel
        {
            Guid = "0001110001110001",
            Names = new List<string> {"exampleothername1", "exampleothername2"}
        },
        new ReservedClientsModel
        {
            Guid = "0001110001110002",
            Names = new List<string> {"anotherexamplename"}
        }
    };

    public int Version { get; set; } = Plugin.ConfigurationVersion;

    public string Name() => "ReservedClients";

    public IBaseConfiguration Generate() => new ReservedClientsConfiguration();
}
