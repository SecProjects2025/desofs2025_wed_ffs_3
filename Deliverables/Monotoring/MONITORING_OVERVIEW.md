# Visão Geral da Monitorização do Sistema e Utilizadores

## Introdução
Para garantir a fiabilidade, desempenho e segurança do backend, foi implementada monitorização do sistema e dos utilizadores utilizando **Prometheus** e **Grafana**. Estas ferramentas permitem o acompanhamento em tempo real da saúde da aplicação, utilização de recursos e atividade dos utilizadores.

---

## Ferramentas de Monitorização Utilizadas

### Prometheus
- **Prometheus** é uma ferramenta open-source de monitorização e alertas.
- Recolhe métricas de aplicações instrumentadas ao aceder a endpoints HTTP expostos (neste caso, `/metrics` do backend ASP.NET Core).
- O Prometheus armazena estas métricas e disponibiliza-as para consulta e visualização.

### Endpoint /metrics
- O backend ASP.NET Core expõe um endpoint `/metrics` usando o pacote `prometheus-net.AspNetCore`.
- Este endpoint fornece métricas em tempo real sobre o desempenho da aplicação, utilização de recursos e tráfego HTTP.
- Exemplo: Aceder a `https://localhost:7262/metrics` devolve uma lista de métricas no formato Prometheus.

### Grafana
- **Grafana** é uma aplicação open-source de visualização e análise interativa de dados.
- Liga-se ao Prometheus como fonte de dados e permite criar dashboards e painéis para visualizar métricas.
- Os dashboards podem ser personalizados para mostrar a informação mais relevante sobre a saúde do sistema e atividade dos utilizadores.

---

## Métricas Testadas e o seu Significado

### 1. `process_cpu_seconds_total`
- **Descrição:**
  - Mede o tempo total de CPU (em segundos) consumido pelo processo do backend.
  - Útil para detetar picos de CPU e monitorizar a utilização de recursos ao longo do tempo.
- **Exemplo no Grafana:**
  - Query: `process_cpu_seconds_total{job="amap-backend"}`
- **Imagem:**
  ![Utilização de CPU](Shared/images/process_cpu_seconds_total.png)

---

### 2. `http_requests_received_total`
- **Descrição:**
  - Conta o número total de requisições HTTP processadas pela aplicação ASP.NET Core.
  - Pode ser filtrado por método HTTP, código de estado e endpoint.
  - Útil para acompanhar o uso da API e detetar padrões de tráfego invulgares.
- **Exemplo no Grafana:**
  - Query: `http_requests_received_total{job="amap-backend"}`
- **Imagem:**
  ![Requisições HTTP Recebidas](Shared/images/http_requests_received_total.png)

---

### 3. `http_request_duration_seconds_sum`
- **Descrição:**
  - Soma a duração total (em segundos) de todas as requisições HTTP processadas.
  - Útil para analisar a latência e desempenho global da API.
  - Pode ser combinado com `http_request_duration_seconds_count` para calcular o tempo médio de resposta.
- **Exemplo no Grafana:**
  - Query: `http_request_duration_seconds_sum{job="amap-backend"}`
- **Imagem:**
  ![Duração das Requisições HTTP](Shared/images/http_request_duration_seconds_sum.png)

---

## Como Funciona a Monitorização
1. O backend expõe métricas em `/metrics`.
2. O Prometheus recolhe este endpoint em intervalos regulares (ex: a cada 15 segundos).
3. O Grafana liga-se ao Prometheus e visualiza os dados recolhidos em dashboards.
4. Métricas como utilização de CPU, número de requisições HTTP e duração das requisições são monitorizadas em tempo real.

---

## Conclusão
Com Prometheus e Grafana integrados, o backend é continuamente monitorizado quanto a desempenho, utilização de recursos e atividade dos utilizadores. Esta configuração permite a deteção precoce de problemas, apoia o planeamento de capacidade e fornece informações valiosas para manter um sistema saudável e seguro. 