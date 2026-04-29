using BpmnDotNet.Abstractions.Common;
using BpmnDotNet.Abstractions.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sample.ConsoleApp.Service;

/// <summary>
/// Регистрация Bpmn handlers.
/// </summary>
public class BpmnClientHost : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BpmnClientHost> _logger;
    private readonly IBpmnClient _bpmnClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="BpmnClientHost"/> class.
    /// </summary>
    /// <param name="serviceProvider">IServiceProvider.</param>
    /// <param name="logger">ILogger.</param>
    /// <param name="bpmnClient">IBpmnClient.</param>
    public BpmnClientHost(IServiceProvider serviceProvider, ILogger<BpmnClientHost> logger, IBpmnClient bpmnClient)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bpmnClient = bpmnClient ?? throw new ArgumentNullException(nameof(bpmnClient));
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var handlerTypes = scope.ServiceProvider.GetServices<IBpmnHandler>()?.ToArray() ?? [];
        if (!handlerTypes.Any())
        {
            throw new InvalidOperationException("[BpmnClientHost] Bpmn handlers not found");
        }

        _bpmnClient.RegisterHandlers<IBpmnHandler>(handlerTypes);

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}