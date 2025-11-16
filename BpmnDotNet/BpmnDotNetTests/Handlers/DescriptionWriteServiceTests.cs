using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.Entities;
using BpmnDotNet.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.Handlers;

public class DescriptionWriteServiceTests
{
    private readonly IElasticClientSetDataAsync _elasticClient;
    private readonly ILogger<DescriptionWriteService> _logger;
    private readonly DescriptionWriteService _descriptionWriteService;

    public DescriptionWriteServiceTests()
    {
        _elasticClient = Substitute.For<IElasticClientSetDataAsync>();
        _logger = Substitute.For<ILogger<DescriptionWriteService>>();
        _descriptionWriteService = new DescriptionWriteService(_elasticClient, _logger);
    }

    [Fact]
    public async Task Commit_CheckFillDatabase_CallMoq()
    {
        _descriptionWriteService.AddDescription("testId1", "test_message1");
        _descriptionWriteService.AddDescription("testId2", "test_message2");

        await _descriptionWriteService.Commit();
        
        await _elasticClient.Received(2).SetDataAsync(Arg.Any<DescriptionData>());
    }

    [Fact]
    public async Task Init_CheckFillDatabase_CallMoq()
    {
        _descriptionWriteService.AddDescription("testId1", "test_message1");
        _descriptionWriteService.AddDescription("testId2", "test_message2");

        await _descriptionWriteService.Init();
        await _descriptionWriteService.Commit();
        
        await _elasticClient.Received(0).SetDataAsync(Arg.Any<DescriptionData>());
    }
}