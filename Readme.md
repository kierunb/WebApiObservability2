# ASP.NET Core WebApi with Observability

Demonstration of:
- Logging using *ILogger* and integration with *Seq*
- Metrics using *prometheus-net* with integration with *Prometheus* and *Grafana*
- Distributed Traces using *OpenTelemetry* with integration with *Jeager*
    - Configuration - https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/AspNetCore/Program.cs
- Health checks and WatchDog pattern

## External Tools setup

- Prometheus - https://prometheus.io/download/
```shell

docker run --name prometheus -p 9090:9090 -v /path/to/prometheus.yml:/etc/prometheus/prometheus.yml prom/prometheus

```

- Grafana - https://grafana.com/grafana/download
```shell

docker run -d -p 3000:3000 --name grafana grafana/grafana-oss

```

- Seq - https://docs.datalust.co/docs
```shell

docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -v seqvolume:/data -p 5342:80 -p 5341:5341 datalust/seq

```

- Jeager - https://www.jaegertracing.io/
```shell

docker run --name jaeger -p 13133:13133 -p 16686:16686 -p 4317:4317 -d --restart=unless-stopped jaegertracing/opentelemetry-all-in-one

```