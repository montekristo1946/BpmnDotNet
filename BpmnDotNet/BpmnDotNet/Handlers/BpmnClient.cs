namespace BpmnDotNet.Handlers;

using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.Dto;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
internal class BpmnClient : IBpmnClient
{
    /// <summary>
    /// Хранилище процессов.
    /// </summary>
    internal readonly ConcurrentDictionary<(string IdBpmnProcess, string TokenProcess), BusinessProcessJobStatus> BpmnProcesses = new();

    private readonly ConcurrentDictionary<string, BpmnProcessDto> _bpmnProcessDtos = new();
    private readonly Task _cleanerTask;
    private readonly CancellationTokenSource _cts = new();

    private readonly ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>> _handlers = new();
    private readonly ILogger<BpmnClient> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IPathFinder _pathFinder;
    private readonly IHistoryNodeStateWriter _historyNodeStateWriter;
    private readonly IDescriptionWriteService _descriptionWriteService;
    private volatile bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BpmnClient"/> class.
    /// </summary>
    /// <param name="businessProcessDtos">BpmnProcessDtos.</param>
    /// <param name="loggerFactory">ILoggerFactory.</param>
    /// <param name="pathFinder">IPathFinder.</param>
    /// <param name="historyNodeStateWriter">Сервис для записи истории.</param>
    /// <param name="descriptionWriteService">Сервис для регистрации дискрипторов блоков выполенения.</param>
    /// <param name="delayMs">Интервал очистки исполненных тасков. </param>
    public BpmnClient(
        BpmnProcessDto[] businessProcessDtos,
        ILoggerFactory loggerFactory,
        IPathFinder pathFinder,
        IHistoryNodeStateWriter historyNodeStateWriter,
        IDescriptionWriteService descriptionWriteService,
        int delayMs = 1000)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _pathFinder = pathFinder ?? throw new ArgumentNullException(nameof(pathFinder));
        _historyNodeStateWriter =
            historyNodeStateWriter ?? throw new ArgumentNullException(nameof(historyNodeStateWriter));
        _descriptionWriteService =
            descriptionWriteService ?? throw new ArgumentNullException(nameof(descriptionWriteService));
        _ = businessProcessDtos ?? throw new ArgumentNullException(nameof(businessProcessDtos));

        _logger = _loggerFactory.CreateLogger<BpmnClient>();

        FillingBusinessProcessDtos(businessProcessDtos);

        _cleanerTask = Task.Run(() => CleaningBpmnProcesses(_cts.Token, TimeSpan.FromMilliseconds(delayMs)), _cts.Token);
    }

    /// <inheritdoc/>
    public BusinessProcessJobStatus StartNewProcess(IContextBpmnProcess context, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(context);
        var logger = _loggerFactory.CreateLogger<BusinessProcess>();
        var bpmnShema = GetBpmnShema(_bpmnProcessDtos, context.IdBpmnProcess);

        var process = new BusinessProcess(
            context,
            logger,
            bpmnShema,
            _pathFinder,
            _handlers,
            timeout,
            _historyNodeStateWriter);

        var resAdd = BpmnProcesses.TryAdd((context.IdBpmnProcess, context.TokenProcess), process.JobStatus);
        if (resAdd is false)
        {
            throw new InvalidOperationException(
                $"[StartNewProcess] Fail Init new process {context.IdBpmnProcess}, {context.TokenProcess}");
        }

        return process.JobStatus;
    }

    /// <inheritdoc/>
    public void RegisterHandlers<THandler>(THandler[] handlersBpmn)
        where THandler : IBpmnHandler
    {
        ArgumentNullException.ThrowIfNull(handlersBpmn);
        _descriptionWriteService.InitAsync();

        foreach (THandler handler in handlersBpmn)
        {
            var taskDefinitionId = handler.TaskDefinitionId;

            var resAdd = _handlers.TryAdd(taskDefinitionId, handler.AsyncJobHandler);

            if (resAdd is false)
            {
                throw new InvalidOperationException($"[RegisterHandlers] Fail Registration {taskDefinitionId}");
            }

            _descriptionWriteService.AddDescription(handler.TaskDefinitionId, handler.Description);
        }

        _descriptionWriteService.CommitAsync();
    }

    /// <inheritdoc />
    public void SendMessage(string idBpmnProcess, string tokenProcess, Type messageType, object message)
    {
        var resGet = BpmnProcesses.TryGetValue((idBpmnProcess, tokenProcess), out var bpmn);
        if (!resGet || bpmn is null || bpmn.Process is null)
        {
            throw new InvalidOperationException(
                $"[SendMessage] Not find bpmnProcesses: {idBpmnProcess} {tokenProcess}");
        }

        var resAdd = bpmn.Process.AddMessageToQueue(messageType, message);
        if (!resAdd)
        {
            throw new InvalidOperationException(
                $"[SendMessage] Not Add message : {idBpmnProcess} {tokenProcess} {messageType}");
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        ClearBpmnProcessesDictionary(true);

        _cts?.Cancel();
        _cleanerTask.Wait();
        _cleanerTask.Dispose();
        _cts?.Dispose();
    }

    /// <summary>
    /// Очистит кэш от завершенных процессов.
    /// </summary>
    /// <param name="isForce">Форсированный запуск.</param>
    internal void ClearBpmnProcessesDictionary(bool isForce = false)
    {
        foreach (var status in BpmnProcesses.Values)
        {
            if (status.StatusType != StatusType.Completed
                && status.StatusType != StatusType.Failed
                && !isForce)
            {
                continue;
            }

            var resRemote = BpmnProcesses.TryRemove((status.IdBpmnProcess, status.TokenProcess), out var processJob);
            if (!resRemote)
            {
                _logger.LogWarning(
                    "Could not delete the process {IdBpmnProcess} {TokenProcess}",
                    status.IdBpmnProcess,
                    status.TokenProcess);
            }
            else
            {
                processJob?.Process?.ForceCancel();
                processJob?.Process?.Dispose();
                _logger.LogDebug(
                    "Delete the process {IdBpmnProcess} {TokenProcess} {StatusType}",
                    status.IdBpmnProcess,
                    status.TokenProcess,
                    status.StatusType);
            }
        }
    }

    private async Task CleaningBpmnProcesses(CancellationToken cts, TimeSpan delay)
    {
        try
        {
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(delay, cts);

                ClearBpmnProcessesDictionary();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[CleaningBpmnProcesses] Cancel Thread ");
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }

    private void FillingBusinessProcessDtos(BpmnProcessDto[] businessProcessDtos)
    {
        foreach (var businessProcessDto in businessProcessDtos)
        {
            var resAdd = _bpmnProcessDtos.TryAdd(businessProcessDto.IdBpmnProcess, businessProcessDto);
            if (resAdd is false)
            {
                throw new InvalidOperationException($"Fail load {businessProcessDto.IdBpmnProcess}");
            }
        }
    }

    private BpmnProcessDto GetBpmnShema(
        ConcurrentDictionary<string, BpmnProcessDto> bpmnProcessDtos,
        string idBpmnProcess)
    {
        if (!bpmnProcessDtos.TryGetValue(idBpmnProcess, out var bpmn))
        {
            throw new InvalidOperationException($"Not find BpmnProcessDto: {idBpmnProcess}");
        }

        return bpmn;
    }
}