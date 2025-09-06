# BpmnDotNet.Arm.Web

## Назначение:
Web server для отображения состояния процесса.

## Собра для клиента:
~~~
dotnet publish ./BpmnDotNet.Arm.Web/BpmnDotNet.Arm.Web.csproj  -c Release -r linux-x64 -o ./Deploy/linux --self-contained true
~~~

## Настройка:
~~~
SystemConfigure.json
{
  "UseUrls": "http://localhost:5002", //url доступа к сервису.
  "ElasticConnectionUrl":  "http://localhost:9200" //url elastic
}
~~~