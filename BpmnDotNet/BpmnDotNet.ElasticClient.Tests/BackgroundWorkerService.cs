using BpmnDotNet.Common;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.Common.Interfases;
using BpmnDotNet.ElasticClient.Tests.Configs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace BpmnDotNet.ElasticClient.Tests;

public class BackgroundWorkerService : BackgroundService
{
    private readonly ILogger<BackgroundWorkerService> _logger;
    private readonly AppSettings _config;
    private readonly IElasticClient _elasticClient;

    public BackgroundWorkerService(ILogger<BackgroundWorkerService> logger, AppSettings config, IElasticClient elasticClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Produce();
    }
    
    private async Task Produce()
    {
        var keepRunning = true;

        // _sampleService.StartNewProcess();

        while (keepRunning)
        {
            _logger.LogDebug(@"a) Send 1 start \n q) Quit");
            var key = char.ToLower(Console.ReadKey(true).KeyChar);

            switch (key)
            {
                case 'a':
                    Run();
                    break;
                case 's':
                   
                    break;
                case 'q':
                    Console.WriteLine("Quitting");
                   
                    keepRunning = false;
                    break;
            }
        }

        _logger.LogDebug("Consumer listening - press ENTER to quit");
        Console.ReadLine();
    }

    string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)])
            .ToArray());
    }
    private void Run()
    {
        var historyNodeState = new HistoryNodeState()
        {
            IdBpmnProcess = GenerateRandomString(10),
            TokenProcess = GenerateRandomString(10),

        };
        _elasticClient.SetHistoryNodeStateAsync(historyNodeState);
    }
}