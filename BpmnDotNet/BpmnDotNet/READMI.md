# BpmnDotNet

## Назначение:
Библиотека для подключения к проектам. Является основным продуктом. Рекомендуется использовать нугет пакетом.


## Собрать пакет:
~~~
 dotnet pack BpmnDotNet/ \
  --configuration Release \
  -o ./nupkgs \
  -p:Version=1.0.23
~~~
## Опубликовать пакет
~~~
export NUGET_PRIVATE="https://172.29.14.53:5002/v3/index.json"
dotnet nuget push -s $NUGET_PRIVATE ./nupkgs/*.nupkg -k " "
~~~