using System.Diagnostics;
using AutoFixture;
using BpmnDotNet.Common;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.ElasticClient.Tests.Configs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace BpmnDotNet.ElasticClient.Tests;

public class BackgroundWorkerService : BackgroundService
{
    private readonly ILogger<BackgroundWorkerService> _logger;
    private readonly AppSettings _config;
    private readonly IElasticClient _elasticClient;
    private readonly Fixture _fixture;

    public BackgroundWorkerService(ILogger<BackgroundWorkerService> logger, AppSettings config,
        IElasticClient elasticClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        _fixture = new Fixture();
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
                    Pus();
                    break;
                case 's':
                    // GetDataIndex();
                    await GetLastData();
                    // Pagination();
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

    private void Pagination()
    {
        var count = _elasticClient.GetHistoryNodeStateCountAsync("8P4W9c9Nhs").Result;
        var pageSize = (int) count / 3;
        var documents = _elasticClient
            .SearchWithPaginationAsync("8P4W9c9Nhs", 2,pageSize).Result;
        
        if ( documents is null)
        {
            throw new Exception("Failed to set history node state");
        }
    }

    private async Task GetLastData()
    {
        
        var documents = await _elasticClient.GetLastDataAsync(10,"8P4W9c9Nhs");
        if ( documents is null)
        {
            throw new Exception("Failed to set history node state");
        }
    }

    private void GetDataIndex()
    {
        var document = _elasticClient.GetDataFromIdAsync<HistoryNodeState>("09ddbfac-e53e-4bb3-bfd8-e636e1ebf0e6_8SnxEZBTb9").Result;
        if ( document is null)
        {
            throw new Exception("Failed to set history node state");
        }
        _logger.LogDebug(document.ToString());
        
        var document2 = _elasticClient.GetDataFromIdAsync<UIBpmnDiagram>("01DNtpUzMm").Result;
        if ( document2 is null)
        {
            throw new Exception("Failed to set history node state");
        }
        _logger.LogDebug(document2.ToString());
    }

    string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)])
            .ToArray());
    }

    private void Pus()
    {
        var sw = new Stopwatch();
        sw.Start();
        var guid = Guid.NewGuid();
        for (var i = 0; i < 10; i++)
        {
            var historyNodeState = new HistoryNodeState()
            {
                IdBpmnProcess = guid.ToString(),
                TokenProcess = GenerateRandomString(10),
                NoesStaus = _fixture.Build<NodeJobStatus>().CreateMany(15).ToArray()
            };
            var reshistoryNodeState = _elasticClient.SetDataAsync(historyNodeState).Result;
            if (!reshistoryNodeState)
            {
                throw new Exception("Failed to set history node state");
            }

            var UIBpmnDiagram = new UIBpmnDiagram()
            {
                IdBpmnProcess = GenerateRandomString(10),
            };

            var resUi = _elasticClient.SetDataAsync(UIBpmnDiagram).Result;
            if (!resUi)
            {
                throw new Exception("Failed to set SetUIBpmnDiagramAsync");
            }

            if (i > 0 && i % 100 == 0)
            {
                _logger.LogDebug($"processed:{i}");
            }
        }

        sw.Stop();
        _logger.LogDebug($"Elapsed time: {sw.ElapsedMilliseconds}");
    }
}