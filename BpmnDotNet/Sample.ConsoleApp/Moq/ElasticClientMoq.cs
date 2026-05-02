using BpmnDotNet.ElasticClientDomain.Abstractions;

namespace Sample.ConsoleApp.Moq;

internal class ElasticClientMoq:IElasticClientSetDataAsync
{
    public Task<bool> SetDataAsync<T>(T document, CancellationToken token = default) where T : class
    {
        return Task.FromResult(true);
    }
}