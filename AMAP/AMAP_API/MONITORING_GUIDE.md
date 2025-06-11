# System and User Monitoring Guide

## 1. Objective
Monitor the health, performance, and usage of the backend system and users, enabling early detection of issues and visibility into system behavior.

---

## 2. Tools Used
- **Prometheus**: Collects and stores metrics from the backend.
- **Grafana**: Visualizes metrics and creates dashboards/alerts.
- **prometheus-net.AspNetCore**: .NET library to expose metrics from ASP.NET Core applications.

---

## 3. Step-by-Step Integration

### A. Add Prometheus Metrics to ASP.NET Core

1. **Install the NuGet package**
   ```sh
   dotnet add package prometheus-net.AspNetCore
   ```

2. **Configure in `Program.cs`**
   ```csharp
   // Add at the top:
   using Prometheus;

   // In the builder.Services section (if needed):
   // builder.Services.AddHealthChecks();

   // In the app pipeline (after app.UseRouting()):
   app.UseEndpoints(endpoints =>
   {
       endpoints.MapControllers();
       endpoints.MapMetrics(); // Exposes /metrics endpoint
   });
   ```

3. **Run your API**
   - A new endpoint `/metrics` estará disponível com métricas Prometheus.

---

### B. Install and Configure Prometheus

1. **Download Prometheus**
   - [https://prometheus.io/download/](https://prometheus.io/download/)

2. **Edit `prometheus.yml`**
   ```yaml
   global:
     scrape_interval: 15s

   scrape_configs:
     - job_name: 'amap-backend'
       static_configs:
         - targets: ['localhost:5000'] # Porta da tua API
   ```

3. **Start Prometheus**
   ```sh
   ./prometheus --config.file=prometheus.yml
   ```
   - Acede a [http://localhost:9090](http://localhost:9090) para ver o Prometheus.

---

### C. Install and Configure Grafana

1. **Download Grafana**
   - [https://grafana.com/grafana/download](https://grafana.com/grafana/download)

2. **Start Grafana**
   - Siga as instruções do site. Por defeito, corre em [http://localhost:3000](http://localhost:3000)

3. **Adicionar Prometheus como fonte de dados**
   - Em Grafana, vai a "Configuration" > "Data Sources" > "Add data source" > Escolhe Prometheus e coloca `http://localhost:9090`.

4. **Criar dashboards**
   - Usa dashboards prontos ou cria gráficos para:
     - Número de requisições por endpoint
     - Erros (4xx, 5xx)
     - Tempo de resposta
     - Utilização de recursos

---

### D. (Opcional) Alertas
- Em Grafana, podes criar alertas para notificar a equipa em caso de falhas, picos de erro, etc.

---

## 4. Best Practices
- Monitoriza endpoints críticos e recursos do sistema.
- Revê dashboards e alertas regularmente.
- Mantém as ferramentas atualizadas.

---

## 5. References
- [prometheus-net documentation](https://github.com/prometheus-net/prometheus-net)
- [Prometheus Getting Started](https://prometheus.io/docs/introduction/overview/)
- [Grafana Getting Started](https://grafana.com/docs/grafana/latest/getting-started/)

---

## Example Queries for Grafana

- **CPU Usage:**
  ```
  process_cpu_seconds_total{job="amap-backend"}
  ```
- **Total HTTP Requests:**
  ```
  http_requests_received_total{job="amap-backend"}
  ```
- **Request Duration (95th percentile):**
  ```
  histogram_quantile(0.95, sum(rate(http_request_duration_seconds_bucket{job="amap-backend"}[5m])) by (le))
  ```

---

## Security Note

> In production, restrict access to the `/metrics` endpoint to internal systems or protect it with authentication/firewall rules to prevent exposure of sensitive operational data.

---

## Creating Alerts in Grafana (Optional)

1. In your dashboard, click on a panel and select "Edit".
2. Go to the "Alert" tab and click "Create alert rule".
3. Define the condition (e.g., if CPU usage > 80% for 5 minutes).
4. Set up notification channels (email, Slack, etc.).
5. Save the alert.

--- 