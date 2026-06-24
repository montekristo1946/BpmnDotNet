# BpmnDotNet.Arm.Web

## Назначение:
Web server для отображения состояния процесса.

## Собра для клиента:
~~~
dotnet publish ./BpmnDotNet/BpmnDotNet.Arm.Web/BpmnDotNet.Arm.Web.csproj -c Release -r linux-x64 -o Deployment/BpmnDotNet.Arm.Web --self-contained true
~~~

## Запуск клиента
~~~
./Deployment/BpmnDotNet.Arm.Web/BpmnDotNet.Arm.Web --ElasticConnectionUrl=http://localhost:9200
~~~
## Настройка:
~~~
SystemConfigure.json
{
  "UseUrls": "http://localhost:5002", //url доступа к сервису.
  "ElasticConnectionUrl":  "http://localhost:9200" //url elastic
}
~~~