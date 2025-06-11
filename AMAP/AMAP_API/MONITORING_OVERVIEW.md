# System and User Monitoring Overview

## Introduction
To ensure the reliability, performance, and security of the backend, system and user monitoring was implemented using **Prometheus** and **Grafana**. These tools allow real-time tracking of the application's health, resource usage, and user activity.

---

## Monitoring Tools Used

### Prometheus
- **Prometheus** is an open-source monitoring and alerting toolkit.
- It collects metrics from instrumented applications by scraping exposed HTTP endpoints (in this case, `/metrics` from the ASP.NET Core backend).
- Prometheus stores these metrics and makes them available for querying and visualization.

### /metrics Endpoint
- The ASP.NET Core backend exposes a `/metrics` endpoint using the `prometheus-net.AspNetCore` package.
- This endpoint provides real-time metrics about the application's performance, resource usage, and HTTP traffic.
- Example: Accessing `https://localhost:7262/metrics` returns a list of metrics in Prometheus format.

### Grafana
- **Grafana** is a powerful open-source analytics and interactive visualization web application.
- It connects to Prometheus as a data source and allows the creation of dashboards and panels to visualize metrics.
- Dashboards can be customized to show the most relevant information for system health and user activity.

---

## Metrics Tested and Their Meaning

### 1. `process_cpu_seconds_total`
- **Description:**
  - Measures the total CPU time (in seconds) consumed by the backend process.
  - Useful for detecting CPU spikes and monitoring resource usage over time.
- **Example in Grafana:**
  - Query: `process_cpu_seconds_total{job="amap-backend"}`
- **Image:**
  ![CPU Usage](Shared/images/process_cpu_seconds_total.png)

---

### 2. `http_requests_received_total`
- **Description:**
  - Counts the total number of HTTP requests processed by the ASP.NET Core application.
  - Can be filtered by HTTP method, status code, and endpoint.
  - Useful for tracking API usage and detecting unusual traffic patterns.
- **Example in Grafana:**
  - Query: `http_requests_received_total{job="amap-backend"}`
- **Image:**
  ![HTTP Requests Received](Shared/images/http_requests_received_total.png)

---

### 3. `http_request_duration_seconds_sum`
- **Description:**
  - Sums the total duration (in seconds) of all HTTP requests processed.
  - Useful for analyzing the overall latency and performance of the API.
  - Can be combined with `http_request_duration_seconds_count` to calculate average response time.
- **Example in Grafana:**
  - Query: `http_request_duration_seconds_sum{job="amap-backend"}`
- **Image:**
  ![HTTP Request Duration](Shared/images/http_request_duration_seconds_sum.png)

---

## How Monitoring Works
1. The backend exposes metrics at `/metrics`.
2. Prometheus scrapes this endpoint at regular intervals (e.g., every 15 seconds).
3. Grafana connects to Prometheus and visualizes the collected data in dashboards.
4. Metrics such as CPU usage, HTTP request count, and request duration are monitored in real time.

---

## Conclusion
With Prometheus and Grafana integrated, the backend is continuously monitored for performance, resource usage, and user activity. This setup enables early detection of issues, supports capacity planning, and provides valuable insights for maintaining a healthy and secure system. 