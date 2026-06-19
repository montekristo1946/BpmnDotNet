using System.Diagnostics;
using System.Globalization;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.ClientDomain.Abstractions;
using Sample.ConsoleApp.Common;
using Sample.ConsoleApp.Context;
using Sample.ConsoleApp.Handlers;
using Sample.ConsoleApp.Messages;

namespace Sample.ConsoleApp;

internal class SampleService
{
    private readonly IBpmnClient _bpmnClient;

    public SampleService(IBpmnClient bpmnClient)
    {
        _bpmnClient = bpmnClient;
    }

    private ContextData CreateContextData(int tokenId, DateTime dataTimeProcess)
    {

        var tokenProcess = $"Train_{dataTimeProcess.Ticks}_{tokenId}";

        var contextData = new ContextData
        {
            IdBpmnProcess = Constants.IdBpmnProcessingMain,
            TokenProcess = tokenProcess,
            TestValue = 25,
            TestValue2 = "Call from StartNewProcess",
            RegistrationMessagesType = new(),
            ConditionRoute = new(),
            ReceivedMessage = new(),
        };

        //Регистрируем сообщение которое нужно ожидать.
        contextData.RegistrationMessagesType.TryAdd(typeof(MessageExampleFirst),nameof(ReceiveTaskFirstHandle));

        return contextData;
    }

    private object CreateMessageExampleFirst()
    {
        return new MessageExampleFirst
        {
            Age = (int)DateTime.Now.Ticks,
            Name = "John Doe",
            Email = "email test"
        };
    }

    public void StartNewProcess(DateTime dataTimeProcess)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        var contextData = CreateContextData(1,dataTimeProcess);
        _bpmnClient.StartNewProcessAsync(contextData, cts.Token).Wait(cts.Token);
        
        Console.WriteLine($"The method StartNewProcess is complete");
    }

    public void SendMessage(DateTime dataTimeProcess)
    {
        //Отправим тестовое сообщение
        var messageExampleFirst = CreateMessageExampleFirst();
        var idBpmnProcess = "IdBpmnProcessingMain";
        var tokenId = 1;
        var tokenProcess = $"Train_{dataTimeProcess.Ticks}_{tokenId}";

        try
        {
            _bpmnClient.SendMessage(
                idBpmnProcess,
                tokenProcess,
                typeof(MessageExampleFirst),
                messageExampleFirst);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Stop()
    {
        _bpmnClient.DisposeAsync();
    }

    public void StartMultiInput()
    {
        // var dataTimeProcess = DateTime.Now;
        // Console.WriteLine($"Start Multi input: {dataTimeProcess:yyyy-MM-dd HH:mm:ss.fff} ms");
        // var timeout = TimeSpan.FromMinutes(10);
        // var tokenProcess = $"Train_{dataTimeProcess.Ticks}_{dataTimeProcess.ToString(CultureInfo.InvariantCulture)}";
        //
        // var contextData = new ContextData
        // {
        //     IdBpmnProcess = Constants.TestMultiInputProcess,
        //     TokenProcess = tokenProcess,
        //     TestValue = 0,
        //     TestValue2 = "Call from StartNewProcess"
        // };
        // var taskNode = _bpmnClient.StartNewProcess(contextData, timeout);
        //
        // taskNode.ProcessTask.Wait();
        //
        // Console.WriteLine($"End Multi input: {dataTimeProcess:yyyy-MM-dd HH:mm:ss.fff} ms");
        throw  new NotImplementedException();
    }

    public void StressTest()
    {
      
        // var startID = 1;
        //
        // var totalCount = 10000;
        // var batchSize = 10;
        // var sw = new Stopwatch();
        // var testTime = DateTime.Now;
        // sw.Restart();
        // for (int i = 0; i < totalCount; i += batchSize)
        // {
        //     var currentBatchSize = Math.Min(batchSize, totalCount - i);
        //     var tasks = new List<Task>();
        //     // Обработка текущей партии
        //     for (int j = i; j < i + currentBatchSize; j++)
        //     {
        //         var tokenId = startID + j;
        //         var timeout = TimeSpan.FromMinutes(10);
        //         var contextData = CreateContextData(tokenId,testTime);
        //         var taskNode = _bpmnClient.StartNewProcess(contextData, timeout);
        //
        //         tasks.Add(taskNode.ProcessTask);
        //     }
        //     Task.WaitAll(tasks.ToArray());
        //     Console.WriteLine($"--- Партия ---  {i}");
        // }
        // sw.Stop();
        // Console.WriteLine($"elapsed time: {sw.ElapsedMilliseconds} ms");
        throw new NotImplementedException();
    }
}