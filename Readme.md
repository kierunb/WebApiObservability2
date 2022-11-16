# ASP.NET Core WebApi with Observability

Demonstration of:
- Logging using *ILogger* and integration with *Seq*
- Metrics using *prometheus-net* with integration with *Prometheus* and *Grafana*
- Distributed Traces using *OpenTelemetry* with integration with *Jeager*
- Health checks and WatchDog pattern

## External Tools setup
- Prometheus - https://prometheus.io/download/
- Grafana - https://grafana.com/grafana/download
- Seq
```
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -v seqvolume:/data -p 5342:80 -p 5341:5341 datalust/seq
```
- Jeager
```
docker run --name jaeger -p 13133:13133 -p 16686:16686 -p 4317:4317 -d --restart=unless-stopped jaegertracing/opentelemetry-all-in-one
```