using System.Diagnostics;
using BpmnDotNet.Abstractions.Handlers;
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
            TestValue2 = "Call from StartNewProcess"
        };

        //Регистрируем сообщение которое нужно ожидать.
        contextData.RegistrationMessagesType.TryAdd(nameof(ReceiveTaskFirstHandle), typeof(MessageExampleFirst));

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
        var tasks = new List<Task>();
        var startID = 1;

        var totalCount = 10;
        var batchSize = 1;
        var sw = new Stopwatch();
        sw.Restart();
        for (int i = 0; i < totalCount; i += batchSize)
        {
            var currentBatchSize = Math.Min(batchSize, totalCount - i);

            // Обработка текущей партии
            for (int j = i; j < i + currentBatchSize; j++)
            {

                var tokenId = startID + j;
                var timeout = TimeSpan.FromMinutes(10);
                var contextData = CreateContextData(tokenId,dataTimeProcess);
                var taskNode = _bpmnClient.StartNewProcess(contextData, timeout);

                tasks.Add(taskNode.ProcessTask);
            }
            // Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"  Элемент {i}");
        }
        sw.Stop();
        Console.WriteLine($"elapsed time: {sw.ElapsedMilliseconds} ms");

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
        _bpmnClient.Dispose();
    }
}