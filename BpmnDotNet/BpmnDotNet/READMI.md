# BpmnDotNet

## Назначение:
Библиотека для подключения к проектам. Является основным продуктом. Рекомендуется использовать нугет пакетом.


## Собрать пакет:
~~~
 dotnet pack BpmnDotNet/ \
  --configuration Release \
  -o ./nupkgs \
  -p:Version=1.0.26
~~~
## Опубликовать пакет
~~~
export NUGET_PRIVATE="https://172.29.14.53:5002/v3/index.json"
dotnet nuget push -s $NUGET_PRIVATE ./nupkgs/*.nupkg -k " "
~~~


# ElasticClient

## Назначение:
Библиотека для работы с elastic. Реализует интерфейса IElasticClient.

## Запустить эластику.
~~~
docker run --rm  \
  --name elasticsearch \
  -p 9200:9200 \
  -p 9300:9300 \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  -e "cluster.routing.allocation.disk.threshold_enabled=false" \
  -e "ES_JAVA_OPTS=-Xms512m -Xmx512m" \
  -v /mnt/Disk_D/TMP/15.11.2025/ElasticDb/:/usr/share/elasticsearch/data \
  -v /mnt/Disk_D/TMP/15.11.2025/ElasticsearchLog/:/usr/share/elasticsearch/logs \
  docker.elastic.co/elasticsearch/elasticsearch:9.1.2
~~~

## Состояние эластики можно смотреть
Утилита администрирования: [elasticvue_1.8.0_amd64.AppImage](https://github.com/cars10/elasticvue)


## Регистрация в продуктовом сервисе:
~~~
services.AddScoped<ElasticClientConfig>();
services.AddSingleton<IElasticClient, ElasticClient>();
~~~