using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Serilog;

namespace WilsonEvoModuleLibrary.Hubs;

public sealed class ModuleClient
{
    private readonly AzureBusSenderService _busSender;
    private readonly ILogger _logger;

    public ModuleClient(ILogger logger, AzureBusSenderService busSender)
    {
        _logger = logger;
        _busSender = busSender;
    }

    public Task<Result> Next(string sessionId, object response, CancellationToken token = default)
    {
        return _busSender.Run(sessionId, response, token);
    }

    public Task<Result> Start(string shortUrl, string channelName, object? request, CancellationToken token = default)
    {
        return _busSender.Start(shortUrl, channelName, request, token);
    }
}