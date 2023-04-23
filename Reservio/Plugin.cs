﻿using Microsoft.Extensions.DependencyInjection;
using SharedLibraryCore;
using SharedLibraryCore.Events.Management;
using SharedLibraryCore.Interfaces;
using SharedLibraryCore.Interfaces.Events;

namespace Reservio;

public class Plugin : IPluginV2
{
    private readonly ReservedClientsConfiguration _config;
    public string Name => "Reservio";
    public string Version => "2023-04-23";
    public string Author => "Amos";

    public Plugin(ReservedClientsConfiguration config)
    {
        _config = config;

        IManagementEventSubscriptions.Load += OnLoad;
        IManagementEventSubscriptions.ClientStateInitialized += OnClientStateInitialized;
    }

    public static void RegisterDependencies(IServiceCollection serviceCollection)
    {
        serviceCollection.AddConfiguration("ReservedClientsSettings", new ReservedClientsConfiguration());
    }

    private Task OnClientStateInitialized(ClientStateInitializeEvent clientEvent, CancellationToken token)
    {
        var clientGuid = _config.ReservedClients.Find(client => client.Names.Contains(clientEvent.Client.CleanedName.ToLower()));

        if (clientGuid is null) return Task.CompletedTask;
        if (clientGuid.Game != clientEvent.Client.GameName) return Task.CompletedTask;

        if (clientGuid.Guid.ToLower() != clientEvent.Client.GuidString)
        {
            clientEvent.Client.Kick(_config.KickMessage, Utilities.IW4MAdminClient());
        }

        return Task.CompletedTask;
    }

    private async Task OnLoad(IManager manager, CancellationToken token)
    {
        var duplicateNames = GetDuplicateNames();
        var duplicateGuids = GetDuplicateGuids();
        var capitalisedNames = GetCapitalisedNames();

        if (duplicateGuids.Any() || duplicateNames.Any() || capitalisedNames.Any())
        {
            LogConfigIssues(duplicateNames, duplicateGuids, capitalisedNames);
            Environment.Exit(1);
        }


        Console.WriteLine($"[{Name}] loaded. Version: {Version}");
        await Task.CompletedTask;
    }

    private List<string> GetDuplicateNames()
    {
        return _config.ReservedClients
            .SelectMany(client => client.Names)
            .GroupBy(str => str)
            .Where(grouping => grouping.Count() > 1)
            .Select(grouping => grouping.Key).ToList();
    }

    private List<string> GetDuplicateGuids()
    {
        return _config.ReservedClients
            .GroupBy(client => client.Guid.ToLower())
            .Where(grouping => grouping.Count() > 1)
            .Select(grouping => grouping.Key).ToList();
    }

    private List<string> GetCapitalisedNames()
    {
        return _config.ReservedClients
            .SelectMany(client => client.Names)
            .Where(str => str.Any(char.IsUpper))
            .ToList();
    }

    private void LogConfigIssues(IReadOnlyCollection<string> duplicateNames, IReadOnlyCollection<string> duplicateGuids,
        IReadOnlyCollection<string> capitalisedNames)
    {
        if (duplicateNames.Any())
        {
            Console.WriteLine($"[{Name}] Duplicate Names: {string.Join(", ", duplicateNames)}");
        }

        if (duplicateGuids.Any())
        {
            Console.WriteLine($"[{Name}] Duplicate GUIDs: {string.Join(", ", duplicateGuids)}");
        }

        if (capitalisedNames.Any())
        {
            Console.WriteLine($"[{Name}] Capitalised names found: {string.Join(", ", capitalisedNames)}");
        }

        Console.WriteLine($"[{Name}] Resolve issues before starting!\n[{Name}] Exiting...");
    }
}
