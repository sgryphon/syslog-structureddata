## Elasticsearch-Logstask-Kibana (ELK) stack example


You need to be running an ELK server, for example on Linux this can be run
in a docker container:

```powershell
sudo docker-compose -f examples/ElkStack/docker-compose.yml up elk
```

This uses the image from the elk-docker project, sebp/elk (see https://github.com/spujadas/elk-docker).

Note that you will need to ensure the prerequisites are met (see https://elk-docker.readthedocs.io/).

You can check the image is running by browsing to `http://localhost:5601`. Kibana should load, but won't show and dashboards until you get some data.

You can stop the container with `^C`, and start it again with `sudo docker start elk`.

Then in another console, run the ElkStack example:

```powershell
dotnet run --project ./examples/EklStack
```


### Running docker directly

If running docker directly (not via docker-compose):

```powershell
sudo docker pull sebp/elk
sudo docker run --ulimit nofile=65536:65536 -p 5601:5601 -p 9200:9200 -p 5044:5044 sebp/elk 
```
