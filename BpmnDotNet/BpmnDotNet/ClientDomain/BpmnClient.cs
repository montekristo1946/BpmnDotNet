namespace BpmnDotNet.Handlers;

using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.BpmnEngineDomain;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNet.ClientDomain.Abstractions;
using BpmnDotNet.Dto;
using BpmnDotNet.HistoryDomain.Abstractions;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
internal class BpmnClient : IBpmnClient
{
    /// <summary>
    /// Хранилище процессов.
    /// </summary>
    internal readonly ConcurrentDictionary<(string IdBpmnProcess, string TokenProcess), BusinessProcessJobStatus>
        BpmnProcesses = new();

    private readonly ConcurrentDictionary<string, BpmnProcessDto> _bpmnProcessDtos = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>> _handlers = new();
    private readonly ILogger<BpmnClient> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IHistoryNodeStateWriter _historyNodeStateWriter;
    private readonly IDescriptionWriteService _descriptionWriteService;
    private readonly IProcessModelBuilder _processModelBuilder;
    private readonly TimeSpan _delayClearOldProcess;
    private int _disposed = 0;
    private Task? _cleanerTask = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="BpmnClient"/> class.
    /// </summary>
    /// <param name="loggerFactory">ILoggerFactory.</param>
    /// <param name="historyNodeStateWriter">Сервис для записи истории.</param>
    /// <param name="descriptionWriteService">Сервис для регистрации дискрипторов блоков выполнения.</param>
    /// <param name="processModelBuilder"><see cref="IProcessModelBuilder"/>.</param>
    /// <param name="delayClearOldProcess">Интервал очистки исполненных тасков. </param>
    public BpmnClient(
        ILoggerFactory loggerFactory,
        IHistoryNodeStateWriter historyNodeStateWriter,
        IDescriptionWriteService descriptionWriteService,
        IProcessModelBuilder processModelBuilder,
        TimeSpan delayClearOldProcess = default)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _historyNodeStateWriter =
            historyNodeStateWriter ?? throw new ArgumentNullException(nameof(historyNodeStateWriter));
        _descriptionWriteService =
            descriptionWriteService ?? throw new ArgumentNullException(nameof(descriptionWriteService));
        _processModelBuilder = processModelBuilder ?? throw new ArgumentNullException(nameof(processModelBuilder));

        _logger = _loggerFactory.CreateLogger<BpmnClient>();

        _delayClearOldProcess = delayClearOldProcess == TimeSpan.Zero ? TimeSpan.FromSeconds(1) : delayClearOldProcess;
    }

    /// <summary>
    /// Запустить фоновый поток очистки кэша.
    /// </summary>
    public void StartCleanerBackgroundThead()
    {
        _cleanerTask = Task.Run(() => CleaningBpmnProcesses(_cts.Token, _delayClearOldProcess), _cts.Token);
    }

    /// <summary>
    /// Зальет BpmnProcessDto в кэш.
    /// </summary>
    /// <param name="businessProcessDtos"><see cref="BpmnProcessDto"/>.</param>
    public void FillingBusinessProcessDtos(BpmnProcessDto[] businessProcessDtos)
    {
        foreach (var businessProcessDto in businessProcessDtos)
        {
            if (_bpmnProcessDtos.TryGetValue(businessProcessDto.IdBpmnProcess, out var existingProcess))
            {
                _logger.LogError(
                    "Failed to load process with ID {IdBpmnProcess}. A process with this ID already exists. " +
                    "Existing process: {ExistingProcessDescription}, New process: {NewProcessDescription}.",
                    businessProcessDto.IdBpmnProcess,
                    existingProcess.IdBpmnProcess ?? "Empty IdBpmnProcess",
                    businessProcessDto.IdBpmnProcess ?? "Empty IdBpmnProcess");
                throw new InvalidOperationException($"Fail, duplicate key load {businessProcessDto.IdBpmnProcess}");
            }

            var resAdd = _bpmnProcessDtos.TryAdd(businessProcessDto.IdBpmnProcess, businessProcessDto);
            if (resAdd is false)
            {
                throw new InvalidOperationException($"Fail load {businessProcessDto.IdBpmnProcess}");
            }
        }
    }

    /// <inheritdoc/>
    public async Task<BusinessProcessJobStatus> StartNewProcessAsync(
        IContextBpmnProcess context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        var logger = _loggerFactory.CreateLogger<BpmnEngine>();
        var processInstance = new BpmnEngine(logger, _historyNodeStateWriter);
        var bpmnShema = GetBpmnShema(_bpmnProcessDtos, context.IdBpmnProcess);
        var processModel = _processModelBuilder.Build(bpmnShema, _handlers);
        var retValue = await processInstance.StartProcessAsync(context, processModel, cancellationToken);

        BpmnProcesses[(context.IdBpmnProcess, context.TokenProcess)] = retValue;

        return retValue;
    }

    /// <inheritdoc/>
    public void RegisterHandlers<THandler>(THandler[] handlersBpmn)
        where THandler : IBpmnHandler
    {
        ArgumentNullException.ThrowIfNull(handlersBpmn);
        _descriptionWriteService.InitNewInstance();

        foreach (THandler handler in handlersBpmn)
        {
            var taskDefinitionId = handler.TaskDefinitionId;
            if (taskDefinitionId is null)
            {
                throw new InvalidOperationException(
                    $"[BpmnClient:RegisterHandlers] {handler.GetType().Name} TaskDefinitionId is null");
            }

            if (handler.Description is null)
            {
                throw new InvalidOperationException(
                    $"[BpmnClient:RegisterHandlers] {handler.GetType().Name} Description is null");
            }

            if (_handlers.ContainsKey(taskDefinitionId))
            {
                throw new InvalidOperationException(
                    $"[BpmnClient:RegisterHandlers] Handler for TaskDefinitionId: {taskDefinitionId} is already registered");
            }

            var resAdd = _handlers.TryAdd(taskDefinitionId, handler.ActivityHandlerAsync);

            if (resAdd is false)
            {
                throw new InvalidOperationException(
                    $"[BpmnClient:RegisterHandlers] Fail Registration {taskDefinitionId} Handler:{handler.GetType().Name}");
            }

            _descriptionWriteService.AddDescription(handler.TaskDefinitionId, handler.Description);
            _logger.LogInformation(
                $"[BpmnClient:RegisterHandlers] Registration completed; Handler:{handler.GetType().Name}; TaskDefinitionId: {taskDefinitionId}");
        }

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        _descriptionWriteService.CommitAsync(cts.Token).Wait(cts.Token);
    }

    /// <inheritdoc />
    public void SendMessage(string idBpmnProcess, string tokenProcess, Type messageType, object message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idBpmnProcess);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenProcess);
        ArgumentNullException.ThrowIfNull(messageType);
        ArgumentNullException.ThrowIfNull(message);

        var resGet = BpmnProcesses.TryGetValue((idBpmnProcess, tokenProcess), out var bpmn);
        if (!resGet || bpmn is null || bpmn?.Process is null || bpmn.Process.IsProcessCancel)
        {
            throw new InvalidOperationException(
                $"[BpmnClient:SendMessage] Not find bpmnProcesses: {idBpmnProcess} {tokenProcess}");
        }

        var resAdd = bpmn.Process.AddMessageToQueue(messageType, message);
        if (!resAdd)
        {
            throw new InvalidOperationException(
                $"[BpmnClient:SendMessage] Not Add message : {idBpmnProcess} {tokenProcess} {messageType}");
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        await ClearBpmnProcessesDictionaryAsync(true);
        await _cts.CancelAsync();

        try
        {
            if (_cleanerTask is not null)
            {
                await _cleanerTask.WaitAsync(TimeSpan.FromSeconds(5));
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[BpmnClient:Dispose] OperationCanceledException");
        }
        catch (AggregateException agg)
            when (agg.InnerExceptions.All(e => e is OperationCanceledException or TaskCanceledException))
        {
            _logger.LogWarning("[BpmnClient:Dispose] Task was canceled (AggregateException)");
        }

        _cleanerTask?.Dispose();
        _cts.Dispose();
    }

    /// <summary>
    /// Очистит кэш от завершенных процессов.
    /// </summary>
    /// <param name="isForce">Форсированный запуск.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal virtual async Task ClearBpmnProcessesDictionaryAsync(bool isForce = false)
    {
        foreach (var status in BpmnProcesses.Values)
        {
            var process = status.Process;
            if (!process.IsProcessCancel && !isForce)
            {
                continue;
            }

            var resRemote = BpmnProcesses.TryRemove((status.IdBpmnProcess, status.TokenProcess), out var processJob);
            if (!resRemote || processJob is null || processJob.Process is null)
            {
                _logger.LogWarning(
                    "Could not delete the process {IdBpmnProcess} {TokenProcess}",
                    status.IdBpmnProcess,
                    status.TokenProcess);
            }
            else
            {
                await processJob.Process.DisposeAsync();

                _logger.LogDebug(
                    "Delete the process {IdBpmnProcess} {TokenProcess}",
                    status.IdBpmnProcess,
                    status.TokenProcess);
            }
        }
    }

    /// <summary>
    /// Фоновая очистка кэша.
    /// </summary>
    /// <param name="cts"><see cref="CancellationToken"/>.</param>
    /// <param name="delay"><see cref="TimeSpan"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal virtual async Task CleaningBpmnProcesses(CancellationToken cts, TimeSpan delay)
    {
        try
        {
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(delay, cts);

                await ClearBpmnProcessesDictionaryAsync();
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


    /// <summary>
    /// Запрос BpmnProcessDto с логированием.
    /// </summary>
    /// <param name="bpmnProcessDtos">Словарь локального кэширования.</param>
    /// <param name="idBpmnProcess">id процесса.</param>
    /// <returns><see cref="BpmnProcessDto"/>.</returns>
    internal BpmnProcessDto GetBpmnShema(
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