# Guia de Monitorização do Sistema e Utilizadores

## 1. Objetivo
Monitorizar a saúde, desempenho e utilização do sistema backend e dos utilizadores, permitindo a deteção precoce de problemas e visibilidade sobre o comportamento do sistema.

---

## 2. Ferramentas Utilizadas
- **Prometheus**: Recolhe e armazena métricas do backend.
- **Grafana**: Visualiza métricas e cria dashboards/alertas.
- **prometheus-net.AspNetCore**: Biblioteca .NET para expor métricas de aplicações ASP.NET Core.

---

## 3. Integração Passo a Passo

### A. Adicionar Métricas Prometheus ao ASP.NET Core

1. **Instalar o pacote NuGet**
   ```sh
   dotnet add package prometheus-net.AspNetCore
   ```

2. **Configurar no `Program.cs`**
   ```csharp
   // Adiciona no topo:
   using Prometheus;

   // Na secção builder.Services (se necessário):
   // builder.Services.AddHealthChecks();

   // No pipeline da app (após app.UseRouting()):
   app.UseEndpoints(endpoints =>
   {
       endpoints.MapControllers();
       endpoints.MapMetrics(); // Expõe o endpoint /metrics
   });
   ```

3. **Executar a API**
   - Um novo endpoint `/metrics` estará disponível com métricas Prometheus.

---

### B. Instalar e Configurar o Prometheus

1. **Descarregar o Prometheus**
   - [https://prometheus.io/download/](https://prometheus.io/download/)

2. **Editar o `prometheus.yml`**
   ```yaml
   global:
     scrape_interval: 15s

   scrape_configs:
     - job_name: 'amap-backend'
       static_configs:
         - targets: ['localhost:5000'] # Porta da tua API
   ```

3. **Iniciar o Prometheus**
   ```sh
   ./prometheus --config.file=prometheus.yml
   ```
   - Acede a [http://localhost:9090](http://localhost:9090) para ver o Prometheus.

---

### C. Instalar e Configurar o Grafana

1. **Descarregar o Grafana**
   - [https://grafana.com/grafana/download](https://grafana.com/grafana/download)

2. **Iniciar o Grafana**
   - Segue as instruções do site. Por defeito, corre em [http://localhost:3000](http://localhost:3000)

3. **Adicionar o Prometheus como fonte de dados**
   - No Grafana, vai a "Configuração" > "Fontes de Dados" > "Adicionar fonte de dados" > Escolhe Prometheus e coloca `http://localhost:9090`.

4. **Criar dashboards**
   - Usa dashboards prontos ou cria gráficos para:
     - Número de requisições por endpoint
     - Erros (4xx, 5xx)
     - Tempo de resposta
     - Utilização de recursos

---

### D. (Opcional) Alertas
- No Grafana, podes criar alertas para notificar a equipa em caso de falhas, picos de erro, etc.

---

## 4. Boas Práticas
- Monitoriza endpoints críticos e recursos do sistema.
- Revê dashboards e alertas regularmente.
- Mantém as ferramentas atualizadas.

---

## 5. Referências
- [Documentação prometheus-net](https://github.com/prometheus-net/prometheus-net)
- [Introdução ao Prometheus](https://prometheus.io/docs/introduction/overview/)
- [Introdução ao Grafana](https://grafana.com/docs/grafana/latest/getting-started/)

---

## Exemplos de Queries para Grafana

- **Utilização de CPU:**
  ```
  process_cpu_seconds_total{job="amap-backend"}
  ```
- **Total de Requisições HTTP:**
  ```
  http_requests_received_total{job="amap-backend"}
  ```
- **Duração das Requisições (percentil 95%):**
  ```
  histogram_quantile(0.95, sum(rate(http_request_duration_seconds_bucket{job="amap-backend"}[5m])) by (le))
  ```

---

## Nota de Segurança

> Em produção, restringe o acesso ao endpoint `/metrics` apenas a sistemas internos ou protege-o com autenticação/firewall para evitar exposição de dados operacionais sensíveis.

---

## Como Criar Alertas no Grafana (Opcional)

1. No teu dashboard, clica num painel e seleciona "Editar".
2. Vai ao separador "Alerta" e clica em "Criar regra de alerta".
3. Define a condição (ex: se a utilização de CPU > 80% durante 5 minutos).
4. Configura canais de notificação (email, Slack, etc.).
5. Guarda o alerta.

--- 