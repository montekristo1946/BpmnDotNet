using BpmnDotNet.BusinessProcessDomain;
using BpmnDotNet.BusinessProcessDomain.Activity;
using BpmnDotNet.BusinessProcessDomain.Dto;
using Xunit.Abstractions;

namespace BpmnDotNetTests.BpmnEngineDomain;

public class BpmnEngineIntegrationTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public BpmnEngineIntegrationTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task StartProcessAsync_CheckParallelGateway()
    {
        var processModel = new ProcessModel
        {
            Id = "TestProcess",
            StartNodeId = "start",
            Nodes = new Dictionary<string, BpmnNode>
            {
                ["start"] = new StartEvent { Id = "start", Name = "Начало" },

                ["parallel_split"] = new ParallelGateway
                {
                    Id = "parallel_split",
                    Name = "Разветвление",
                    IsSplit = true
                },

                ["task1"] = new ServiceTask
                {
                    Id = "task1",
                    Name = "Задача 1",
                    Handler = async (token, instance) =>
                    {
                        _testOutputHelper.WriteLine("Выполняется задача 1");
                        await Task.Delay(100);
                    }
                },

                ["task2"] = new ServiceTask
                {
                    Id = "task2",
                    Name = "Задача 2",
                    Handler = async (token, instance) =>
                    {
                        _testOutputHelper.WriteLine("Выполняется задача 2");
                        await Task.Delay(100);
                    }
                },

                ["receive"] = new ReceiveTask
                {
                    Id = "receive",
                    Name = "Ожидание сообщения",
                    MessageName = "ApprovalMessage"
                },

                ["parallel_join"] = new ParallelGateway
                {
                    Id = "parallel_join",
                    Name = "Слияние",
                    IsSplit = false
                },

                ["end"] = new EndEvent { Id = "end", Name = "Конец" }
            },
            Flows = new List<Flow>
            {
                new() { Id = "f1", SourceId = "start", TargetId = "parallel_split" },
                new() { Id = "f2", SourceId = "parallel_split", TargetId = "task1" },
                new() { Id = "f3", SourceId = "parallel_split", TargetId = "task2" },
                new() { Id = "f4", SourceId = "task1", TargetId = "receive" },
                new() { Id = "f5", SourceId = "task2", TargetId = "parallel_join" },
                new() { Id = "f6", SourceId = "receive", TargetId = "parallel_join" },
                new() { Id = "f7", SourceId = "parallel_join", TargetId = "end" }
            }
        };

        var engine = new BpmnEngine();

        // Запускаем процесс
        _testOutputHelper.WriteLine("=== Запуск процесса ===");
        var instance = await engine.StartProcessAsync(processModel);

        // Ждём немного, потом отправляем сообщение
        await Task.Delay(500);
        _testOutputHelper.WriteLine("\n=== Отправка сообщения ===");
        engine.SendMessage("ApprovalMessage", instance.InstanceId,
            new Dictionary<string, object> { ["approved"] = true });

        // Запускаем цикл снова, чтобы обработать пробуждённый токен
        await engine.RunEventLoopAsync();

        _testOutputHelper.WriteLine("\n=== Процесс завершён ===");
    }
    
     [Fact]
    public async Task StartProcessAsync_CheckExclusiveGateway()
    {
        var processModel = new ProcessModel
        {
            Id = "TestProcess",
            StartNodeId = "start",
            Nodes = new Dictionary<string, BpmnNode>
            {
                ["start"] = new StartEvent { Id = "start", Name = "Начало" },

                ["ExclusiveGateway_1"] = new ExclusiveGateway()
                {
                    Id = "ExclusiveGateway_1",
                    Name = "Эксклюзивный шлюз",
                },
                ["ExclusiveGateway_2"] = new ExclusiveGateway()
                {
                    Id = "ExclusiveGateway_2",
                    Name = "Эксклюзивный шлюз",
                },

                ["task1"] = new ServiceTask
                {
                    Id = "task1",
                    Name = "Задача 1",
                    Handler = async (token, instance) =>
                    {
                        _testOutputHelper.WriteLine("Выполняется задача 1");
                        await Task.Delay(100);
                    }
                },

                ["task2"] = new ServiceTask
                {
                    Id = "task2",
                    Name = "Задача 2",
                    Handler = async (token, instance) =>
                    {
                        _testOutputHelper.WriteLine("Выполняется задача 2");
                        await Task.Delay(100);
                    }
                },

                ["end"] = new EndEvent { Id = "end", Name = "Конец" }
            },
            Flows = new List<Flow>
            {
                new() { Id = "f1", SourceId = "start", TargetId = "ExclusiveGateway_1" },
                new() { Id = "f2", SourceId = "ExclusiveGateway_1", TargetId = "task1" },
                new() { Id = "f3", SourceId = "ExclusiveGateway_1", TargetId = "task2" },
                new() { Id = "f4", SourceId = "task1", TargetId = "ExclusiveGateway_2" },
                new() { Id = "f5", SourceId = "task2", TargetId = "ExclusiveGateway_2" },
                new() { Id = "f6", SourceId = "ExclusiveGateway_2", TargetId = "end" }
            }
        };

        var engine = new BpmnEngine();

        // Запускаем процесс
        _testOutputHelper.WriteLine("=== Запуск процесса ===");
        var instance = await engine.StartProcessAsync(processModel);
        _testOutputHelper.WriteLine("\n=== Процесс завершён ===");
    }
}

