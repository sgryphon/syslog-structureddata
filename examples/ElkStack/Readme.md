## Elasticsearch-Logstask-Kibana (ELK) stack example

This example uses the GELF input for logstash to injest data.

You need to be running an ELK server, for example on Linux this can be run in a docker container.

This uses the image from the elk-docker project, sebp/elk (see https://github.com/spujadas/elk-docker).

Note that you will need to ensure the prerequisites are met (see https://elk-docker.readthedocs.io/).

To add the extra logstash input plugin you need to build a new image:

```powershell
sudo docker-compose -f examples/ElkStack/docker/docker-compose.yml build elk
```

Then, to run the image:

```powershell
sudo docker-compose -f examples/ElkStack/docker/docker-compose.yml up
```

You can check the image is running by browsing to `http://localhost:5601`. Kibana should load, but won't show and dashboards until you get some data.

* port 5601 - Kibana web interface
* port 9200 - Elasticsearch JSON interface
* port 9300 - Elasticsearch transport interface (not mapped)
* port 5044 - Logstash beats interface
* port 12201/udp - GELF input plugin

You can stop the container with `^C`, and start it again with `sudo docker start elk`.

Then in another console, run the ElkStack example:

```powershell
dotnet run --project ./examples/ElkStack
```

### Running docker directly

If running docker directly (not via docker-compose):

```powershell
sudo docker pull sebp/elk
sudo docker run --ulimit nofile=65536:65536 -p 5601:5601 -p 9200:9200 -p 5044:5044 sebp/elk 
```

### Troubleshooting

Check ELK image:

```powershell
sudo docker image ls
```

Check ELK container:

```powershell
sudo docker container ls
```

Start a shell in container and check logstash plugins:

```powershell
sudo docker exec -it <container-id> /bin/bash
/opt/logstash/bin/logstash-plugin list
```

To manually create a dummy log entry and check it is working, run the following, then wait for logstash to start, then enter lines of text, e.g. "test log entry":

```powershell
sudo docker exec -it <container-id> /bin/bash
/opt/logstash/bin/logstash --path.data /tmp/logstash/data -e 'input { stdin { } } output { elasticsearch { hosts => ["localhost"] } }'
```

Check Elasticsearch directly (use `q=<search-text>` to search for specific entries):

```
http://localhost:9200/_search?pretty&q=test
```

