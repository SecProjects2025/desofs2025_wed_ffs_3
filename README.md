> # Modelação de Ameaças - Sistema AMAP (Fase 1)

| ![Logo do ISEP](Imagens/logoisep.png) | ![Logo do DEI](Imagens/logo_DEI_big.png) |
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

**Porto, 2025-04-27**

---

## Índice

[TOC]

---

## Introdução

Este documento apresenta a modelação de ameaças para o sistema AMAP (Associação para a Manutenção da Agricultura de Proximidade), originalmente desenvolvido no âmbito da disciplina de Engenharia de Requisitos (ENGREQ) e agora utilizado como base para a aplicação de conceitos de segurança na disciplina de Desenvolvimento de Software Seguro (DESOFS). O presente trabalho incide exclusivamente sobre a componente de backend do sistema, analisando as suas vulnerabilidades e propondo estratégias de mitigação.

### Contexto do projeto

O projeto tem por objetivo criar uma solução informática que suporte a operação das AMAP/CSAs (Associações para a Manutenção da Agricultura de Proximidade / Comunidades que Sustentam a Agricultura).

A AMAP Porto foi selecionada como piloto para a implementação e teste do protótipo, com vista a modernizar e otimizar a gestão, comercialização e distribuição de bens alimentares produzidos por agricultores locais, ligando‑os a consumidores de forma eficiente e sustentável.

### O sistema AMAP

O sistema AMAP é uma plataforma de gestão que permite a coordenação entre gestores, produtores e co‑produtores, facilitando:

- Gestão dos períodos de subscrição de produtos agrícolas  
- Criação e manutenção de catálogos de produtos  
- Gestão das ofertas de produtos pelos produtores  
- Processamento das subscrições pelos co‑produtores  
- Gestão de pagamentos  
- Organização das datas de entrega  
- Gestão de utilizadores com diferentes níveis de acesso (Gestor, Produtor, Co‑produtor)

A arquitetura do sistema baseia‑se numa API RESTful desenvolvida em ASP.NET Core, fornecendo endpoints seguros para todas as operações necessárias.

### Objetivo e âmbito da modelação de ameaças

Este documento corresponde à Fase 1 do processo de desenvolvimento seguro (SSDLC – Secure Software Development Life Cycle), concentrando‑se nas etapas de análise e de design de segurança. O âmbito da modelação de ameaças inclui:

1. **Análise arquitetural:** compreensão da arquitetura do sistema, dos seus componentes e das suas interações  
2. **Modelação de fluxo de dados:** identificação dos principais fluxos de informação no sistema  
3. **Identificação de ameaças:** aplicação da metodologia STRIDE para deteção sistemática de ameaças  
4. **Avaliação de riscos:** priorização das ameaças identificadas com base em critérios objetivos  
5. **Definição de mitigações:** proposta de controlos e estratégias para mitigar as ameaças mais críticas  
6. **Requisitos de segurança:** estabelecimento de requisitos de segurança específicos para o desenvolvimento  
7. **Planeamento de testes:** definição de uma estratégia para validar a implementação dos controlos de segurança  

Esta modelação de ameaças servirá de base às fases subsequentes do projeto, nas quais os requisitos de segurança identificados serão implementados e validados através de práticas de DevSecOps, testes de segurança e integração contínua.  Análise do Sistema

### Visão Geral do Sistema

O sistema AMAP (Associação para a Manutenção da Agricultura de Proximidade) foi concebido para modernizar e otimizar a gestão, comercialização e distribuição de produtos agrícolas locais, ligando produtores diretamente a consumidores (co‑produtores). Suporta o modelo de negócio das AMAP/CSA (Associações para a Manutenção da Agricultura de Proximidade / Comunidades que Sustentam a Agricultura), promovendo agricultura sustentável e comércio justo.

### Arquitetura do Sistema

![Diagrama de Componentes do AMAP](Imagens/modeloComponentes.png)

A componente de back‑end do AMAP assenta numa arquitetura n‑camadas, que garante separação de responsabilidades, escalabilidade e facilidade de manutenção. A estrutura principal inclui:

#### Camada de Serviços (API)

- **Tecnologia:** ASP.NET Core Web API  
- **Responsabilidade:** expor endpoints RESTful seguros e robustos para todas as operações  
- **Componentes principais:**  
  - **Controladores (Controllers):** implementam a lógica de negócio e coordenam as operações  
  - **Middleware:** gere autenticação, autorização e tratamento global de exceções  
  - **Filtros:** aplicam validações e transformações transversais às requisições  

#### Camada de Dados

- **Tecnologia:** Entity Framework Core com SQL Server  
- **Responsabilidade:** gerir a persistência e recuperação de dados  
- **Componentes principais:**  
  - **Repositórios (Repositories):** encapsulam as operações de acesso a dados por entidade  
  - **Unit of Work:** assegura transações atómicas entre vários repositórios  
  - **DbContext:** faz o mapeamento das entidades às tabelas e gere o ciclo de vida dos objetos  

#### Camada Partilhada

- **Responsabilidade:** definir o modelo de domínio e componentes comuns  
- **Entidades principais:**  
  - **Utilizador (AppUser):** representa utilizadores com diferentes perfis de acesso  
  - **Product, ProductOffer, ProductImage:** armazenam informação sobre produtos agrícolas  
  - **SubscriptionPeriod, DeliveryDate:** gerem períodos de subscrição e datas de entrega  
  - **Composite, Delivery, Payment:** controlam composições de produtos, entregas e pagamentos  
- **Utilities:**  
  - **EmailSender:** serviço de envio de notificações por e‑mail  
  - **Enumerações de domínio:** UserType, PaymentType  
- **ViewModels:** objetos que preparam dados para apresentação nas interfaces  

#### Fluxo de Comunicação

1. O cliente envia uma requisição HTTP para um endpoint da API.  
2. O controlador correspondente processa a requisição e chama os repositórios para operações de dados.  
3. O Unit of Work coordena, se necessário, transações atómicas entre múltiplas alterações.  
4. Os serviços auxiliares (por exemplo, EmailSender) executam tarefas complementares.  
5. A API devolve uma resposta HTTP padronizada com o resultado ou uma mensagem de erro.

> **Nota:** Inserir aqui um diagrama simplificado da arquitetura em camadas.  

### Modelo de Domínio

![Modelo de Domínio do AMAP](Imagens/modeloDominio.png)

O modelo de domínio do AMAP descreve as entidades centrais do negócio e as suas relações, servindo de base conceptual ao sistema:

#### Entidades Principais

- **AppUser**: representa utilizadores do sistema, com os seguintes perfis:
  - **Gestor** (Manager): gere a AMAP e configura os períodos de subscrição  
  - **Produtor** (Producer): regista produtos e define as respetivas ofertas  
  - **Co‑produtor** (CoProducer): efetua subscrições e pagamentos  

- **SubscriptionPeriod**: define um intervalo temporal (por exemplo, semestral) para subscrições, contendo:
  - **Duration**: descrição textual do período  
  - **StartDate / EndDate**: datas de início e fim  
  - **DeliveryDates**: conjunto de datas agendadas para entrega  

- **DeliveryDate**: representa uma data específica de entrega:
  - **Date**: data e hora da entrega  
  - **SubscriptionPeriodId**: identifica o período de subscrição a que pertence  

- **Product**: representa um produto agrícola, com atributos como:
  - **ProducerId / Producer**: produtor responsável  
  - **Name / Description**: nome e descrição  
  - **Photo**: imagem do produto  
  - **ReferencePrice**: preço base de referência  
  - **DeliveryUnits**: unidade de medida para entrega  
  - **IsDeleted**: flag de eliminação lógica  
  - **Composites**: relação com subprodutos (para produtos compostos)  

- **ProductImage**: armazena imagens de produtos:
  - **ImageUrl**: URL da imagem  
  - **ProductId**: produto associado  

- **ProductOffer**: associa produtos a períodos de subscrição, especificando:
  - **SubscriptionPeriodId / SubscriptionPeriod**: período de subscrição  
  - **ProductId / Product**: produto oferecido  
  - **Price**: preço para esse período  
  - **Quantity**: quantidade disponível  
  - **DeliveryDate**: data prevista para entrega  
  - **PaymentType**: forma de pagamento aceite  

- **Subscription**: representa a subscrição de um co‑produtor numa oferta:
  - **CoProducerId / CoProducer**: co‑produtor  
  - **ProductOfferId / ProductOffer**: oferta selecionada  
  - **ExpectedQuantity**: quantidade esperada  
  - **DeliveredQuantity**: quantidade efetivamente entregue  
  - **PaymentType**: forma de pagamento escolhida  

- **Delivery**: regista a entrega efetiva de uma subscrição:
  - **SubscriptionId / Subscription**: subscrição associada  
  - **DeliveredQuantity**: quantidade entregue (pode diferir da esperada)  

- **Payment**: documenta pagamentos efetuados:
  - **AmountPaid**: valor pago  
  - **PaymentDate**: data do pagamento  
  - **SubscriptionId**: subscrição relacionada  

- **Composite**: estabelece a composição de produtos:
  - **ProductId**: produto principal  
  - **SubProductId**: subproduto componente  
  - **Quantity**: quantidade do subproduto  

#### Padrões e Decisões de Design

- **Padrão Composite (Compósito):** implementado para permitir produtos simples e compostos, como cabazes de legumes.  
- **Remoção lógica:** produtos são marcados com a flag `IsDeleted` em vez de serem eliminados fisicamente.  
- **Filtros globais:** configurados através do Entity Framework para excluir automaticamente entidades marcadas como removidas.  
- **Relacionamentos controlados:** definições de cascata ou restrição adaptadas para manter a integridade referencial.  

### Requisitos Funcionais do Back‑end

1. **Gestão de períodos de subscrição**  
   - Criar e eliminar períodos com datas de início, fim e datas de entrega associadas.  
   - Notificar automaticamente os produtores sobre novos períodos.  

2. **Catálogo de produtos**  
   - Registar produtos simples e compostos.  
   - Gerir atributos como preço, descrição e unidades de entrega.  
   - Aplicar remoção lógica de produtos para preservar histórico.  

3. **Gestão de ofertas**  
   - Criar ofertas ligadas a produtos e períodos de subscrição.  
   - Definir preços, quantidades disponíveis e métodos de pagamento.  

4. **Gestão de subscrições**  
   - Processar subscrições de co‑produtores.  
   - Monitorizar quantidades esperadas e associar as respetivas formas de pagamento.  

5. **Gestão de entregas**  
   - Registar entregas efetivas.  
   - Controlar discrepâncias entre quantidades subscritas e efetivamente entregues.  

6. **Processamento de pagamentos**  
   - Registar pagamentos realizados.  
   - Calcular valores a pagar e a receber.  

7. **Notificações por e‑mail**  
   - Enviar comunicações automáticas a produtores e co‑produtores sobre novos períodos de subscrição e outras atualizações.  

8. **Controlo de acesso**  
   - Autenticação e autorização baseadas em perfis de utilizador.  
   - Restringir operações conforme o tipo de utilizador (Gestor, Produtor, Co‑produtor).  

### Requisitos Não Funcionais do Backend

Os requisitos não funcionais concentram‑se em aspetos qualitativos essenciais ao funcionamento e manutenção do backend:

#### Desempenho
- **Tempo de resposta:** consultas complexas processadas em menos de 3 segundos  
- **Indexação:** otimização de índices para acelerar as consultas mais frequentes  
- **Monitorização:** supervisão contínua do desempenho das queries  

#### Segurança
- **Autenticação:** robusta, implementada via ASP.NET Identity  
- **Encriptação:** palavras‑passe protegidas com SHA‑256 (com salt)  
- **Cookies:** configuração segura (HttpOnly, Secure, SameSite)  
- **Validação de inputs:** controlo rigoroso dos dados recebidos no back‑end  
- **Prevenção de ataques:** proteção contra injeções SQL e outras ameaças comuns  

#### Manutenibilidade
- **Modularidade:** código organizado em camadas (API, Data, Shared)  
- **Documentação:** API documentada automaticamente através do Swagger  
- **Gestão de erros:** tratamento consistente de exceções e mensagens de erro  

#### Confiabilidade
- **Integridade de dados:** validações e restrições para garantir consistência  
- **Recuperação:** procedimentos regulares de backup e restauro de dados  

#### Escalabilidade
- **Arquitetura desacoplada:** camadas que permitem distribuição de carga  
- **Preparação para crescimento:** design que suporta aumento do volume de dados  
- **Otimização contínua:** consultas ajustadas para crescer com o número de registos  

Esta análise detalhada do sistema AMAP estabelece a base para a subsequente modelação de ameaças, proporcionando uma visão clara dos componentes, arquitetura e requisitos que precisam ser protegidos.

---

## Fluxo de Dados

### Diagramas de Fluxo de Dados

- **DFD Nível 0**: apresenta uma visão geral dos principais processos do sistema.  
- **DFD Nível 1**: detalha os processos principais (ex: autenticação, subscrição, pagamento).  
- Limites de confiança são indicados de forma clara nos diagramas.

> *Inserir DFDs com os limites de confiança destacados.*

---

## Identificação e Análise de Ameaças

### Aplicação do STRIDE

A técnica STRIDE foi aplicada aos principais componentes do sistema e aos processos descritos nos DFDs. Abaixo, é apresentada uma tabela com as ameaças identificadas, por categoria STRIDE.

> *Inserir tabela STRIDE.*

### Principais Ameaças Identificadas

A seguir, são descritas as 10-12 ameaças mais relevantes:

Para cada ameaça:

- Descrição da ameaça  
- Vetor de ataque  
- Agente de ameaça (interno, externo)

Além disso, são detalhados 2-3 casos de abuso realistas para as ameaças mais críticas.

---

## Avaliação de Risco

### Metodologia de Avaliação

A avaliação de risco foi realizada utilizando a metodologia DREAD (ou outra metodologia simples), atribuindo pontuações aos seguintes critérios: Dano, Reprodutibilidade, Exploração, Atingibilidade e Descoberta.

### Matriz de Risco

> *Inserir matriz de risco com pontuações atribuídas às ameaças.*

As ameaças foram classificadas em alta, média e baixa prioridade, com justificativa para as que representam maior risco.

---

## Mitigações Propostas

### Estratégias de Mitigação por Categoria

- **Autenticação e Controle de Acesso**  
- **Segurança de Dados**  
- **Comunicação Segura**  
- **Registro e Monitorização**  
- **Proteção contra DoS**

### Mitigações para Ameaças de Alta Prioridade

São propostas 5-7 mitigações concretas para as ameaças mais críticas, com foco na viabilidade de implementação e impacto na segurança geral do sistema.

---

## Requisitos de Segurança

### Requisitos por Categoria

Apresenta-se uma lista de 15-20 requisitos de segurança específicos para o sistema AMAP, agrupados por categoria:

- Autenticação  
- Proteção de Dados  
- Comunicação  
- Acesso e Privilégios  
- Monitorização  

Os requisitos foram definidos de forma prática, visando implementação efetiva.

---

## Plano de Testes de Segurança

### Metodologia ASVS

Foi utilizada a abordagem OWASP ASVS como referência para os testes de segurança. Os níveis de verificação aplicáveis ao sistema AMAP foram identificados.

### Matriz de Testes

> *Inserir matriz que relaciona os requisitos de segurança com os testes específicos.*

São descritos 8-10 testes de segurança prioritários com mais detalhes, focando em ameaças de alto risco.

### Avaliação ASVS Preliminar

Identificação das seções do ASVS mais relevantes para o sistema AMAP e plano para completar a avaliação completa na próxima fase.

---

## Processo de Revisão

Será adotado um processo contínuo de revisão da modelação de ameaças com base nos seguintes gatilhos:

- Mudanças na arquitetura;  
- Novas funcionalidades;  
- Incidentes de segurança.

---

## Conclusão

O presente relatório identificou as principais ameaças ao sistema AMAP e propôs um conjunto de mitigações e requisitos de segurança com base em metodologias reconhecidas. A próxima fase incluirá testes detalhados e a implementação prática das medidas propostas.

---

## Referências

- OWASP Top 10  
- OWASP Application Security Verification Standard (ASVS)  
- [Outras fontes utilizadas]

---

## Anexo

Checklist ASVS parcialmente preenchido, destacando as seções mais críticas para o sistema AMAP.