using System.Globalization;
using AutoFixture;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.BPMNDiagram;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.Common.Entities;
using Microsoft.Extensions.Logging;

namespace BpmnDotNet.ElasticClient.Tests.Handlers;

public class ElasticClientTestsIntegration
{
    private readonly ElasticClient.Handlers.ElasticClient _elasticClient;
    private readonly ILoggerFactory loggerFactory;
    private readonly Fixture _fixture;

    private string[] _filtersProcessStatus =
    [
        nameof(ProcessStatus.None), nameof(ProcessStatus.Works), nameof(ProcessStatus.Completed),
        nameof(ProcessStatus.Error)
    ];

    public ElasticClientTestsIntegration()
    {
        var config = new ElasticClientConfig { ConnectionString = "http://localhost:9200" };
        var logger = LoggerFactory.Create(builder => { builder.AddConsole(); }).CreateLogger<IElasticClient>();
        _elasticClient = new ElasticClient.Handlers.ElasticClient(config, logger);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task SetDataAsync_cheksLoadPlaneInDatabase_True()
    {
        var name = $"Name_{DateTime.Now.ToString("MM.dd.yyyy_HH:mm:ss", CultureInfo.InvariantCulture)}";
        var plane = _fixture.Build<BpmnPlane>().With(p => p.Name, name).Create();
        
        var res = await _elasticClient.SetDataAsync(plane);

        Assert.True(res);
    }

    [Fact]
    public async Task GetConditionRouteWithExclusiveGateWay_CheckFalsePath_NotNull()
    {
        var getParams = new[] { nameof(BpmnPlane.IdBpmnProcess), nameof(BpmnPlane.Name) };
        var res = await _elasticClient.GetAllFieldsAsync<BpmnPlane>(getParams, 100);

        Assert.NotEmpty(res);
    }

    [Fact]
    public async Task GetDataFromIdAsync_GetBpmnPlane_NotNull()
    {
        var dBpmnProcess = $"test_IdBpmnProcess";
        var plane = _fixture.Build<BpmnPlane>().With(p => p.IdBpmnProcess, dBpmnProcess).Create();

        var resSetDataAsync = await _elasticClient.SetDataAsync(plane);

        var res = await _elasticClient.GetDataFromIdAsync<BpmnPlane>(dBpmnProcess, []);

        Assert.True(resSetDataAsync);
        Assert.NotNull(res);
        Assert.Equal(dBpmnProcess, res.IdBpmnProcess);
    }

    [Fact]
    public async Task GetCountHistoryNodeStateAsync_GetCount_NotNull()
    {
        var dBpmnProcess = $"test_IdBpmnProcess";
        var historyNodeState = _fixture.Build<HistoryNodeState>()
            .With(p => p.IdBpmnProcess, dBpmnProcess)
            .With(p => p.DateCreated, DateTime.Now.Ticks)
            .With(p => p.DateLastModified, DateTime.Now.Ticks)
            .With(p => p.TokenProcess, DateTime.Now.Ticks.ToString)
            .Create();

        var resSetDataAsync = await _elasticClient.SetDataAsync(historyNodeState);

        var res = await _elasticClient.GetCountHistoryNodeStateAsync(dBpmnProcess, _filtersProcessStatus, 1);

        Assert.True(resSetDataAsync);
        Assert.Equal(1, res);
    }


    [Fact]
    public async Task GetHistoryNodeStateAsync_GetHistory_NotNull()
    {
        var dBpmnProcess = $"test_IdBpmnProcess";
        var historyNodeState = _fixture.Build<HistoryNodeState>()
            .With(p => p.IdBpmnProcess, dBpmnProcess)
            .With(p => p.DateCreated, DateTime.Now.Ticks)
            .With(p => p.DateLastModified, DateTime.Now.Ticks)
            .With(p => p.TokenProcess, DateTime.Now.Ticks.ToString)
            .Create();

        var resSetDataAsync = await _elasticClient.SetDataAsync(historyNodeState);

        var res = await _elasticClient.GetHistoryNodeStateAsync(dBpmnProcess, 0, 1, _filtersProcessStatus);

        Assert.True(resSetDataAsync);
        Assert.NotNull(res);
    }

    [Fact]
    public async Task GetHistoryNodeFromTokenMaskAsync_GetHistory_NotNull()
    {
        var dBpmnProcess = $"test_IdBpmnProcess";
        var historyNodeState = _fixture.Build<HistoryNodeState>()
            .With(p => p.IdBpmnProcess, dBpmnProcess)
            .With(p => p.DateCreated, DateTime.Now.Ticks)
            .With(p => p.DateLastModified, DateTime.Now.Ticks)
            .With(p => p.TokenProcess, DateTime.Now.Ticks.ToString)
            .Create();

        var resSetDataAsync = await _elasticClient.SetDataAsync(historyNodeState);

        var res = await _elasticClient.GetHistoryNodeFromTokenMaskAsync(dBpmnProcess, "*", 1);

        Assert.True(resSetDataAsync);
        Assert.NotNull(res);
    }
}