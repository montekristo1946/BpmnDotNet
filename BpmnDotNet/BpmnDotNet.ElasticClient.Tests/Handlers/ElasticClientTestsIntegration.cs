using System.Globalization;
using AutoFixture;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.BPMNDiagram;
using BpmnDotNet.ElasticClient;
using BpmnDotNet.ElasticClient.Handlers;
using Microsoft.Extensions.Logging;

namespace BpmnDotNetElasticClientTests.Handlers;

public class ElasticClientTestsIntegration
{
    private readonly ElasticClient _elasticClient;
    private readonly ILoggerFactory loggerFactory;
    private readonly Fixture _fixture;

    public ElasticClientTestsIntegration()
    {
        var config = new ElasticClientConfig { ConnectionString = "http://localhost:9200" };
        var logger = LoggerFactory.Create(builder => { builder.AddConsole(); }).CreateLogger<IElasticClient>();
        _elasticClient = new ElasticClient(config, logger);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task SetDataAsync_cheksLoadPlaneInDatabase_Bool()
    {
        var name = $"Name_{DateTime.Now.ToString("MM.dd.yyyy_HH:mm:ss", CultureInfo.InvariantCulture)}";
        var plane = _fixture.Build<BpmnPlane>().With(p => p.Name, name).Create();

        var res = await _elasticClient.SetDataAsync(plane);
     
        Assert.True(res);
    }

    [Fact]
    public async Task GetConditionRouteWithExclusiveGateWay_CheckFalsePath_TruePath()
    {
        var getParams = new[] { nameof(BpmnPlane.IdBpmnProcess), nameof(BpmnPlane.Name) };
        var res = await _elasticClient.GetAllFieldsAsync<BpmnPlane>(getParams, 100);

        Assert.NotEmpty(res);
    }
}