using BpmnDotNet.Interfaces.Handlers;
using Sample.ConsoleApp.Common;
using Sample.ConsoleApp.Context;
using Sample.ConsoleApp.Handlers;
using Sample.ConsoleApp.Messages;

namespace Sample.ConsoleApp;

public class SampleService
{
    private readonly IBpmnClient _bpmnClient;

    public SampleService(IBpmnClient bpmnClient)
    {
        _bpmnClient = bpmnClient;
    }

    private ContextData CreateContextData()
    {
        var contextData = new ContextData
        {
            IdBpmnProcess = Constants.IdBpmnProcessing_2,
            TokenProcess = Guid.NewGuid() + "_test_1",
            TestValue = 25,
            TestValue2 = "Call from StartNewProcess"
        };

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

    public void StartNewProcess()
    {
        var tasks = new List<Task>();

        for (var j = 0; j < 1; j++)
        {
            for (var i = 0; i < 1; i++)
            {
                var timeout = TimeSpan.FromMinutes(10);
                var contextData = CreateContextData();
                var taskNode = _bpmnClient.StartNewProcess(contextData, timeout);
                tasks.Add(taskNode.ProcessTask);
            }

            Task.WaitAll(tasks.ToArray());
        }
    }

    public void SendMessage()
    {
        //Отправим тестовое сообщение
        var messageExampleFirst = CreateMessageExampleFirst();
        var idBpmnProcess = "IdBpmnProcessingMain";
        var tokenProcess = "a81eaccf-e7b1-4c97-b1df-8f163353f678_test_1";

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