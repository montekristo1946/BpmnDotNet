# BpmnDotNet.ElasticClient

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
  -v /mnt/Disk_D/TMP/16.08.2025/ElasticDb/:/usr/share/elasticsearch/data \
  docker.elastic.co/elasticsearch/elasticsearch:9.1.2
~~~