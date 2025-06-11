# Automação da Pipeline DevSecOps

## Pipeline de CI/CD com GitHub Actions

Este documento descreve a pipeline de Integração Contínua e Implantação Contínua (CI/CD) configurada utilizando o GitHub Actions para o projeto AMAP. A pipeline automatiza os processos de construção, teste e implantação da aplicação sempre que há novas alterações no código. A pipeline é definida no ficheiro `.github/workflows/main.yml`.

## Visão Geral da Pipeline

A pipeline de CI/CD foi projetada para garantir qualidade, segurança e confiabilidade do código, seguindo as melhores práticas de desenvolvimento. A pipeline é executada automaticamente em cada push para qualquer branch, com etapas adicionais de implantação sendo executadas apenas na branch `main`.

## Fluxo da Pipeline

```mermaid
graph TD
    A[Push de Código] --> B(Checkout)
    B --> C(Setup de Pré-requisitos)
    C --> D(Build e Testes)
    D --> E(Iniciar Aplicação)
    E --> F(Análise ZAP)
    F --> G(Análise SonarCloud)
    G --> H(Análise Snyk)
    H --> I(SCA com OWASP)
    I --> J(Parar Aplicação)
    J --> K{Branch main?}
    K -->|Sim| L(Build Docker)
    L --> M(Push Docker Hub)
    M --> N(Deploy via SSH Gateway)
    K -->|Não| O(Fim)
    N --> O
```

## Tecnologias Utilizadas e Justificações

1. **GitHub Actions**
    - Plataforma nativa do GitHub para CI/CD
    - Integração perfeita com o repositório
    - Suporte a containers e runners auto-hospedados
    - Marketplace rico em ações pré-configuradas

2. **.NET 9.0**
    - Framework moderna
    - Suporte a containers nativo
    - Ferramentas para testes e cobertura de código

3. **Docker**
    - Containerização para garantir consistência entre ambientes
    - Multi-stage builds para otimizar tamanho da imagem
    - Facilidade de deploy em diferentes ambientes

4. **OWASP ZAP**
    - Análise de segurança automatizada de aplicações web
    - Detecção de vulnerabilidades OWASP Top 10
    - Geração de relatórios
    - Integração com GitHub Actions
    - Criação automática de issues para vulnerabilidades encontradas

5. **SonarCloud**
    - Análise contínua da qualidade do código
    - Detecção de bugs e vulnerabilidades
    - Métricas de cobertura de código
    - Integração nativa com GitHub

6. **Snyk**
    - Análise de vulnerabilidades nas dependências
    - Monitorização contínua de segurança
    - Integração com Docker para análise de imagens

7. **Docker Hub**
    - Registo de containers confiável
    - Integração com GitHub Actions
    - Suporte a autenticação segura

8. **SSH Gateway**
    - Acesso seguro ao ambiente de staging
    - Suporte a autenticação por chave
    - Configuração flexível para diferentes ambientes

---

## Fases da Pipeline

A pipeline é dividida nas seguintes fases principais, executadas sequencialmente:

### 1. Checkout do Código

Esta é a primeira fase, onde o código mais recente do repositório é clonado para o ambiente do runner.

*   **Objetivo:** Obter a versão mais recente do código-fonte para as etapas subsequentes da pipeline.
*   **Passos:**
    1.  Utilização da ação `actions/checkout@v3` para clonar o repositório.
*   **Visualização:**

    ![CheckoutStep.png](../../Imagens/pipeline/CheckoutStep.png)

### 2. Setup de Pré-requisitos

Esta fase garante que o ambiente de execução do GitHub Actions runner esteja pronto para construir e testar a aplicação, instalando as ferramentas necessárias.

*   **Objetivo:** Instalar as ferramentas necessárias, como o .NET SDK e o ReportGenerator.
*   **Passos:**
    1.  Instalação do .NET SDK na versão especificada (`${{ env.DOTNET_VERSION }}`) utilizando `actions/setup-dotnet@v3`.
    2.  Instalação global da ferramenta ReportGenerator (`dotnet tool install -g dotnet-reportgenerator-globaltool`) para processar relatórios de cobertura de código.
    3.  Instalação do Entity Framework Core CLI (`dotnet tool install --global dotnet-ef`) para gestão de migrações da base de dados.
    4.  Instalação da ferramenta dotnet-coverage (`dotnet tool install --global dotnet-coverage`) para a recolha de métricas de cobertura de código.
*   **Visualização:**

    ![SetupSteps.png](../../Imagens/pipeline/SetupSteps.png)

### 3. Build, Teste e Análise

Esta fase é responsável por compilar o código, executar os testes unitários com cobertura de código e realizar análises de qualidade e segurança estática com SonarCloud.

*   **Objetivo:** Verificar a integridade e qualidade do código, garantindo que as funcionalidades existentes não foram quebradas pelos testes e realizar a análise estática com o SonarCloud.

#### 3.1 Restauração e 'Build'

Esta subfase foca na preparação do projeto, restaurando dependências e compilando o código.

*   **Objetivo:** Garantir que todas as dependências do projeto estão disponíveis e que o código fonte pode ser compilado com sucesso.
*   **Passos:**
    1.  Restauração das dependências do projeto (`dotnet restore AMAP/AMAP.sln`).
    2.  Construção do projeto (`dotnet build AMAP/AMAP.sln --no-restore`).
*   **Visualização:**

    ![RestoreAndBuildStep.png](../../Imagens/pipeline/RestoreAndBuildStep.png)
    ![BuildSucceeded.png](../../Imagens/pipeline/BuildSucceeded.png)

#### 3.2 Testes Unitários

Nesta subfase, os testes unitários são executados e a cobertura de código é reportada.

*   **Objetivo:** Verificar se as funcionalidades individuais da aplicação funcionam como esperado e medir a percentagem de código coberto pelos testes.
*   **Passos:**
    1.  Execução dos testes unitários com coleta de cobertura de código (`dotnet test AMAP/AMAP.sln --no-build --collect:"XPlat Code Coverage"`).
    2.  Geração do relatório de cobertura no formato OpenCover (`reportgenerator`).
*   **Visualização:**

    ![RunTests.png](../../Imagens/pipeline/RunTests.png)
    ![TestsWithCoverage.png](../../Imagens/pipeline/TestsWithCoverage.png)

### 4. Iniciar Aplicação para Análise ZAP

Esta fase inicia a aplicação para que o ZAP possa realizar a análise de segurança.

*   **Objetivo:** Disponibilizar a aplicação para análise de segurança pelo ZAP.
*   **Passos:**
    1.  Iniciar a aplicação em modo de desenvolvimento.
    2.  Aguardar a aplicação estar pronta para receber requisições.
*   **Visualização:**

    ![StartApp.png](../../Imagens/pipeline/StartApp.png)

### 5. Análise de Segurança com ZAP

Esta fase executa a análise de segurança da aplicação utilizando o OWASP ZAP (Zed Attack Proxy).

*   **Objetivo:** Identificar vulnerabilidades de segurança na aplicação web através de testes automatizados.
*   **Passos:**
    1.  Execução do ZAP baseline scan na aplicação.
    2.  Geração de relatórios de segurança.
    3.  Criação de issues no GitHub para vulnerabilidades encontradas.
*   **Visualização:**

    ![ZAPScan.png](../../Imagens/pipeline/ZapScan.png)
    ![ZAPReport.png](../../Imagens/pipeline/ZapReport.png)

### 6. Análise com SonarCloud

Esta subfase integra o SonarCloud para realizar a análise estática do código.

*   **Objetivo:** Identificar e reportar bugs, code smells, vulnerabilidades e hotspots de segurança no código.
*   **Passos:**
    1.  Configuração e execução da análise do SonarCloud para análise de código, cobertura e segurança.
*   **Visualização:**

    ![SonnarCloudScanner.png](../../Imagens/pipeline/SonnarCloudScanner.png)
    ![SonarCloudOverview.png](../../Imagens/pipeline/SonarCloudOverview.png)
    ![MainBranchReport.png](../../Imagens/pipeline/MainBranchReport.png)

### 7. Análise de Segurança com Snyk

Esta fase executa a análise de segurança do código e das dependências utilizando o Snyk, verificando vulnerabilidades conhecidas.

*   **Objetivo:** Identificar vulnerabilidades conhecidas no código e nas bibliotecas utilizadas.
*   **Passos:**
    1.  Instalação do Snyk CLI (se necessário).
    2.  Execução da análise do Snyk no projeto (por exemplo, `snyk test`).
*   **Visualização:**

    ![SynkMonitor.png](../../Imagens/pipeline/SynkMonitor.png)
    ![SynkTest.png](../../Imagens/pipeline/SynkTest.png)

### 8. Análise de Dependências (SCA) com OWASP Dependency Check

Esta fase utiliza o OWASP Dependency Check para analisar as dependências do projeto na tentativa de encontrar vulnerabilidades conhecidas.

*   **Objetivo:** Identificar componentes de 'software' com vulnerabilidades de segurança conhecidas e gerar relatórios detalhados.
*   **Passos:**
    1.  Execução da ação `dependency-check/Dependency-Check_Action` no projeto.
    2.  Geração de relatórios com as vulnerabilidades encontradas.
*   **Visualização:**

    ![OWASP.png](../../Imagens/pipeline/OWASP.png)

### 9. Parar Aplicação

Esta fase encerra a aplicação após todas as análises de segurança.

*   **Objetivo:** Encerrar a aplicação após a conclusão de todas as análises de segurança.
*   **Passos:**
    1.  Parar a aplicação de forma limpa.
    2.  Verificar se todos os recursos foram libertos.
*   **Visualização:**

    ![StopApp.png](../../Imagens/pipeline/StopApp.png)

### 10. Construção da Imagem Docker

Nesta fase, a aplicação é empacotada numa imagem Docker, pronta para ser executada em qualquer ambiente com Docker.

*   **Objetivo:** Criar um container leve e portátil contendo a aplicação pronta para ser executada em qualquer ambiente com Docker.
*   **Passos:**
    1.  Configuração do ambiente Docker buildx.
    2.  Construção da imagem Docker usando o `Dockerfile`.

### 11. Push da Imagem Docker (Apenas na main branch)

Esta fase envia a imagem Docker construída para um registro de containers (Docker Hub), disponibilizando-a para implantação.

*   **Objetivo:** Disponibilizar a imagem Docker para ser utilizada em ambientes de implantação (como o staging).
*   **Pré-requisitos:** Configuração dos segredos `DOCKERHUB_USERNAME` e `DOCKERHUB_TOKEN` no GitHub.
*   **Passos:**
    1.  Login no Docker Hub.
    2.  Push da imagem Docker para o repositório configurado.
*   **Visualização:**

    ![DockerPush.png](../../Imagens/pipeline/DockerPush.png)

    ![DockerHubBrowser.png](../../Imagens/pipeline/DockerHubBrowser.png)

### 12. Implantação no Ambiente de Staging (Apenas na main branch)

Esta fase implanta a nova versão da aplicação no ambiente de staging, atualizando a aplicação rodando na máquina virtual de destino.

*   **Objetivo:** Atualizar a aplicação no ambiente de staging para testes e validação antes de uma possível implantação em produção.
*   **Pré-requisitos:** Configuração de acesso SSH à VM de staging via gateway, incluindo os segredos `SSH_PRIVATE_KEY`, `SSH_KNOWN_HOSTS`, `SSH_PASSPHRASE`, `VM_HOST`, `VM_PORT`, `VM_USER`, `VM_GATEWAY_HOST`, `VM_GATEWAY_PORT`, `VM_GATEWAY_USER`. O Docker e Docker Compose devem estar instalados na VM.
*   **Passos:**
    1.  Configuração do agente SSH para permitir a conexão segura.
    2.  Cópia do arquivo `docker-compose-staging.yml` para a VM via SCP, utilizando o gateway SSH.
    3.  Execução de comandos remotos na VM via SSH (utilizando o gateway) para:
        *   Navegar até o diretório da aplicação.
        *   Fazer pull da nova imagem Docker.
        *   Parar os containers Docker em execução (`docker compose down`).
        *   Iniciar os novos containers com a nova imagem (`docker compose up -d`).
    4.  Output do URL de acesso ao ambiente de staging (local da VM, acessível via rede ISEP/VPN).
*   **Visualização:**

    ![EnvSetupVm.png](../../Imagens/pipeline/EnvSetupVm.png)

    ![DockerComposeRun.png](../../Imagens/pipeline/DockerComposeRun.png)

## Gestão de Dependências

### Análise e Instalação

1. **Verificação de Dependências**
    - Análise automática de pacotes NuGet
    - Verificação de versões compatíveis
    - Detecção de conflitos de dependências

2. **Instalação Segura**
    - Restauração de pacotes via `dotnet restore`
    - Verificação da integridade dos pacotes
    - Cache das dependências para builds mais rápidas

3. **Monitorização de Segurança**
    - Integração com Snyk para análise de vulnerabilidades
    - Verificação de licenças de pacotes
    - Alertas automáticos para dependências desatualizadas

---

## Análise de Segurança

A pipeline integra múltiplas ferramentas de segurança para garantir uma análise abrangente:

1. **Análise de Segurança de Aplicação Web (ZAP)**
    - Testes automatizados de segurança
    - Detecção de vulnerabilidades OWASP Top 10
    - Geração de relatórios de segurança
    - Criação automática de issues no GitHub

2. **Análise de Código (SAST - Static Application Security Testing)**
    - Detecção de problemas de segurança
    - Verificação de boas práticas
    - Sugestões de correção

3. **Análise de Dependências (Snyk)**
    - Verificação de vulnerabilidades em bibliotecas
    - Monitorização de dependências
    - Recomendações de atualizações

4. **Análise de Containers**
    - Verificação de vulnerabilidades em imagens base
    - Recomendações de atualizações
    - Monitorização de camadas Docker

## Relatórios Gerados

A pipeline gera diversos relatórios para análise:

1. **Relatórios de Testes**
    - Resultados de testes unitários
    - Cobertura de código
    - Tempo de execução dos testes
2. **Relatórios de Qualidade**
    - Métricas do SonarCloud
    - Code smells e bugs
    - Duplicação de código
    - Complexidade ciclomática
3. **Relatórios de Segurança**
    - Vulnerabilidades do Snyk (código e containers)
    - Hotspots de segurança do SonarCloud
    - Dependências desatualizadas
    - **Vulnerabilidades de Dependências (OWASP Dependency Check)**
4. **Relatórios de Build**
    - Logs de compilação
    - Tempo de build
    - Uso de recursos
5. **Relatórios de Deploy**
    - Status da implantação
    - Logs de containers
    - Métricas de performance
6. **Relatórios de Cobertura**
    - Relatórios HTML de cobertura de código
    - Disponibilizados como artefatos do GitHub Actions
    - Acessíveis através da interface do GitHub Actions

### Exemplos de Relatórios

- [dependency-check-report.html](dependency-check-report.html)
- [coverage-report.html](coverage-report.html)

## Configuração de Segredos

Para que a pipeline funcione corretamente, diversos segredos precisam de ser configurados no repositório do GitHub. Estes incluem credenciais para o Docker Hub e acesso SSH à máquina virtual de staging.

*   `DOCKERHUB_USERNAME`: O nome de utilizador no Docker Hub.
*   `DOCKERHUB_TOKEN`: Um Personal Access Token do Docker Hub com permissões de push.
*   `SSH_PRIVATE_KEY`: A chave privada SSH para autenticação na VM de staging.
*   `SSH_KNOWN_HOSTS`: A host key da VM de staging para verificar a autenticidade do servidor.
*   `SSH_PASSPHRASE`: A senha da chave privada SSH.
*   `SONAR_TOKEN`: Token de autenticação para o SonarCloud
*   `SNYK_TOKEN`: Token de autenticação para o Snyk
*   `VM_HOST`: O hostname ou endereço IP da VM de staging (resolvível dentro da rede ISEP/VPN).
*   `VM_PORT`: A porta SSH da VM de staging.
*   `VM_USER`: O utilizador SSH para login na VM de staging.
*   `VM_GATEWAY_HOST`: O hostname ou endereço IP do servidor gateway SSH.
*   `VM_GATEWAY_PORT`: A porta SSH do servidor gateway.
*   `VM_GATEWAY_USER`: O utilizador SSH para login no servidor gateway.

---

## Duração dos Steps

Aqui está um resumo da duração média de cada step da pipeline:

1. **Setup de Pré-requisitos**
   - Duração média: 32 sec

2. **Build e Testes**
   - Duração média: 1m 13sec

3. **Iniciar Aplicação**
   - Duração média: 1m 22sec

4. **Análise de Segurança com ZAP**
   - Duração média: 1m 8sec

5. **Análise SonarCloud**
   - Duração média: 2m 1sec

6. **Análise de Segurança (Snyk)**
   - Duração média: 24sec

7. **SCA com OWASP**
   - Duração média: 58sec

8. **Parar Aplicação**
   - Duração média: 1sec

9. **Build Docker**
   - Duração média: 49sec

10. **Push Docker Hub**
    - Duração média: 9sec

11. **Deploy via SSH Gateway**
    - Duração média: 19sec

**Duração Total da Pipeline**: 9min 38sec