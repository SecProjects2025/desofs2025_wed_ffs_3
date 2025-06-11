> 
>
> # Desenvolvimento e Testes - Sistema AMAP (Fase 2 - Sprint 2)

| ![Logo do ISEP](../../Imagens/logoisep.png) | ![Logo do DEI](../../Imagens/logo_DEI_big.png) |
|:------------------------------------:|:----------------------------------------:|

**Desenvolvimento de Software Seguros**  
**Mestrado em Engenharia Informática - 2024/2025 **  

**Professores:** 
Paulo Baltarejo de Sousa (T),
Joana Santos (PL),
Cláudio Maia (TP)

**Autores: **
Diana Marques 1240445, 
Diogo Costa 1211514, 
Pedro Costa 1211439, 
Pedro Vilarinho 1211149, 
Tiago Tavares 1240494

**Porto, 2025-06-11**

---

# Índice

[TOC]

---

# Desenvolvimento

## Funcionalidades Implementadas e Complexidade

*   **Agregados do Domínio**
    *   Descrever os novos ou alterados agregados (ou entidades de negócio complexas) e a sua modelação.
*   **Implementação de Autorização**
    *   Detalhar os mecanismos de autorização (ex: papéis, permissões, políticas) e como foram integrados.
*   **Funcionalidades de Back-end**
    *   Apresentar as principais funcionalidades do back-end desenvolvidas (ex: gestão de utilizadores, processamento de dados, integração de APIs).

## Qualidade e Boas Práticas de Código

*   **Encapsulamento de Domínio**
    *   Explicar como o domínio foi isolado e protegido, mencionando a estrutura de camadas e princípios seguidos.
*   **Controlos de Segurança Consistentes**
    *   Detalhar as medidas de segurança implementadas de forma uniforme (ex: validação de inputs, tratamento de sessões, tratamento de erros).
*   **Outras Boas Práticas Adotadas**
    *   Mencionar outras práticas (ex: Clean Code, princípios SOLID, padrões de design, testes unitários).

## Mecanismos de Registo (Logging)

*   **Estratégia e Ferramentas de Logging**
    *   Descrever o framework/biblioteca de logging e a estratégia adotada.
*   **Tipos de Eventos Registados**
    *   Listar os tipos de eventos que são registados e a sua relevância (segurança, erros, etc.).
*   **Gestão de Dados Sensíveis nos Logs**
    *   Explicar as medidas tomadas para garantir que dados sensíveis não são expostos nos logs.

---

# Compilação e Testes

## Automação de Compilação e Testes

*   **Processo de Compilação Automatizado**
    *   Descrever as ferramentas e o processo de build automático (ex: GitHub Actions, Azure DevOps).
*   **Execução Automatizada de Testes**
    *   Explicar como os testes são desencadeados e executados automaticamente (ex: em cada commit, pull request).

## Estratégia de Testes Abrangente

*   **Análise Estática de Código (SAST)**
    *   Detalhar as ferramentas de SAST (ex: SonarQube, Linters) e como foram usadas para melhorar a qualidade e segurança do código.
*   **Análise de Composição de Software (SCA)**
    *   Descrever as ferramentas de SCA (ex: Snyk, OWASP Dependency-Check) e o seu papel na gestão de vulnerabilidades em dependências.
*   **Análise Dinâmica de Aplicações (DAST)**
    *   Explicar as ferramentas de DAST (ex: OWASP ZAP) e como foram utilizadas para detetar vulnerabilidades em tempo de execução.
*   **Outros Tipos de Testes**
    *   **Testes Unitários**
        *   Mencionar a cobertura de código, frameworks utilizados (ex: xUnit) e a sua importância.
    *   **Testes de Integração**
        *   Descrever como foram testadas as interações entre componentes.
    *   **Testes de Aceitação/End-to-End**
        *   Se aplicável, descrever como os cenários de utilizador foram testados.
    *   **Testes de Segurança Específicos**
        *   Mencionar quaisquer outros testes de segurança (ex: testes de penetração, testes de autorização aprofundados).

---

# Produção

## Gestão da Infraestrutura de Produção (Conceitual)

### Ambiente de Alojamento Proposto

O sistema **AMAP** utiliza uma arquitetura containerizada com as seguintes tecnologias:

- **Containerização**: Docker com imagens multi-stage otimizadas baseadas em .NET 9.0  
- **Orquestração**: Docker Compose para gestão de serviços e redes  
- **Registo de Containers**: Docker Hub para armazenamento e distribuição de imagens versionadas  
- **Ambiente de Staging**: Máquina virtual do ISEP com acesso via gateway SSH  
- **Pipeline CI/CD**: GitHub Actions para automação completa do ciclo de desenvolvimento

### Estratégias de Deployment

A aplicação é implantada através de uma pipeline CI/CD automatizada:

- **Build e Validação**: Compilação com .NET 9.0, testes unitários e análise de cobertura  
- **Análise de Segurança**: Integração com SonarCloud, Snyk e OWASP Dependency Check  
- **Containerização**: Construção de imagem Docker otimizada com multi-stage build  
- **Registo**: Push automático para o Docker Hub com versionamento único  
- **Deploy**: Implantação automática no ambiente de staging via SSH  

## Registo e Rastreabilidade em Produção

### Sistema de Logging Centralizado

O sistema implementa logging estruturado através de:

- **Logs de Aplicação**: Sistema nativo de logging do .NET Core com níveis configuráveis  
- **Logs de Container**: Docker com driver `json-file` e rotação automática  
- **Logs de Pipeline**: GitHub Actions com relatórios detalhados de build e deploy  
- **Relatórios de Segurança**: Geração automática de relatórios HTML por Snyk e OWASP  
- **Relatórios de Qualidade**: SonarCloud com métricas de código e segurança  

### Rastreabilidade de Eventos e Auditoria

A rastreabilidade é garantida através de:

- **Versionamento Único**: Cada deploy possui uma tag única baseada no commit SHA  
- **Logs de Deploy**: Registos detalhados de cada etapa da pipeline  
- **Métricas de Qualidade**: Cobertura de código e análise de vulnerabilidades  
- **Auditoria de Segurança**: Monitorização contínua com alertas automáticos  

## Gestão de Incidentes

### Monitorização e Alertas

O sistema de monitorização inclui:

- **Análise Estática Contínua**: SonarCloud com deteção automática de vulnerabilidades  
- **Análise de Dependências**: Snyk e OWASP Dependency Check com alertas  
- **Monitorização de Pipeline**: GitHub Actions com notificações de falhas  
- **Logs de Container**: Monitorização de estado e recursos dos containers  
- **Relatórios Automáticos**: Geração de relatórios HTML de vulnerabilidades  

### Processo de Resposta a Incidentes

O processo implementado segue:

- **Deteção**: Alertas automáticos da pipeline CI/CD e ferramentas de análise  
- **Análise**: Investigação através de logs detalhados e relatórios  
- **Contenção**: Rollback automático para versão anterior em caso de falha  
- **Resolução**: Correção das vulnerabilidades identificadas e novo deploy  
- **Validação**: Testes automáticos após a correção  

## Gestão de Atualizações e Patches

### Estratégia de Atualização de Software

A estratégia implementada utiliza:

- **Deploy Automatizado**: Trigger por push na branch `main`  
- **Validação Contínua**: Testes automáticos e análise de segurança  
- **Versionamento Semântico**: Tags únicas baseadas no commit SHA  
- **Rollback Automático**: Capacidade de reverter para versão anterior  
- **Deploy Gradual**: Primeiro em staging, seguido de validação  

### Manutenção de Dependências

O processo de manutenção inclui:

- **Monitorização Contínua**: Snyk monitoriza dependências e alerta para vulnerabilidades  
- **Análise Automática**: OWASP Dependency Check executado em cada build  
- **Relatórios Detalhados**: HTML com vulnerabilidades identificadas  
- **Atualizações Seguras**: Validação automática antes de atualizar dependências  
- **Cobertura de Testes**: Garantia de que as atualizações não quebram funcionalidades  

## Gestão de Configurações

### Separação e Segurança de Configurações

As configurações são geridas através de:

- **GitHub Secrets**: Armazenamento seguro de credenciais e tokens  
- **Variáveis de Ambiente**: Separação entre desenvolvimento e staging  
- **Configurações de Container**: Docker Compose com variáveis específicas  
- **Segredos Protegidos**: Tokens de autenticação armazenados de forma segura  
- **Configurações de Rede**: Rede Docker isolada (`amap-network`)  

### Configurações de Segurança Implementadas

A segurança é garantida através de:

- **Autenticação SSH**: Chaves privadas para acesso seguro à VM  
- **Tokens de API**: Autenticação segura para Docker Hub, SonarCloud e Snyk  
- **Isolamento de Rede**: Rede Docker bridge para isolamento de tráfego  
- **Portas Configuradas**: Mapeamento da porta externa 8080 para a porta interna 80  
- **Restart Policy**: `unless-stopped` para garantir disponibilidade  

Esta implementação demonstra uma abordagem **DevSecOps** bem estruturada, garantindo que o sistema **AMAP** está preparado para produção com segurança integrada, monitorização contínua e processos automatizados.

---

# Operação

A fase de Operação do sistema **AMAP** compreende um conjunto de práticas e mecanismos destinados a garantir a **segurança**, **disponibilidade** e **continuidade do serviço** após a sua implementação. Seguem-se as principais estratégias adotadas:

## Monitorização do Sistema e dos Utilizadores

### Monitorização da Atividade e Segurança dos Utilizadores

O sistema **AMAP** implementa um mecanismo abrangente de *logging*, que regista todas as operações críticas realizadas pelos utilizadores, nomeadamente:

- Ações administrativas (criação, atualização e remoção de produtos, ofertas, pagamentos, entre outros);
- Acessos a dados sensíveis (informações sobre pagamentos, subscrições, totais financeiros);
- Eventos de autenticação (login, logout, tentativas de acesso falhadas, bloqueios de conta);
- Ocorrência de erros e exceções.

Estes registos permitem rastrear a atividade dos utilizadores, identificar comportamentos suspeitos e manter uma trilha de auditoria completa, essencial para a segurança e conformidade do sistema.

### Monitorização da Saúde do Sistema

A monitorização do desempenho e do estado do sistema é realizada através da análise dos logs de eventos críticos, como erros, falhas de arranque e operações sensíveis. Para ambientes locais, o **Visualizador de Eventos do Windows** pode ser utilizado para deteção rápida de falhas. Em ambientes online, é possível integrar ferramentas como o **UptimeRobot**, que permitem monitorizar a disponibilidade da API de forma automatizada.

---

## Estratégias de Backup e Restauro

A segurança e continuidade dos dados do sistema **AMAP** são asseguradas através de um sistema automatizado de backup e restauro da base de dados SQL Server, conforme descrito nesta secção.

### Sistema de Backup e Restauro

O projeto disponibiliza scripts em PowerShell que automatizam todo o processo de backup e restauro da base de dados `AMAP_DB`.

#### Requisitos

- PowerShell 5.1 ou superior  
- SQL Server Express instalado  
- Módulo SqlServer do PowerShell
  - Instalação: 

```cmd
Install-Module -Name SqlServer
```

#### Scripts Disponíveis

##### 1. Backup da Base de Dados (backup-database.ps1)

Este script realiza o backup completo da base de dados `AMAP_DB` de forma automática e segura.

- **Configurações principais:**

  - Servidor SQL: DESKTOP-37QTCQN\SQLEXPRESS (ajustável conforme o ambiente)

  - Nome da base de dados: AMAP_DB

  - Diretório de backup: C:\backup\AMAP
  - Retenção automática: 7 dias (backups mais antigos são removidos)


- **Execução:** 

``` cmd
.\backup-database.ps1
```

- **Funcionalidades:**

  - Criação de backups completos com nomes únicos baseados em timestamp  

  - Limpeza automática de backups antigos, conforme política de retenção  

  - Geração e manutenção de logs detalhados de todas as operações  

  - Tratamento automático de erros e notificações no log


---

##### 2. Restauro da Base de Dados (restore-database.ps1)

Permite restaurar a base de dados a partir de qualquer ficheiro de backup gerado.

- **Execução:** 

```cmd
powershell.exe -ExecutionPolicy Bypass -File "C:\caminho\para\restore-database.ps1"
```

- **Parâmetros opcionais:**

  - -ServerName: Nome do servidor SQL  

  - -DatabaseName: Nome da base de dados


- **Funcionalidades:**

  - Restauro completo e automático da base de dados  

  - Eliminação de conexões ativas antes do restauro, para evitar conflitos  

  - Retorno ao modo multi-utilizador após o restauro  

  - Registo detalhado de todas as operações e erros no log


#### Agendamento de Backups

A automação dos backups pode ser facilmente configurada através do Agendador de Tarefas do Windows:

1. Abrir o Agendador de Tarefas  
2. Criar uma nova tarefa básica  
3. Definir a periodicidade (ex: diária)  
4. Ação: Iniciar um programa  
5. Programa/script: powershell.exe  
6. Argumentos: -ExecutionPolicy Bypass -File "C:\caminho\para\backup-database.ps1"

#### Logs

Os scripts mantêm registos detalhados em dois ficheiros:

- backup_log.txt: Histórico de todos os backups realizados  
- restore_log.txt: Histórico de todos os restauros efetuados

Estes logs são essenciais para auditoria, troubleshooting e validação do processo.

#### Boas Práticas Recomendadas

- Testar regularmente os backups realizando restauros em ambiente de teste  
- Armazenar cópias dos backups em local externo ao servidor principal  
- Monitorizar os logs para garantir que os processos estão a ser executados corretamente  
- Ajustar o período de retenção consoante as necessidades do projeto  
- Manter a documentação do processo de backup/restauro atualizada

#### Suporte

Em caso de problemas:

- Verificar os logs em C:\backup\AMAP\  
- Confirmar que o SQL Server está em execução  
- Verificar as permissões do utilizador que executa os scripts  
- Consultar a documentação oficial do SQL Server

#### Resumo

Este sistema garante que a base de dados do AMAP pode ser rapidamente recuperada em caso de falha, minimizando o risco de perda de dados e assegurando a continuidade do serviço.

---

## Testes de Penetração (Concecional)

A segurança do sistema é reforçada através da integração de ferramentas de **análise dinâmica de segurança (DAST)**, como o **Snyk**, na *pipeline* CI/CD. Está prevista a integração futura do **OWASP ZAP** para a realização de testes automatizados às APIs. Adicionalmente, recomenda-se a realização de **testes de penetração manuais por especialistas externos** em momentos críticos do ciclo de vida do sistema, com o objetivo de identificar vulnerabilidades não detetadas por ferramentas automatizadas.

---

## Gestão Contínua de Vulnerabilidades

### Processo de Identificação e Remediação

O ciclo de vida da gestão de vulnerabilidades no sistema **AMAP** inclui:

- Identificação automática de vulnerabilidades através de ferramentas como **SonarCloud**, **Snyk** e **OWASP Dependency Check**;
- Triagem e priorização das falhas identificadas;
- Correção e atualização do código-fonte e das dependências;
- Monitorização contínua dos resultados e revisão das configurações de segurança.

### Atualização e Patching de Dependências

As dependências do sistema são regularmente analisadas e atualizadas com o auxílio de comandos como:

```bash
dotnet list package --vulnerable
```

Adicionalmente, são gerados relatórios automáticos através de ferramentas de *Software Composition Analysis* (SCA). O **inventário de pacotes** e o seu estado de segurança são mantidos atualizados, existindo um plano de ação para a atualização ou substituição de componentes vulneráveis ou obsoletos.

---

# Avaliação do ASVS

# Categorias

### Architecture, Design and Threat

Dos 39 requisitos desta categoria, 33 estão cumpridos ou não se aplicam ao sistema AMAP. O projeto implementa pipeline CI/CD com validações de segurança, análise estática de código e validação de configurações seguras, garantindo uma base sólida de segurança desde o design até à entrega.

Os principais requisitos pendentes são: política formal de gestão de chaves (NIST SP 800-57), uso de key vaults e APIs seguras para segredos, processo de substituição de chaves e re-encriptação, logging consistente e envio seguro para sistemas remotos, e validação automática de deployments cloud na pipeline.

No geral, a arquitetura e o design do sistema são robustos.

### Communication

Dos 8 requisitos avaliados nesta categoria, o sistema AMAP cumpre 6, garantindo o uso obrigatório de TLS/HTTPS, certificados confiáveis e autenticação segura nas comunicações.

As principais lacunas identificadas são a ausência de logging explícito para falhas de TLS backend e a falta de configuração de revogação de certificados (OCSP Stapling).

No geral, a comunicação do sistema é segura e robusta.

### Configuration

Dos 23 requisitos avaliados nesta categoria, o sistema AMAP cumpre 18 ou estes não se aplicam ao contexto do projeto. O sistema adota boas práticas de automação de build e deploy, gestão de dependências, validação de headers e proteção contra disclosure acidental.

As principais lacunas identificadas prendem-se com a ausência de alguns headers de segurança HTTP e mecanismos explícitos de verificação de integridade de configurações.

No geral, a configuração do sistema é robusta e segura, recomendando-se apenas o reforço destes pontos para conformidade total.

### API and Web Service Security

Dos 9 requisitos avaliados nesta categoria, o sistema AMAP cumpre todos ou não se aplicam, exceto um. O projeto segue boas práticas na proteção de endpoints REST, validação de métodos e permissões, proteção contra CSRF, validação de content-type e uso obrigatório de HTTPS.

Os requisitos relativos a SOAP e GraphQL não se aplicam.

A principal lacuna identificada é a ausência de validação formal de JSON Schema nos payloads recebidos.

No geral, a solução apresenta uma abordagem segura para APIs e web services.

### Malicious Code Search

Dos 7 requisitos desta categoria, todos estão cumpridos ou não se aplicam ao sistema AMAP. O projeto não contém código malicioso, funcionalidades ocultas, nem recolha de dados não autorizada, e utiliza apenas dependências oficiais e analisadas por ferramentas como SonarCloud e Snyk. Requisitos relativos a sensores ou permissões não se aplicam, pois a aplicação não acede a esses recursos.

### Business Logic Security

Dos 8 requisitos ASVS avaliados para lógica de negócio, o sistema AMAP cumpre 5, incluindo validação da ordem dos passos, limites por utilizador, rate limiting e validações baseadas em análise de risco.

As principais lacunas são a ausência de controlo de tempo entre passos, proteção contra race conditions e monitorização ativa de padrões anómalos de negócio.

No geral, a solução apresenta uma lógica de negócio robusta.

### Files and Resources

Dos 13 requisitos ASVS avaliados para ficheiros e recursos, o sistema AMAP cumpre 9, garantindo validação de formato, tamanho e nomes de ficheiros, bem como proteção contra ataques comuns (LFI, RFI, path traversal, command injection).

As principais lacunas identificadas são a ausência de quotas por utilizador, falta de scanning antivírus, e ausência de restrições explícitas de permissões e extensões a nível do servidor web.

No geral, a solução é segura nesta categoria.

## Rastreabilidade

A rastreabilidade foi garantida através de:

- Relatórios de cobertura;
- Análises estáticas automatizadas;

Para mais detalhe, está disponível uma **checklist ASVS em Excel**, com os requisitos avaliados e o respetivo estado de conformidade.

---

# Conclusão

Durante este sprint, foi implementada com sucesso uma pipeline **CI/CD** robusta para a solução **AMAP**, estabelecendo uma base sólida para um desenvolvimento contínuo e seguro. A automatização de todo o processo — desde o *build* até ao *deploy* — tem permitido entregas mais rápidas e fiáveis, ao mesmo tempo que mantemos elevados padrões de qualidade com o apoio de ferramentas como o **SonarCloud**, **Snyk** e **OWASP Dependency Check**.

Para o próximo sprint, o foco será reforçar a segurança da aplicação e avançar na implementação dos controlos definidos no **ASVS**. Apesar de já terem sido aplicadas várias medidas de segurança, reconhecemos que este é um processo contínuo e em constante evolução.

Plano de Ação:

- Mitigação de vulnerabilidades já identificadas  
- Reforço da proteção de dados através de mecanismos de encriptação  
- Integração de mais testes de segurança automatizados  
- Expansão da cobertura do ASVS

Estas iniciativas têm como objetivo não só reforçar a segurança da solução, como também promover uma cultura de segurança mais sólida no grupo. Com as práticas já implementadas e as melhorias planeadas, estamos confiantes de que conseguiremos alcançar um elevado nível de segurança e qualidade no desenvolvimento da solução.

---

# Referências

1. GitHub. (s.d.). *Documentação do GitHub Actions*. Disponível em: https://docs.github.com/actions
2. SonarCloud. (s.d.). *Documentação do SonarCloud*. Disponível em: https://docs.sonarcloud.io/
3. OWASP Foundation. (s.d.). *OWASP Dependency Check*. Disponível em: https://owasp.org/www-project-dependency-check/
4. Microsoft. (s.d.). *Documentação de Segurança do .NET*. Disponível em: https://learn.microsoft.com/pt-pt/dotnet/standard/security/
5. Check Point Software Technologies. (s.d.). [O que é SAST (Static Application Security Testing)](https://www.checkpoint.com/pt/cyber-hub/cloud-security/what-is-static-application-security-testing-sast/).
6. Check Point Software Technologies. (s.d.). [O que é DAST (Dynamic Application Security Testing)](https://www.checkpoint.com/pt/cyber-hub/cloud-security/what-is-dynamic-application-security-testing-dast/).
7. OWASP. (s.d.). [IAST – Interactive Application Security Testing](https://owasp.org/www-project-devsecops-guideline/latest/02c-Interactive-Application-Security-Testing).
8. Check Point Software Technologies. (s.d.). [O que é SCA (Software Composition Analysis)](https://www.checkpoint.com/pt/cyber-hub/cloud-security/what-is-software-composition-analysis-sca/).
9. Snyk. (s.d.). [Site oficial do Snyk](https://snyk.io/).
10. Check Point Software Technologies. (s.d.). [O que é uma DevSecOps Pipeline](https://www.checkpoint.com/pt/cyber-hub/cloud-security/devsecops/what-is-a-devsecops-pipeline/).
11. GitHub. (s.d.). [Utilização de Segredos no GitHub Actions](https://docs.github.com/en/actions/security-for-github-actions/security-guides/using-secrets-in-github-actions).
12. Microsoft. (s.d.). [Comando dotnet list package](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-package-list).
13. OWASP Foundation. (2021). *OWASP Application Security Verification Standard (ASVS) 4.0*. Disponível em: [https://owasp.org/www-project-application-security-verification-standard/](https://owasp.org/www-project-application-security-verification-standard/)
14. OWASP Foundation. (2021). *OWASP Top 10: The Ten Most Critical Web Application Security Risks*. Disponível em: [https://owasp.org/www-project-top-ten/](https://owasp.org/www-project-top-ten/)
15. Microsoft. (2022). *Security Development Lifecycle (SDL)*. Disponível em: [https://www.microsoft.com/en-us/securityengineering/sdl](https://www.microsoft.com/en-us/securityengineering/sdl)
16. OWASP Foundation. (s.d.). *Threat Modeling Process*. Disponível em: [https://owasp.org/www-community/Threat_Modeling_Process](https://owasp.org/www-community/Threat_Modeling_Process)
17. OWASP Foundation. (s.d.). *Free for Open Source Application Security Tools*. Disponível em: [https://owasp.org/www-community/Free_for_Open_Source_Application_Security_Tools](https://owasp.org/www-community/Free_for_Open_Source_Application_Security_Tools)
18. TechTarget. (s.d.). *Data Flow Diagram (DFD)*. Disponível em: [https://www.techtarget.com/searchdatamanagement/definition/data-flow-diagram-DFD](https://www.techtarget.com/searchdatamanagement/definition/data-flow-diagram-DFD)
19. OWASP Foundation. (s.d.). *Abuse Case Cheat Sheet*. Disponível em: [https://cheatsheetseries.owasp.org/cheatsheets/Abuse_Case_Cheat_Sheet.html](https://cheatsheetseries.owasp.org/cheatsheets/Abuse_Case_Cheat_Sheet.html)
20. Documentos de apoio disponibilizados no Moodle da unidade curricular **Desenvolvimento de Software Seguro** (ISEP, 2024/2025).
