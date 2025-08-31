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

    public async Task<int> GetCountAllPages(string idActiveProcess)
    {
        if (string.IsNullOrEmpty(idActiveProcess))
        {
            return 0;
        }

        var res = await _elasticClient.GetAllGroupFromTokenAsync(idActiveProcess);

        return res;
    }
    
    public async Task<ListProcessPanelDto[]> GetPagesStates(string idActiveProcess, string lastToken, int countLineOnePage )
    {
        if (string.IsNullOrEmpty(idActiveProcess))
        {
            return [];
        }

        if (string.IsNullOrEmpty(lastToken))
        {
            lastToken = "*";
        }

        if (countLineOnePage == 0)
        {
            countLineOnePage = 10;
        }
        
        var  idHistoryNodeState= await _elasticClient.GetIdHistoryNodeStateAsync(idActiveProcess,lastToken,countLineOnePage);

        var retArray = new List<ListProcessPanelDto>();
        foreach (var id in idHistoryNodeState)
        {
            var historyNode = await _elasticClient.GetDataFromIdAsync<HistoryNodeState>(id,[nameof(HistoryNodeState.NodeStaus)]);
            if (historyNode == null)
            {
                continue;
            }
            var listProcessPanelDto = new ListProcessPanelDto()
            {
                TokenProcess = historyNode.TokenProcess,
                DateCreated = historyNode.DateCreated,
                DateLastModified = historyNode.DateLastModified,
                IdBpmnProcess = historyNode.IdBpmnProcess,
                State = Map(historyNode.ProcessingStaus),
            };
            retArray.Add(listProcessPanelDto);
        }
    

        return retArray.ToArray();
    }

    private ProcessState Map(ProcessingStaus argProcessingStaus)
    {
        return argProcessingStaus switch
        {
            ProcessingStaus.Complete => ProcessState.Completed,
            ProcessingStaus.Failed => ProcessState.Error,
            ProcessingStaus.None => ProcessState.None,
            _ => ProcessState.Running
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