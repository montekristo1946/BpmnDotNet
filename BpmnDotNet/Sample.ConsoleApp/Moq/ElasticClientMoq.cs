using BpmnDotNet.Common.Abstractions;

namespace Sample.ConsoleApp.Moq;

public class ElasticClientMoq:IElasticClientSetDataAsync
{
    public Task<bool> SetDataAsync<T>(T document, CancellationToken token = default) where T : class
    {
        return Task.FromResult(true);
    }
}