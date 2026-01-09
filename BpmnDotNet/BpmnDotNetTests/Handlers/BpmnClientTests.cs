using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.Dto;
using BpmnDotNet.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.Handlers;

public class BpmnClientTests
{
    private readonly BpmnProcessDto[] businessProcessDtos;
    private readonly ILoggerFactory loggerFactory;
    private readonly  IPathFinder pathFinder;
    private readonly   IHistoryNodeStateWriter historyNodeStateWriter;
    private readonly IDescriptionWriteService descriptionWriteService;
    private readonly BpmnClient bpmnClient;
    
    public BpmnClientTests()
    {
        businessProcessDtos = new BpmnProcessDto[]{};
        loggerFactory  = Substitute.For<ILoggerFactory>();
        pathFinder = Substitute.For<IPathFinder>();
        historyNodeStateWriter = Substitute.For<IHistoryNodeStateWriter>();
        descriptionWriteService = Substitute.For<IDescriptionWriteService>();
        bpmnClient = new BpmnClient(businessProcessDtos,loggerFactory, pathFinder, historyNodeStateWriter, descriptionWriteService,10000);
    }

    [Fact]
    public void ClearBpmnProcessesDictionary_FullPassNormalFinalizedProcess_CountProcessInBpmnProcesses()
    {
        var idBpmnProcess = Guid.NewGuid().ToString();
        var tokenProcess = Guid.NewGuid().ToString();
        var newBusinessProcess = new BusinessProcessJobStatus()
        {
            StatusType = StatusType.Completed,
            IdBpmnProcess = idBpmnProcess,
            TokenProcess = tokenProcess
        };
        
       var resAdd = bpmnClient.BpmnProcesses.TryAdd((idBpmnProcess, tokenProcess), newBusinessProcess);

       bpmnClient.ClearBpmnProcessesDictionary();
       
       Assert.True(resAdd);
       Assert.Empty(bpmnClient.BpmnProcesses.Keys);
       
       bpmnClient.Dispose();
    }
    
    [Fact]
    public void ClearBpmnProcessesDictionary_NoNormalCompletedFinalizedProcess_CountProcessInBpmnProcesses()
    {
        var idBpmnProcess = Guid.NewGuid().ToString();
        var tokenProcess = Guid.NewGuid().ToString();
        var newBusinessProcess = new BusinessProcessJobStatus()
        {
            StatusType = StatusType.Failed,
            IdBpmnProcess = idBpmnProcess,
            TokenProcess = tokenProcess
        };
        
        var resAdd = bpmnClient.BpmnProcesses.TryAdd((idBpmnProcess, tokenProcess), newBusinessProcess);

        bpmnClient.ClearBpmnProcessesDictionary();
       
        Assert.True(resAdd);
        Assert.Empty(bpmnClient.BpmnProcesses.Keys);
       
        bpmnClient.Dispose();
    }
    
    [Fact]
    public void ClearBpmnProcessesDictionary_CheckForceCall_CountProcessInBpmnProcesses()
    {
        var idBpmnProcess = Guid.NewGuid().ToString();
        var tokenProcess = Guid.NewGuid().ToString();
        var newBusinessProcess = new BusinessProcessJobStatus()
        {
            StatusType = StatusType.Pending,
            IdBpmnProcess = idBpmnProcess,
            TokenProcess = tokenProcess
        };
        
        var resAdd = bpmnClient.BpmnProcesses.TryAdd((idBpmnProcess, tokenProcess), newBusinessProcess);

        bpmnClient.ClearBpmnProcessesDictionary(true);
       
        Assert.True(resAdd);
        Assert.Empty(bpmnClient.BpmnProcesses.Keys);
       
        bpmnClient.Dispose();
    }
    
    [Fact]
    public void ClearBpmnProcessesDictionary_CheckWaitPending_CountProcessInBpmnProcesses()
    {
        var idBpmnProcess = Guid.NewGuid().ToString();
        var tokenProcess = Guid.NewGuid().ToString();
        var newBusinessProcess = new BusinessProcessJobStatus()
        {
            StatusType = StatusType.Pending,
            IdBpmnProcess = idBpmnProcess,
            TokenProcess = tokenProcess
        };
        
        var resAdd = bpmnClient.BpmnProcesses.TryAdd((idBpmnProcess, tokenProcess), newBusinessProcess);

        bpmnClient.ClearBpmnProcessesDictionary();
       
        Assert.True(resAdd);
        Assert.Single(bpmnClient.BpmnProcesses.Keys);
       
        bpmnClient.Dispose();
    }
}