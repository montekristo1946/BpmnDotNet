using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.Dto;
using Microsoft.Extensions.Logging;

namespace BpmnDotNet.Arm.Core.Handlers;

public class ListProcessPanelHandler : IListProcessPanelHandler
{
    private readonly ILogger<ListProcessPanelHandler> _logger;
    private readonly IElasticClient _elasticClient;

    public ListProcessPanelHandler(ILogger<ListProcessPanelHandler> logger, IElasticClient elasticClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
    }

    public async Task<int> GetCountAllPages(string idActiveProcess, string[] processStatus = null)
    {
        if (string.IsNullOrEmpty(idActiveProcess))
        {
            return 0;
        }

        var res = await _elasticClient.GetCountHistoryNodeStateAsync(idActiveProcess, processStatus);

        return res;
    }

    public async Task<ListProcessPanelDto[]> GetPagesStates(
        string idBpmnProcess,
        int from,
        int size,
        string[] processStatus = null)
    {
        if (string.IsNullOrEmpty(idBpmnProcess))
        {
            return [];
        }

        if (size == 0)
        {
            return [];
        }

        var historyNodes =
            await _elasticClient.GetHistoryNodeStateAsync(idBpmnProcess, from, size, processStatus);

        var retArray = new List<ListProcessPanelDto>();
        foreach (var historyNode in historyNodes)
        {
            var listProcessPanelDto = new ListProcessPanelDto()
            {
                TokenProcess = historyNode.TokenProcess,
                DateCreated = new DateTime(historyNode.DateCreated),
                DateLastModified = new DateTime(historyNode.DateLastModified),
                IdBpmnProcess = historyNode.IdBpmnProcess,
                State = Map(historyNode.ProcessStatus),
                IdStorageHistoryNodeState = historyNode.Id,
            };
            retArray.Add(listProcessPanelDto);
        }


        return [..retArray];
    }

    public async Task<string[]> GetErrors(string idUpdateNodeJobStatus)
    {
        var historyNodeState = await _elasticClient.GetDataFromIdAsync<HistoryNodeState>(idUpdateNodeJobStatus,
            [nameof(HistoryNodeState.NodeStaus)]) ?? new HistoryNodeState();

        return historyNodeState?.ArrayMessageErrors ?? [];
    }

    public async Task<ListProcessPanelDto[]> GetHistoryNodeFromTokenMaskAsync(string idBpmnProcess, string filterToken, int sizeSample)
    {
        var historyNodes = await _elasticClient.GetHistoryNodeFromTokenMaskAsync(idBpmnProcess,filterToken,sizeSample);
        var retArray = new List<ListProcessPanelDto>();
       
        foreach (var historyNode in historyNodes)
        {
            var listProcessPanelDto = new ListProcessPanelDto()
            {
                TokenProcess = historyNode.TokenProcess,
                DateCreated = new DateTime(historyNode.DateCreated),
                DateLastModified = new DateTime(historyNode.DateLastModified),
                IdBpmnProcess = historyNode.IdBpmnProcess,
                State = Map(historyNode.ProcessStatus),
                IdStorageHistoryNodeState = historyNode.Id,
            };
            retArray.Add(listProcessPanelDto);
        }


        return [..retArray];
    }

    private ProcessState Map(ProcessStatus argStatusType)
    {
        return argStatusType switch
        {
            ProcessStatus.Completed => ProcessState.Completed,
            ProcessStatus.Error => ProcessState.Error,
            ProcessStatus.Works => ProcessState.Works,
            ProcessStatus.None => ProcessState.None,
            _ => ProcessState.Works
        };
    }


    /*  public Task<ListProcessPanelDto[]> GetStates(string idActiveProcess)
      {
          var retResult = new List<ListProcessPanelDto>();
          var random = new Random();



          var elementDemo = new ListProcessPanelDto()
          {
              IdBpmnProcess = $"Архив метеорологических наблюдений и прогностических данных о многолетних климатических изменениях и экстремальных погодных явлениях",
              TokenProcess = $"ID Синоптический анализ атмосферных фронтов, циркуляционных процессов в тропосфере и стратосфере, термобарических аномалий и их влияния на сезонные колебания температурного режим",
              DateCreated = DateTime.Now,
              State = ProcessState.Completed,
              DateLastModified = DateTime.Now,
          };
          // retResult.Add(elementDemo);

          for (int i = 0; i < 100; i++)
          {
              var randomNumber = random.Next();
              var randomHours = random.Next(1, 100);
              var randomEnd = random.Next(1, 100);
              var randomState = random.Next(0, 4);
              var element = new ListProcessPanelDto()
              {
                  IdBpmnProcess = $"IdBpmnProcess_{i}",
                  TokenProcess = $"ID paravoz {randomNumber}",
                  DateCreated = DateTime.Now.AddDays(-1).AddHours(randomHours),
                  State = (ProcessState)randomState,
                  DateLastModified = DateTime.Now.AddDays(-1).AddHours(randomEnd),
              };
              retResult.Add(element);

          }

          return Task.FromResult(retResult.ToArray());
      }*/
}