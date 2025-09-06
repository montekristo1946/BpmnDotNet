using System.Diagnostics;
using AutoFixture;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.BPMNDiagram;
using BpmnDotNet.Common.Dto;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BpmnDotNet.ElasticClient.Tests;

public class BackgroundWorkerService : BackgroundService
{
    private readonly IElasticClient _elasticClient;
    private readonly Fixture _fixture;
    private readonly ILogger<BackgroundWorkerService> _logger;
    private readonly IXmlSerializationBpmnDiagramSection _xmlSerializationProcessSection;

    public BackgroundWorkerService(ILogger<BackgroundWorkerService> logger,
        IElasticClient elasticClient, IXmlSerializationBpmnDiagramSection xmlSerializationProcessSection)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        _xmlSerializationProcessSection = xmlSerializationProcessSection;
        _fixture = new Fixture();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Produce();
    }

    private Task Produce()
    {
        var keepRunning = true;
        
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
                    // LoadXmlBpmn();
                    // GetXmlBpmn();
                    // GetAllIdBpmnPlan();
                    // GetCountHistoryNodeState();
                    // GetHistoryNodeStateAsync();
                    GetHistoryNodeFromTokenMask();
                    break;
                case 'q':
                    Console.WriteLine("Quitting");

                    keepRunning = false;
                    break;
            }
        }

        _logger.LogDebug("Consumer listening - press ENTER to quit");
        Console.ReadLine();
        return Task.CompletedTask;
    }

    
    private void GetDataIndex()
    {
        var document = _elasticClient
            .GetDataFromIdAsync<HistoryNodeState>("09ddbfac-e53e-4bb3-bfd8-e636e1ebf0e6_8SnxEZBTb9").Result;
        if (document is null) throw new Exception("Failed to set history node state");
        _logger.LogDebug(document.ToString());
    }

    private string GenerateRandomString(int length)
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
            var historyNodeState = new HistoryNodeState
            {
                IdBpmnProcess = guid.ToString(),
                TokenProcess = GenerateRandomString(10),
                NodeStaus = _fixture.Build<NodeJobStatus>().CreateMany(15).ToArray()
            };
            var reshistoryNodeState = _elasticClient.SetDataAsync(historyNodeState).Result;
            if (!reshistoryNodeState) 
                throw new Exception("Failed to set history node state");
            
            if (i > 0 && i % 100 == 0) _logger.LogDebug($"processed:{i}");
        }

        sw.Stop();
        _logger.LogDebug($"Elapsed time: {sw.ElapsedMilliseconds}");
    }

    private void LoadXmlBpmn()
    {
        var diagram = _xmlSerializationProcessSection.LoadXmlBpmnDiagram("./BpmnDiagram/diagram_1.bpmn");

        var reshistoryNodeState = _elasticClient.SetDataAsync(diagram).Result;
        if (!reshistoryNodeState) throw new Exception("Failed to set history node state");
    }

    private void GetXmlBpmn()
    {
        var reshistoryNodeState =
            _elasticClient.GetDataFromIdAsync<BpmnPlane>("IdBpmnProcessingMain_638914914391324271").Result;
        if (reshistoryNodeState is null) throw new Exception("Failed to set history node state");
    }

    private void GetAllIdBpmnPlan()
    {
        var idBpmnProcesss = _elasticClient.GetAllFieldsAsync<BpmnPlane, string>(
            nameof(BpmnPlane.IdBpmnProcess), 1000)
            .Result;
        if (idBpmnProcesss?.Any() != true)
            throw new Exception("Failed to set history node state");
    }

    private void GetCountHistoryNodeState()
    {
        var processStatus = "Completed";
        var count = _elasticClient.GetCountHistoryNodeState("IdBpmnProcessingMain", ["Completed", "None", "Works", "Error"]).Result;

        if (count == 0)
            throw new Exception("Failed to set history node state");
    }

    private void GetHistoryNodeStateAsync()
    {
        var processStatus = "Completed";
        var res = _elasticClient.GetHistoryNodeStateAsync("IdBpmnProcessingMain", 0, 10, ["Completed", "None", "Works", "Error"]).Result;

        if (res.Any() is false)
            throw new Exception("Failed to set history node state");
    }
    
    private void GetHistoryNodeFromTokenMask()
    {
        var res = _elasticClient.GetHistoryNodeFromTokenMaskAsync("IdBpmnProcessingMain", "*_395*").Result;

        if (res.Any() is false)
            throw new Exception("Failed to set history node state");
    }
}