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

## Visão Geral do Sistema

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

### Requisitos Funcionais do Backend

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

### Introdução

Os Diagramas de Fluxo de Dados (DFDs) são ferramentas essenciais para visualizar como a informação flui através do sistema AMAP. Estes diagramas permitem identificar os componentes principais do sistema, como os dados são processados, armazenados e trocados entre diferentes partes da aplicação e entidades externas.

### Componentes do Sistema

#### Entidades Externas

- **Gestor**: Responsável pela administração do sistema, criação de períodos de subscrição e monitorização de pagamentos.

- **Produtor**: Fornecedor dos produtos agrícolas, responsável por cadastrar produtos, definir quantidades e criar ofertas.

- **CoProdutor**: Utilizador que subscreve as ofertas disponíveis e realiza pagamentos.

- **Sistema de Email**: Componente externo utilizado para enviar notificações aos produtores sobre as datas de início e fim do período e das datas de entregas presentes nesse mesmo período.

#### Processos Principais

- **<u>Processo Central</u>**: **"Sistema de Gestão e Subscrição de Ofertas de Produtos da AMAP"** - Representa todo o sistema como um único processo.

- **Autenticação**: Processo responsável por autenticar os diferentes tipos de utilizadores (Gestor, Produtor, CoProdutor).

- **Gestão de Períodos de Subscrição**: Permite ao Gestor criar e gerir períodos de subscrição com respetivas datas de entrega.

- **Gestão de Produtos e Quantidades**: Permite aos Produtores criar produtos no sistema.

- **Criação de Ofertas**: Processo onde os Produtores podem criar ofertas de produtos para determinados períodos de subscrição.

- **Subscrição**: Processo que permite aos CoProdutores subscreverem ofertas disponíveis.

- **Pagamento**: Gestão dos pagamentos realizados pelos CoProdutores.

- **Controlo de Pagamentos**: Processo para monitorização e gestão de pagamentos pelo Gestor.

- **Notificar Produtor**: Envio de notificações aos Produtores sobre novos períodos e respetivas datas de entrega.

#### Armazenamentos de Dados

- **Utilizadores**: Armazenamento dos dados de utilizadores do sistema (Gestor, Produtor, CoProdutor).

- **Produtos**: Repositório dos produtos cadastrados pelos Produtores.

- **Ofertas de Produtos**: Armazenamento das ofertas criadas pelos Produtores.

- **Subscrições**: Registo das subscrições feitas pelos CoProdutores.

- **Períodos de Subscrição**: Armazenamento dos períodos e datas de entrega.

- **Pagamentos**: Registo dos pagamentos realizados pelos CoProdutores.

### Diagramas de Fluxo de Dados

#### Nível 0

O DFD de Nível 0 (ou Diagrama de Contexto) apresenta uma visão global do sistema AMAP, mostrando como ele interage com as entidades externas.

A seguinte figura apresenta o Diagrama de Fluxo de Dados de Nível 0 (Diagrama de Contexto) do sistema AMAP, ilustrando as interações entre o processo central e as entidades externas, bem como os limites de confiança estabelecidos.

![dfd0](Imagens\dfd0.png)

##### Descrição dos Fluxos Principais

- Gestor ←→ Sistema:
  - Autenticação/Gestão Períodos: O Gestor autentica-se e gere os períodos de subscrição.
  - Controlo de Pagamentos: O Gestor monitoriza e controla os pagamentos no sistema.

- Produtor ←→ Sistema:
  - Visualização do Catálogo e Quantidades: O Produtor pode visualizar seu catálogo de produtos, bem como as quantidades subscritas pelos CoProdutores.
  - Autenticação/Gestão de Produtos e Quantidades/Criação de Ofertas: O Produtor autentica-se, gere seus produtos, cria ofertas e pode alterar a quantidade de produtos entregue fase ao que foi subscrito pelo CoProdutor.

- CoProdutor ←→ Sistema:
  - Autenticação/Subscrição/Pagamentos: O CoProdutor autentica-se, realiza subscrições e efetua pagamentos.
  - Informação das Ofertas Disponíveis: O sistema fornece informações sobre as ofertas disponíveis.

- Sistema → Sistema de Email:
  - Envio de Notificações sobre períodos e datas de entrega: O sistema envia notificações aos produtores através do sistema de email.
  - Comunicação Externa: Representa a comunicação do sistema com o serviço externo de email.

##### Limites de Confiança
Os limites de confiança no diagrama (linhas tracejadas) representam pontos críticos onde:

1. Interface de Utilizador: Separa a camada de apresentação (frontend) do núcleo do sistema, indicando que todas as interações dos utilizadores passam por esta fronteira de segurança.
2. Comunicação Externa: Delimita a fronteira entre o sistema AMAP e o serviço externo de email, indicando um ponto onde dados saem do controlo direto da aplicação.

Estes limites são essenciais para identificar pontos onde:

- Os dados podem ser interceptados ou manipulados

- É necessário implementar validações e controlos de acesso

- Medidas de proteção específicas devem ser aplicadas para garantir a integridade e confidencialidade das informações

#### Nível 1

O DFD de Nível 1 expande o processo central do DFD Nível 0, detalhando os subprocessos internos e como eles interagem entre si, com as entidades externas e com os armazenamentos de dados.

A apresenta a baixo representa o Diagrama de Fluxo de Dados de Nível 1 do sistema AMAP, detalhando os processos internos, armazenamentos de dados e os fluxos de informação entre eles, além de identificar os diversos limites de confiança presentes no sistema.

![dfd0](Imagens\dfd1.png)

##### Processos Detalhados

1. Autenticação:

   - Recebe credenciais de login dos utilizadores (Gestor, Produtor, CoProdutor).

   - Valida as credenciais contra o armazenamento de Utilizadores.

   - Retorna dados do utilizador autenticado.

   - Estabelece o controlo de acesso por perfil.

2. Gestão de Períodos de Subscrição:

   - Recebe dados de novos períodos do Gestor autenticado.

   - Armazena os períodos de subscrição no respetivo armazenamento.

   - Fornece informações sobre períodos existentes.

3. Gestão de Produtos e Quantidades:

   - Recebe dados de produtos do Produtor autenticado.

   - Regista produtos no armazenamento correspondente.

   - Fornece informações sobre produtos existentes.

4. Criação de Ofertas:

   - Recebe lista de produtos com quantidades definidas.

   - Regista ofertas no armazenamento de Ofertas de Produtos.

   - Disponibiliza informações sobre ofertas existentes.

5. Subscrição:

   - Recebe pedidos de subscrição do CoProdutor autenticado.

   - Regista subscrições no armazenamento correspondente.

   - Envia dados da subscrição para o processo de Pagamento.

   - Fornece informações sobre subscrições existentes.

6. Pagamento:

   - Recebe dados de subscrição para pagamento.

   - Regista informações de pagamento no armazenamento.

   - Fornece informações sobre pagamentos existentes.

7. Controlo de Pagamentos:

   - Acessa informações de subscrições e pagamentos existentes.

   - Fornece ao Gestor ferramentas para monitorizar e gerir pagamentos.

8. Notificar Produtor:

   - Recebe notificação e-mail do processo de Gestão de Períodos.

   - Integra-se com o Sistema de Email para enviar notificações aos Produtores.

##### Fluxos de Dados Importantes

- Credenciais de Login → Autenticação: Dados de login fornecidos pelos utilizadores.

- Utilizador Autenticado → Diversos Processos: Informações do utilizador após autenticação.

- Dados do Novo Período → Gestão de Períodos: Informações sobre novos períodos de subscrição.

- Registo de Produtos → Armazenamento de Produtos: Dados de produtos para armazenamento.

- Lista de Produtos com Quantidades → Criação de Ofertas: Produtos disponíveis para criação de ofertas.

- Registo de Ofertas → Armazenamento de Ofertas: Dados de ofertas para armazenamento.

- Registo de Subscrições → Armazenamento de Subscrições: Dados de subscrições para armazenamento.

- Dados de Subscrição para Pagamento → Pagamento: Informações para processamento de pagamento.

- Registo de Pagamento → Armazenamento de Pagamentos: Dados de pagamentos para armazenamento.

- Mensagem de Notificação → Sistema de Email: Conteúdo a ser enviado por email.

##### Limites de Confiança 

Os limites de confiança no DFD Nível 1 são mais detalhados, identificando:

1. **Perímetro de Autenticação:** Separa os utilizadores não autenticados dos autenticados.
2. **Controlo de Acesso por Perfil:** Limita o acesso a funcionalidades específicas baseado no tipo de utilizador.
3. **Database Boundary:** Limites entre processos e armazenamentos de dados, indicando pontos onde os dados precisam ser protegidos durante o armazenamento.
4. **Integração com Sistema de Email:** Limite entre o sistema AMAP e o serviço externo de email.

### Conclusão

A análise de fluxo de dados do sistema AMAP revela uma arquitetura modular com separação clara de responsabilidades entre diferentes processos. Os diagramas demonstram como os dados fluem entre utilizadores, processos e armazenamentos, destacando pontos críticos onde medidas de segurança são necessárias (limites de confiança).

Esta documentação de fluxo de dados é essencial para compreender o funcionamento do sistema, identificar potenciais vulnerabilidades de segurança e garantir que todos os componentes interajam corretamente para fornecer as funcionalidades esperadas.

A estrutura do sistema, composta por processos de autenticação, gestão de períodos, produtos, ofertas, subscrições e pagamentos, reflete a arquitetura da aplicação revelada na análise do código-fonte, particularmente nos controladores e modelos encontrados no projeto AMAP.

---

## Identificação e Análise de Ameaças

### Introdução 

Nesta secção, aplicamos a metodologia STRIDE para identificar e analisar as ameaças mais relevantes para o sistema AMAP. Para cada elemento do DFD, identificamos as ameaças aplicáveis, detalhamos os vetores de ataque, identificamos potenciais agentes de ameaça e ilustramos com casos de abuso concretos.

### Aplicação da Metodologia STRIDE

A metodologia STRIDE é utilizada para identificar ameaças de segurança, onde cada letra representa uma categoria:

- **Spoofing**: Falsificação de identidade

- **Tampering**: Adulteração de dados

- **Repudiation**: Repúdio de ações realizadas

- **Information Disclosure**: Divulgação não autorizada de informação

- **Denial of Service**: Negação de serviço

- **Elevation of Privilege**: Elevação de privilégios

### Análise de Ameaças por Elemento do DFD

#### 1. Elementos do Processo

##### 1.1 Processo de Autenticação

**Ameaças STRIDE Aplicáveis**: S, E

**Ameaças Identificadas**:

- S (Spoofing): Falsificação de identidade para obter acesso não autorizado como Gestor, Produtor ou CoProdutor.

- E (Elevation of Privilege): Escalação de privilégios de uma conta de CoProdutor para Produtor ou Gestor.

**Vetores de Ataque**:

- Ataques de força bruta contra o endpoint de login da API (`/api/Account/login`)

- Manipulação de Claims nos tokens JWT para alterar o UserType

- Roubo de credenciais através de phishing

**Agentes de Ameaça**:

- Atacantes externos que procuram acesso à plataforma

- CoProdutores que tentam obter privilégios de Produtor/Gestor

- Ex-utilizadores cujo acesso não foi revogado

**Casos de Abuso**:

1. Um atacante obtém credenciais de um Gestor através de phishing e acede a funcionalidades administrativas.
2. Um CoProdutor manipula o seu token de autenticação para alterar o seu UserType para Gestor, tentando assim aceder às funcionalidades de gestão de períodos.

##### 1.2 Processo de Gestão de Períodos de Subscrição

**Ameaças STRIDE Aplicáveis:** T, E

**Ameaças Identificadas:**

- T (Tampering): Modificação não autorizada de períodos de subscrição e datas de entrega, comprometendo a integridade do sistema de planeamento.

- E (Elevation of Privilege): Acesso indevido às funcionalidades de gestão de períodos por utilizadores sem privilégios de Gestor.

**Vetores de Ataque:**

- Manipulação de parâmetros na chamada à API para o endpoint `/api/SubscriptionPeriod`

- Exploração de vulnerabilidades no controlo de acesso baseado em roles (RBAC)

- Injeção SQL para modificar diretamente dados de períodos na base de dados

- Obstrução e alteração de pedidos HTTP durante a criação ou modificação de períodos

**Agentes de Ameaça:**

- Gestores mal-intencionados com acesso legítimo ao sistema

- Produtores que tentam obter vantagens comerciais através de períodos favoráveis

- Atacantes externos que conseguiram comprometer contas de Gestor

- Ex-funcionários com conhecimento do sistema e potenciais motivos para sabotagem

**Casos de Abuso:**

1. Um Gestor corrupto cria períodos de subscrição com datas estrategicamente favoráveis para certos Produtores, prejudicando a concorrência justa no sistema.
2. Um Produtor explora uma vulnerabilidade no controlo de acesso da API e consegue aceder ao endpoint `/api/SubscriptionPeriod/SubscriptionPeriod` que deveria ser restrito a Gestores, criando períodos favoráveis para os seus próprios produtos.
3. Um atacante com acesso à base de dados modifica diretamente os registos de datas de entrega, criando inconsistências que comprometem o funcionamento do sistema e causam confusão entre Produtores e CoProdutores.
4. Um atacante realiza um ataque Man-in-the-Middle, intercetando e modificando pedidos HTTP legítimos de Gestores durante a criação de períodos, alterando datas e durações sem o conhecimento do utilizador.

##### 1.3 Processo de Gestão de Produtos e Quantidades

**Ameaças STRIDE Aplicáveis:** T, I

**Ameaças Identificadas:**

- T (Tampering): Alteração não autorizada de informações de produtos, como preços, descrições, fotos ou quantidades disponíveis após a criação de ofertas ou subscrições.

- I (Information Disclosure): Divulgação não autorizada de informações de produtos antes do lançamento oficial, comprometendo a concorrência justa.

**Vetores de Ataque:**

- Manipulação de pedidos à API `/api/Product` para alterar dados de produtos existentes

- Exploração de falhas na validação de entrada para injetar dados maliciosos (ex: XSS em descrições de produtos)

- Upload de imagens maliciosas contendo código executável ou conteúdo inadequado

- Acesso direto à base de dados para modificar informações de produtos sem passar pelas validações da API

- Manipulação de ficheiros CSV durante o processo de importação/exportação de produtos

- Injeção de código malicioso em ficheiros de produtos antes do upload

- Exploração de vulnerabilidades no processamento de ficheiros durante a importação automática para a base de dados

**Agentes de Ameaça:**

- Produtores desonestos que manipulam os seus próprios produtos após subscrições

- Atacantes que comprometeram contas de Produtores legítimos

- Concorrentes que tentam prejudicar a reputação de outros Produtores

- Insiders com acesso privilegiado à base de dados ou sistemas backend

- CoProdutores técnicos que tentam alterar preços ou quantidades de produtos subscritos
- Utilizadores maliciosos com conhecimentos técnicos capazes de manipular ficheiros CSV

**Casos de Abuso:**

1. Um Produtor cria um produto com características atrativas, e após receber múltiplas subscrições, altera a descrição, quantidade ou outras características através do endpoint `/api/Product`, prejudicando os CoProdutores que já subscreveram com base nas informações originais.
2. Um atacante consegue comprometer a conta de um Produtor respeitado e modifica as fotos dos produtos para imagens contendo conteúdo malicioso ou inapropriado, prejudicando a reputação do Produtor e potencialmente comprometendo dispositivos de utilizadores.
3. Um Produtor desonesto insere intencionalmente preços muito baixos no campo ReferencePrice dos seus produtos para atrair subscrições, e depois alega ter sido um erro técnico, tentando renegociar os valores ou entregando produtos de qualidade inferior.
4. Um Produtor malicioso manipula um ficheiro CSV de produtos antes de fazer o upload, inserindo comandos de injeção SQL ou scripts maliciosos nos campos de descrição ou nome. Quando o sistema processa automaticamente este ficheiro para criar produtos na base de dados, os comandos são executados, comprometendo a integridade do sistema.
5. Um atacante consegue acesso ao endpoint de exportação de produtos (download de ficheiro) e obtém informações confidenciais sobre todos os produtos do sistema, incluindo aqueles que ainda não foram publicados, permitindo análise competitiva indevida.
6. Um Produtor técnico explora vulnerabilidades no mecanismo de processamento de ficheiros para injetar produtos falsos ou modificar produtos existentes durante o processo de importação automática, contornando as validações normais da API.
7. Um concorrente com conhecimentos técnicos explora uma vulnerabilidade de injeção SQL no sistema para aceder antecipadamente às informações de novos produtos que serão lançados por outros Produtores, obtendo vantagem competitiva indevida.
8. Um Produtor desonesto utiliza a funcionalidade de exportação de ficheiro para obter uma lista completa de produtos de outros Produtores, incluindo detalhes que normalmente não seriam visíveis através da interface normal, e utiliza estas informações para estabelecer preços estrategicamente abaixo da concorrência.
9. Um atacante interceta comunicações não encriptadas durante o upload de fotos de produtos através do endpoint `/api/Product` e substitui as imagens por outras manipuladas, causando potenciais danos à reputação do Produtor ou inserindo conteúdo malicioso.

##### 1.4 Processo de Criação de Ofertas

**Ameaças STRIDE Aplicáveis:** S, T

**Ameaças Identificadas:**

- S (Spoofing): Criação de ofertas falsas ou fraudulentas em nome de outros Produtores, comprometendo a confiança no sistema.

- T (Tampering): Modificação não autorizada de ofertas existentes, alterando preços, quantidades, datas de entrega ou outras condições após a publicação e subscrição.

**Vetores de Ataque:**

- Manipulação de parâmetros em pedidos à API `/api/ProductOffer` para alterar ofertas após a criação

- Falsificação do identificador de Produtor (ProductId) durante a criação ou modificação de ofertas

- Exploração de falhas no controlo de acesso para modificar ofertas de outros Produtores

- Modificação direta dos registos na base de dados, contornando as validações da API

- Injeção de valores inválidos nos campos de preço (Price), quantidade (Quantity) ou data de entrega (DeliveryDate)

**Agentes de Ameaça:**

- Produtores desonestos que alteram ofertas após receberem subscrições

- Atacantes que comprometeram contas de Produtores

- Concorrentes tentando prejudicar a reputação de outros Produtores

- Administradores de sistema corruptos com acesso privilegiado

- Atacantes externos explorando vulnerabilidades na aplicação

**Casos de Abuso:**

1. Um atacante que conseguiu acesso ao sistema cria uma oferta com preços anormalmente baixos em nome de um Produtor respeitado utilizando o endpoint `/api/ProductOffer`. Quando os CoProdutores fazem subscrições, o Produtor legítimo é confrontado com pedidos que não consegue satisfazer, prejudicando a sua reputação.
2. Um Produtor publica ofertas atrativas com quantidades generosas e preços competitivos. Após receber um grande número de subscrições, modifica as condições através da API, reduzindo quantidades ou aumentando preços, alegando um erro técnico no sistema quando confrontado.
3. Um funcionário interno com acesso à base de dados modifica diretamente os campos de PaymentType em ofertas existentes, alterando os métodos de pagamento aceites após CoProdutores já terem feito subscrições, causando confusão e potenciais disputas.
4. Um Produtor desonesto consegue explorar uma vulnerabilidade no sistema para alterar o campo SubscriptionPeriodId de uma oferta, movendo-a para um período diferente após subscrições já terem sido feitas, causando problemas logísticos para CoProdutores e outros Produtores.

##### 1.5 Processo de Subscrição

**Ameaças STRIDE Aplicáveis:** T, R

**Ameaças Identificadas:**

- T (Tampering): Alteração não autorizada de dados de subscrição, como quantidades esperadas ou tipos de pagamento, após a subscrição ter sido confirmada.

- R (Repudiation): Negação de ter realizado uma subscrição específica após o recebimento do produto, tentando evitar o pagamento ou outras obrigações.

**Vetores de Ataque:**

- Manipulação de parâmetros nos pedidos à API `/api/Subscription` para alterar dados de subscrições

- Exploração de falhas no controlo de acesso para modificar subscrições de outros CoProdutores

- Modificação direta dos registos na base de dados, contornando as validações da API

- Alteração do campo ExpectedQuantity após a subscrição ter sido confirmada

- Manipulação do campo PaymentType para utilizar métodos de pagamento diferentes dos aceites na oferta

- Injeção de SQL para modificar ou eliminar registos de auditoria de subscrições

**Agentes de Ameaça:**

- CoProdutores mal-intencionados que tentam obter mais produtos pelo mesmo preço

- CoProdutores que pretendem negar transações realizadas

- Atacantes que comprometeram contas de CoProdutores legítimos

- Concorrentes que tentam reservar produtos populares para impedir outros de os obterem

- Administradores de sistema corruptos com acesso privilegiado à base de dados

**Casos de Abuso:**

1. Um CoProdutor faz uma subscrição através do endpoint `/api/Subscription` para uma quantidade específica de um produto. Após a confirmação, utiliza ferramentas para obstruir e modificar as comunicações com a API, alterando o campo ExpectedQuantity para um valor maior sem pagar a diferença.
2. Um CoProdutor faz uma subscrição, recebe e consome o produto, mas depois nega ter realizado a transação, alegando que alguém utilizou a sua conta sem autorização. Na ausência de registos de auditoria robustos, o sistema não consegue provar que foi realmente o CoProdutor a realizar a subscrição.
3. Um atacante com conhecimentos técnicos compromete uma conta de CoProdutor e realiza múltiplas subscrições para produtos populares que têm quantidades limitadas, impedindo que outros CoProdutores legítimos possam adquirir esses produtos e potencialmente criando um mercado negro.
4. Um CoProdutor mal-intencionado explora uma vulnerabilidade no sistema que permite modificar o campo ProductOfferId após a subscrição, alterando para um produto diferente ou mais caro sem pagar a diferença de preço.
5. Um atacante com acesso à base de dados modifica diretamente os registos de subscrição, alterando os detalhes da transação ou até mesmo eliminando o registo da subscrição após o produto ter sido recebido, dificultando a rastreabilidade e o controlo financeiro do sistema.

##### 1.6 Processo de Pagamento

**Ameaças STRIDE Aplicáveis:** T, R

**Ameaças Identificadas:**

- T (Tampering): Alteração não autorizada de informações de pagamento, como valores, estados de pagamento ou métodos utilizados.

- R (Repudiation): Negação de ter realizado um pagamento após a transação ter sido concluída e o produto recebido.

**Vetores de Ataque:**

- Manipulação de parâmetros nos pedidos à API `/api/Payment` para alterar detalhes de pagamentos

- Modificação direta de registos na tabela de pagamentos na base de dados

- Alteração do campo PaymentType para utilizar um método diferente do originalmente acordado

- Exploração de falhas no registo de auditoria para eliminar provas de pagamentos

- Injeção de SQL para modificar o estado ou detalhes de pagamentos existentes

- Manipulação das comunicações entre o cliente e servidor durante o processamento de pagamentos

**Agentes de Ameaça:**

- CoProdutores que tentam pagar menos do que o devido

- Utilizadores que tentam obter produtos sem efetuar o pagamento completo

- Administradores de sistema com acesso privilegiado à base de dados

- Atacantes que comprometeram contas de CoProdutores legítimos

- Insiders (funcionários) com acesso ao backend do sistema

**Casos de Abuso:**

1. Um CoProdutor tecnicamente habilidoso interceta e modifica o pedido HTTP durante a submissão de um pagamento ao endpoint `/api/Payment`, alterando o valor a ser pago para um montante menor, enquanto aparenta estar a pagar o valor total acordado.
2. Um CoProdutor realiza uma subscrição, recebe o produto, e depois modifica os registos de pagamento diretamente na base de dados (se tiver esse acesso) ou explora uma vulnerabilidade na API para marcar o pagamento como "pendente" novamente, tentando evitar o pagamento enquanto mantém o produto.
3. Um administrador de sistema ou funcionário com acesso ao backend acede indevidamente à tabela Payment na base de dados e altera o estado de pagamentos específicos para beneficiar certos CoProdutores ou prejudicar determinados Produtores.
4. Um CoProdutor realiza um pagamento utilizando um determinado PaymentType (como MBway), mas depois alega ter pago por outro método (como transferência bancária) que é mais difícil de verificar, explorando potenciais falhas no rastreamento e verificação de pagamentos.
5. Um atacante com conhecimentos técnicos compromete o sistema e modifica os registos de pagamento para redirecioná-los para contas diferentes, prejudicando financeiramente os Produtores legítimos.
6. Um CoProdutor faz um pagamento, recebe o produto, e depois nega ter autorizado o pagamento, alegando que não há provas suficientes de que foi ele quem realizou a transação, devido à falta de mecanismos robustos de não-repúdio no sistema.

##### 1.7 Processo de Controlo de Pagamentos

**Ameaças STRIDE Aplicáveis:** I, E

**Ameaças Identificadas:**

- I (Information Disclosure): Divulgação não autorizada de informações financeiras sensíveis sobre pagamentos, subscrições e transações dos CoProdutores e Produtores.

- E (Elevation of Privilege): Acesso não autorizado a funcionalidades de controlo de pagamentos que são restritas exclusivamente a utilizadores com perfil de Gestor.

**Vetores de Ataque:**

- Exploração de vulnerabilidades no controlo de acesso baseado em roles (`[Authorize(Roles = nameof(UserType.Manager))]`)

- Manipulação de tokens JWT para modificar as claims de perfil de utilizador

- Acesso direto à API `/api/PaymentsInfo` contornando as verificações de autorização

- Ataques de elevação de privilégios para obter acesso a endpoints restritos

- Intercepção de comunicações contendo dados financeiros sensíveis

- Exploração de falhas nas consultas à base de dados que recuperam informações de pagamentos

- Acesso não autorizado a endpoints como `/api/PaymentsInfo/GetPaymentsCC` que expõem dados financeiros agregados

**Agentes de Ameaça:**

- Produtores que tentam aceder a informações financeiras de outros Produtores

- CoProdutores que tentam aceder a informações de pagamentos para além das suas próprias subscrições

- Utilizadores com conhecimentos técnicos que tentam elevar privilégios

- Atacantes externos que comprometeram o sistema

- Funcionários internos sem autorização adequada

- Gestores desonestos abusando do seu acesso legítimo

**Casos de Abuso:**

1. Um Produtor com conhecimentos técnicos manipula o seu token de autenticação para adicionar a claim de perfil de Gestor (`UserType.Manager`), conseguindo assim aceder ao endpoint `/api/PaymentsInfo` para visualizar informações financeiras de todos os Produtores e CoProdutores no sistema.
2. Um atacante explora uma vulnerabilidade no controlo de acesso do sistema para contornar as anotações `[Authorize]` nos controladores, conseguindo aceder a funcionalidades de controlo de pagamentos mesmo sem ter as permissões necessárias.
3. Um funcionário interno da equipa de suporte técnico, que não deveria ter acesso a informações financeiras, utiliza credenciais de acesso à base de dados para consultar diretamente as tabelas de pagamentos, obtendo informações sensíveis sobre transações.
4. Um Gestor corrupto acede legitimamente ao módulo de controlo de pagamentos e extrai informações financeiras detalhadas de Produtores concorrentes, utilizando-as para oferecer condições mais vantajosas e roubar clientes.
5. Um atacante consegue explorar uma falha no endpoint `/api/PaymentsInfo/GetCoProducerCC` para obter informações sobre métodos de pagamento utilizados por CoProdutores, potencialmente expondo dados sensíveis financeiros.
6. Um utilizador mal-intencionado explora uma potencial falha de segurança no sistema de relatórios de pagamento, conseguindo gerar relatórios com informações financeiras detalhadas às quais não deveria ter acesso.

##### 1.8 Processo de Notificação

**Ameaças STRIDE Aplicáveis:** T, I

**Ameaças Identificadas:**

- T (Tampering): Modificação não autorizada do conteúdo das notificações enviadas aos Produtores, alterando informações sobre períodos de subscrição ou datas de entrega.

- I (Information Disclosure): Acesso não autorizado a informações potencialmente sensíveis incluídas nas notificações, como detalhes de períodos antes do anúncio oficial ou dados específicos de Produtores.

**Vetores de Ataque:**

- Intercepção e manipulação do conteúdo de e-mails enviados pela classe `EmailSender`

- Comprometimento das credenciais utilizadas pelo sistema para enviar e-mails

- Exploração de vulnerabilidades no código de geração de notificações, especialmente no método `SendEmailAsync`

- Acesso não autorizado ao serviço de e-mail utilizado pela aplicação

- Manipulação do corpo (body) do e-mail antes do envio, alterando links ou informações

- Injeção de código malicioso no HTML dos e-mails formatados

- Man-in-the-Middle durante a transmissão de e-mails entre o servidor AMAP e o servidor de email

**Agentes de Ameaça:**

- Administradores de sistema com acesso ao servidor de aplicação

- Atacantes que comprometeram a infraestrutura de rede

- Indivíduos com acesso não autorizado às credenciais de e-mail do sistema

- Insiders com acesso ao código ou à configuração do sistema de notificações

- Atacantes capazes de interceptar comunicações entre o sistema AMAP e o serviço de e-mail

**Casos de Abuso:**

1. Um atacante com acesso ao servidor onde a aplicação AMAP está a correr modifica o código da classe `EmailSender` para alterar o conteúdo das notificações sobre novos períodos de subscrição, inserindo informações falsas sobre datas de entrega ou condições, causando confusão entre os Produtores.
2. Um administrador desonesto com acesso ao sistema intercepta notificações destinadas a todos os Produtores e modifica-as para incluir links de phishing, que tenta roubar credenciais quando os Produtores clicarem nesses links pensando serem legítimos.
3. Um atacante compromete as credenciais utilizadas no método `SendEmailAsync` e utiliza-as para enviar e-mails fraudulentos em nome do sistema AMAP, potencialmente danificando a confiança dos utilizadores na plataforma.
4. Um insider com acesso ao código manipula a função de notificação sobre novos períodos, implementada no método de envio de e-mails após a criação de períodos de subscrição em `SubscriptionPeriodController`, para incluir código HTML malicioso que explorará vulnerabilidades nos navegadores dos Produtores.
5. Um atacante com acesso à configuração do sistema modifica os templates de e-mail utilizados nas notificações, alterando subtilmente informações críticas como datas de entrega, causando problemas operacionais para Produtores e CoProdutores.
6. Um administrador de sistema acede indevidamente ao conteúdo das notificações enviadas, obtendo assim informações confidenciais sobre as operações de vários Produtores, incluindo datas e outros dados comercialmente sensíveis.

#### 2. Fluxos de Dados Principais

##### 2.1 Fluxo: Credenciais de Login → Autenticação

**Ameaças STRIDE Aplicáveis:** T, I

**Ameaças Identificadas:**

- T (Tampering): Modificação das credenciais ou tokens de autenticação durante a transmissão, potencialmente permitindo acesso não autorizado.

- I (Information Disclosure): Intercepção e exposição não autorizada das credenciais de login (nome de utilizador e palavra-passe) durante a transmissão entre o cliente e o servidor.

**Vetores de Ataque:**

- Sniffing de tráfego em redes não seguras (especialmente Wi-Fi públicas) para capturar pedidos HTTP contendo credenciais

- Ataques Man-in-the-Middle (MitM) para interceptar comunicações entre o cliente e o servidor de autenticação

- Exploração de conexões não encriptadas (HTTP em vez de HTTPS) no endpoint `/api/Account/login`

- Injeção de scripts maliciosos via XSS para capturar credenciais no navegador antes do envio

- Comprometimento do canal de comunicação através de ataques de DNS spoofing

- Instalação de malware (keyloggers, trojans) no dispositivo do utilizador para capturar credenciais

- Ataques de SSL stripping para forçar o uso de conexões não encriptadas

**Agentes de Ameaça:**

- Atacantes com acesso à mesma rede local (especialmente em redes públicas)

- Administradores de rede maliciosos com capacidade de monitorizar tráfego

- Operadores de pontos de acesso Wi-Fi fraudulentos (rogue access points)

- Atacantes que conseguiram instalar malware nos dispositivos dos utilizadores

- Entidades que controlam infraestrutura de rede intermediária (ISPs, proxies)

- Atores com capacidade de monitorização de redes em grande escala

**Casos de Abuso:**

1. Um atacante cria um ponto de acesso Wi-Fi falso com um nome semelhante a uma rede legítima (ex: "CafeWifi_Free"). Quando um utilizador da AMAP se conecta e acede à plataforma, o atacante captura as credenciais transmitidas, pois a conexão não está devidamente protegida por HTTPS.
2. Um atacante realiza um ataque de DNS spoofing contra um utilizador, redirecionando o tráfego destinado ao domínio da AMAP para um servidor malicioso que imita a página de login. O utilizador insere suas credenciais, que são capturadas pelo atacante antes de reencaminhar o pedido para o servidor legítimo.
3. Um script malicioso é injetado no navegador de um utilizador através de uma vulnerabilidade XSS em outro site. Quando o utilizador acede à plataforma AMAP, o script captura as credenciais no momento em que são digitadas, antes mesmo de serem enviadas.
4. Um atacante compromete o router doméstico de um Gestor da AMAP e instala um firmware malicioso que monitoriza todo o tráfego. Quando o Gestor acede à plataforma, o atacante captura não apenas as credenciais, mas também o token JWT gerado após a autenticação, permitindo acesso prolongado mesmo sem conhecer a palavra-passe.
5. Um atacante utiliza uma ferramenta de SSL stripping num café para interceptar conexões HTTPS e degradá-las para HTTP. Quando um Produtor da AMAP acede à plataforma através desta rede comprometida, suas credenciais são transmitidas em texto simples e facilmente capturadas.

##### 2.2 Fluxo: Dados do Novo Período → Gestão de Períodos

**Ameaças STRIDE Aplicáveis:** T, I

**Ameaças Identificadas:**

- T (Tampering): Adulteração dos dados de períodos de subscrição e datas de entrega durante a transmissão entre o cliente e o servidor, em que pode criar períodos inválidos ou prejudiciais.

- I (Information Disclosure): Acesso não autorizado a informações sobre novos períodos de subscrição antes do seu anúncio oficial, proporcionando vantagens competitivas indevidas.

**Vetores de Ataque:**

- Manipulação de parâmetros JSON no corpo do pedido POST ao endpoint `/api/SubscriptionPeriod/SubscriptionPeriod`

- Intercepção de comunicações entre o cliente e o servidor durante a criação de períodos

- Modificação dos campos StartDate, EndDate ou Duration durante a transmissão

- Alteração dos dados de DeliveryDates associados a um período de subscrição

- Ataques Man-in-the-Middle para monitorizar ou modificar o tráfego

- Exploração de falhas na validação de entrada nos controladores de períodos

- Acesso não autorizado ao endpoint de consulta de períodos `/api/SubscriptionPeriod/SubscriptionPeriod/{id}`

**Agentes de Ameaça:**

- Gestores mal-intencionados com acesso legítimo ao sistema

- Atacantes externos que comprometeram a segurança da rede

- Funcionários internos com acesso parcial ao sistema

- Produtores tentando obter informação privilegiada sobre novos períodos

- Concorrentes tentando sabotar o sistema de períodos e entregas

- Administradores de sistema com acesso privilegiado ao tráfego de rede

**Casos de Abuso:**

1. Um atacante intercepta um pedido legítimo ao endpoint `/api/SubscriptionPeriod/SubscriptionPeriod` e modifica os objetos DeliveryDates no corpo do pedido, alterando as datas para momentos impossíveis (ex: fins-de-semana ou feriados), causando problemas logísticos tanto para Produtores quanto para CoProdutores.
2. Um Gestor desonesto cria um período de subscrição com datas favoráveis para um grupo específico de Produtores, mas, durante a transmissão, um atacante modifica os dados para beneficiar outros Produtores, gerando confusão e potenciais conflitos no sistema.
3. Um funcionário da equipa técnica com acesso à infraestrutura de rede monitoriza os pedidos ao endpoint de criação de períodos e recolhe informações sobre novos períodos planeados antes do anúncio oficial. Estas informações são depois vendidas a Produtores específicos, dando-lhes vantagem para prepararem a sua produção antecipadamente.
4. Um atacante explora uma vulnerabilidade no controller `SubscriptionPeriodController` que permite visualizar períodos ainda não publicados através de um endpoint de consulta mal protegido, obtendo assim informações privilegiadas sobre datas futuras de entrega.
5. Um Produtor tecnicamente habilidoso consegue interceptar as notificações enviadas pelo método de e-mail implementado após a criação de períodos, obtendo informações detalhadas sobre novos períodos antes que outros Produtores as recebam, permitindo-lhe planear estrategicamente as suas ofertas.

##### 2.3 Fluxo: Dados de Subscrição para Pagamento → Pagamento

**Ameaças STRIDE Aplicáveis:** T, I

**Ameaças Identificadas:**

- T (Tampering): Modificação não autorizada dos dados de subscrição durante a transmissão para o processo de pagamento, alterando valores, quantidades ou detalhes do método de pagamento.

- I (Information Disclosure): Acesso não autorizado a informações de transações durante a transmissão entre o processo de subscrição e o processo de pagamento.

**Vetores de Ataque:**

- Manipulação dos parâmetros de pedido enviados ao endpoint `/api/Payment`

- Alteração do valor do campo ExpectedQuantity na subscrição, afetando o valor final a pagar

- Modificação do tipo de pagamento (PaymentType) para utilizar um método diferente do acordo inicial (ex: alterar de transferência bancária para pagamento em dinheiro)

- Intercepção das comunicações entre o módulo de subscrições e o módulo de pagamentos

- Exploração de vulnerabilidades na passagem de dados entre os controllers `SubscriptionController` e `PaymentController`

- Man-in-the-Middle entre componentes do sistema durante o processamento de pagamentos

**Agentes de Ameaça:**

- CoProdutores mal-intencionados que tentam pagar menos do que o devido

- Atacantes externos que comprometeram a segurança da aplicação

- Administradores de sistema com acesso aos servidores

- Funcionários internos com conhecimento da arquitetura do sistema

- Utilizadores técnicos com capacidade para interceptar e modificar comunicações internas

**Casos de Abuso:**

1. Um CoProdutor tecnicamente habilidoso interceta o pedido HTTP quando os dados da sua subscrição são enviados para processamento de pagamento. Utilizando ferramentas como um proxy interceptor, modifica o campo de quantidade esperada ou o preço unitário, reduzindo o valor total a pagar sem afetar a quantidade de produto a receber.
1. Um atacante que comprometeu o sistema explora uma falha na validação de dados entre os módulos de subscrição e pagamento, modificando o PaymentType de uma transação de transferência bancária para dinheiro em mão, dificultando a verificação do pagamento e potencialmente permitindo que um pagamento seja registado como concluído sem realmente o ser.
1. Um administrador de sistema monitoriza o tráfego interno da aplicação e captura informações sobre transações, incluindo detalhes de pagamento como valores, métodos de pagamento escolhidos e padrões de compra dos CoProdutores, obtendo inteligência de negócio indevida.
1. Um funcionário interno com acesso à base de dados explora o momento em que os dados são transmitidos entre a tabela Subscription e a tabela Payment, modificando valores para beneficiar certos CoProdutores ou prejudicar determinados Produtores.
1. Um atacante realiza um Man-in-the-Middle entre os componentes internos da aplicação, modificando os dados de pagamento para alterar registos, manipulando a forma como os dados são passados do SubscriptionController para processamento de pagamento.
1. Um CoProdutor desonesto explora uma vulnerabilidade no processo de verificação de PaymentType que permite que ele selecione um método de pagamento que na realidade não está disponível para aquela oferta específica, contornando as restrições impostas pelo Produtor.

#### 3. Armazenamentos de Dados

##### 3.1 Armazenamento: Utilizadores

**Ameaças STRIDE Aplicáveis:** T, I

**Ameaças Identificadas:**

- T (Tampering): Modificação não autorizada de dados de utilizador, incluindo tipo de utilizador (UserType), informações de perfil ou credenciais, comprometendo a integridade do sistema.

- I (Information Disclosure): Acesso não autorizado a informações sensíveis do utilizador, como palavras-passe, dados pessoais ou roles de sistema, violando a confidencialidade.

**Vetores de Ataque:**

- Ataques de injeção SQL nos endpoints de gestão de utilizadores para manipular dados na tabela AspNetUsers

- Exploração de vulnerabilidades na classe AppUser ou no Identity Framework para alterar o campo UserType

- Acesso direto não autorizado à base de dados contornando a camada de aplicação

- Falhas na proteção de backups da base de dados contendo a tabela de utilizadores

- Exploração de vulnerabilidades em stored procedures ou queries que manipulam dados de utilizadores

- Acessos privilegiados indevidos às tabelas relacionadas com AspNetUsers ou AspNetUserRoles

- Manipulação de tokens de autenticação para obter acesso a funcionalidades de gestão de utilizadores

**Agentes de Ameaça:**

- Administradores de sistema com privilégios elevados na base de dados

- Utilizadores com conhecimentos técnicos tentando elevar os seus privilégios

- Atacantes externos que conseguiram comprometer a segurança do sistema

- Insiders (ex: desenvolvedores, administradores) com acesso legítimo aos sistemas

- Ex-funcionários cujos acessos não foram devidamente revogados

- Atacantes que conseguiram acesso às credenciais de serviço ou ficheiros de configuração

**Casos de Abuso:**

1. Um atacante utiliza uma vulnerabilidade de injeção SQL no endpoint de autenticação para modificar o campo UserType na tabela AspNetUsers, alterando o seu perfil de CoProdutor para Gestor, obtendo assim acesso a funcionalidades administrativas do sistema AMAP.
1. Um administrador de base de dados corrupto acede diretamente à tabela AspNetUsers e altera as passwords hash armazenadas, comprometendo contas de Produtores específicos para beneficiar certos CoProdutores em detrimento de outros.
1. Um atacante consegue acesso a um backup não encriptado da base de dados e extrai informações sensíveis, incluindo endereços de email, dados de perfil e potencialmente passwords hash dos utilizadores, particularmente visando contas de Gestores com acesso privilegiado.
1. Um antigo funcionário de TI que mantém acesso aos servidores modifica os dados de perfil (Name e outros campos de AppUser) de Produtores específicos, causando confusão e potencial desinformação para os CoProdutores que interagem com esses perfis.
1. Um desenvolvedor interno com acesso ao sistema de staging da aplicação modifica os mapeamentos do Entity Framework ou do Identity para alterar como as roles são atribuídas aos utilizadores, criando um backdoor que lhe permite elevar privilégios quando necessário sem deixar rastos óbvios nos logs do sistema.
1. Um administrador com acesso à base de dados poderia criar contas adicionais com privilégios de Gestor (UserType.Manager), inserindo-as diretamente nas tabelas do ASP.NET Identity. Estas contas maliciosas permitiriam manter acesso permanente aos módulos administrativos do sistema, mesmo que outras atividades suspeitas fossem detectadas e investigadas.

##### 3.2 Armazenamento: Ofertas de Produtos

Ameaças STRIDE Aplicáveis: T, I

Ameaças Identificadas:

- T (Tampering): Modificação não autorizada de ofertas de produtos, incluindo preços, quantidades disponíveis, datas de entrega ou métodos de pagamento aceites, comprometendo a integridade do sistema.

- I (Information Disclosure): Acesso antecipado ou não autorizado a informações de ofertas, proporcionando vantagens competitivas indevidas ou expondo dados comercialmente sensíveis.

Vetores de Ataque:

- Ataques de injeção SQL direcionados à tabela ProductOffer para manipular dados das ofertas

- Acesso direto não autorizado à base de dados para modificar registos de ofertas

- Exploração de vulnerabilidades na API /api/ProductOffer para alterar ofertas sem autorização adequada

- Manipulação do campo ProductId para associar uma oferta a um produto diferente

- Alteração não autorizada dos campos Price, Quantity ou DeliveryDate após subscrições terem sido feitas

- Modificação maliciosa do campo PaymentType para restringir ou ampliar as opções de pagamento

- Falhas no controlo de acesso que permitem a Produtores modificar ofertas de outros Produtores

- Exploração de vulnerabilidades no relacionamento entre ProductOffer e SubscriptionPeriod

Agentes de Ameaça:

- Produtores desonestos tentando manipular o mercado ou prejudicar concorrentes

- Administradores de sistema com acesso privilegiado à base de dados

- Atacantes externos que comprometeram a segurança da aplicação

- Desenvolvedores internos com acesso ao código fonte e à base de dados

- CoProdutores técnicos tentando modificar ofertas para obter melhores condições

- Concorrentes tentando sabotar ou espionar atividades na plataforma

Casos de Abuso:

1. Um Produtor tecnicamente habilidoso explora uma vulnerabilidade no controlo de acesso da API /api/ProductOffer para modificar ofertas de outros Produtores, reduzindo os preços dos concorrentes a níveis não lucrativos ou alterando as datas de entrega para momentos impossíveis, prejudicando a sua reputação.
2. Um atacante com acesso direto à base de dados modifica o campo Price em ofertas existentes na tabela ProductOffer após CoProdutores já terem feito subscrições, criando disputas financeiras quando chegar o momento do pagamento ou da entrega.
3. Um administrador de sistema corrupto utiliza o seu acesso privilegiado para eliminar seletivamente ofertas de determinados Produtores, particularmente as mais atrativas, para favorecer outros Produtores com quem tem acordos clandestinos.
4. Um desenvolvedor interno malicioso cria um backdoor no código que processa as ofertas, permitindo-lhe monitorizar e modificar silenciosamente o campo Quantity de ofertas populares, reduzindo a disponibilidade para manipular artificialmente a procura ou reservar produtos para CoProdutores específicos.
5. Um atacante que comprometeu as credenciais de um Produtor acede à sua conta e modifica o campo PaymentType em todas as suas ofertas para incluir apenas métodos de pagamento difíceis de rastrear, facilitando potenciais fraudes futuras.
6. Um CoProdutor com conhecimentos técnicos consegue explorar uma falha de segurança que lhe permite aceder antecipadamente aos dados de novas ofertas antes de serem publicadas, obtendo assim uma vantagem injusta para garantir as melhores subscrições antes que outros CoProdutores tenham conhecimento delas.
7. Um atacante realiza um ataque de injeção SQL através de uma vulnerabilidade na API de ofertas, conseguindo modificar o campo SubscriptionPeriodId para mover ofertas entre diferentes períodos de subscrição, causando confusão logística e potenciais problemas para Produtores e CoProdutores.

##### 3.3 Armazenamento: Pagamentos

Ameaças STRIDE Aplicáveis: T, I

Ameaças Identificadas:

- T (Tampering): Alteração não autorizada de registos de pagamentos, modificando estados, valores ou associações entre pagamentos e subscrições, comprometendo a integridade financeira do sistema.

- I (Information Disclosure): Acesso não autorizado a informações financeiras sensíveis, incluindo métodos de pagamento utilizados e histórico de transações dos utilizadores.

Vetores de Ataque:

- Acesso direto não autorizado à tabela Payment na base de dados

- Ataques de injeção SQL no endpoint /api/Payment para manipular registos de pagamento

- Exploração de vulnerabilidades no PaymentController para modificar estados de pagamento

- Manipulação dos relacionamentos entre as tabelas Payment e Subscription na base de dados

- Modificação não autorizada do campo PaymentType para alterar o método de pagamento registado

- Comprometimento de credenciais de administrador ou de contas com acesso ao sistema de pagamentos

- Exploração de backups não protegidos contendo dados financeiros

- Ataque de elevação de privilégios para obter acesso às funcionalidades de gestão de pagamentos

Agentes de Ameaça:

- Funcionários com acesso ao sistema financeiro ou às APIs de pagamento

- Gestores desonestos com acesso legítimo ao controlo de pagamentos

- Administradores de sistema com acesso privilegiado à base de dados

- Atacantes externos visando informações financeiras sensíveis

- CoProdutores mal-intencionados tentando manipular registos de pagamento

- Ex-funcionários com conhecimento do sistema e potenciais acessos remanescentes

- Hackers especializados em fraudes financeiras

Casos de Abuso:

1. Um funcionário com acesso ao backend da aplicação modifica diretamente registos na tabela Payment para indicar que um pagamento foi realizado quando na verdade não foi, permitindo que CoProdutores específicos recebam produtos sem terem efetivamente pago por eles.
2. Um administrador de sistema corrupto utiliza o seu acesso privilegiado para extrair informações detalhadas sobre os métodos de pagamento e padrões de transação de todos os CoProdutores, vendendo estas informações valiosas para empresas de marketing ou concorrentes.
3. Um atacante que conseguiu comprometer uma conta de Gestor acede ao endpoint /api/PaymentsInfo/GetPaymentsCC e obtém informações completas sobre o estado de pagamentos de todos os CoProdutores, utilizando estes dados para identificar alvos vulneráveis para fraudes.
4. Um desenvolvedor interno cria uma backdoor no sistema que lhe permite modificar o campo PaymentType dos registos existentes, alterando o método de pagamento registado para dificultar a reconciliação financeira e encobrir desvios de fundos.
5. Um hacker realiza um ataque de injeção SQL através de uma vulnerabilidade no sistema, conseguindo manipular a tabela Payment para alterar a associação entre pagamentos e subscrições, atribuindo pagamentos legítimos a subscrições diferentes das originais.
6. Um administrador de base de dados elimina seletivamente registos de pagamento da tabela Payment para encobrir fraudes ou irregularidades cometidas por CoProdutores específicos, comprometendo a auditabilidade do sistema.
7. Um atacante com acesso físico a backups não encriptados do servidor extrai toda a base de dados, incluindo o histórico completo de pagamentos da tabela Payment, obtendo informações valiosas sobre comportamentos financeiros dos utilizadores e potencialmente dados sensíveis de pagamento.
8. Um funcionário do departamento financeiro com acesso legítimo ao sistema utiliza o seu acesso para monitorizar padrões de pagamento de CoProdutores específicos, obtendo inteligência competitiva indevida sobre volumes de compra e preferências, que depois vende a terceiros.

##### 3.4 Armazenamento: Produtos

Ameaças STRIDE Aplicáveis: T, I

Ameaças Identificadas:

- T (Tampering): Modificação não autorizada de informações de produtos, incluindo descrições, preços de referência, fotos ou unidades de entrega, afetando a integridade do catálogo.

- I (Information Disclosure): Acesso não autorizado a informações potencialmente confidenciais sobre produtos, incluindo detalhes que ainda não foram publicados.

Vetores de Ataque:

- Ataques de injeção SQL direcionados à tabela Product para manipular dados de produtos

- Acesso direto não autorizado à base de dados para modificar registos de produtos

- Exploração de vulnerabilidades na API /api/Product para alterar produtos sem autorização adequada

- Manipulação do campo ProducerId para associar produtos a outros Produtores

- Alteração não autorizada dos campos Name, Description, Photo ou ReferencePrice

- Exploração de vulnerabilidades no upload de ficheiros para substituir fotos por conteúdo malicioso

- Ataques à funcionalidade de importação/exportação de produtos via ficheiros CSV

Agentes de Ameaça:

- Produtores desonestos tentando manipular informações de produtos concorrentes

- Administradores de sistema com acesso privilegiado à base de dados

- Atacantes externos que comprometeram a segurança da aplicação

- Concorrentes tentando sabotar ou copiar catálogos de produtos

- Utilizadores maliciosos explorando funcionalidades de upload de ficheiros

Casos de Abuso:

1. Um Produtor tecnicamente habilidoso explora uma vulnerabilidade no controlo de acesso da API /api/Product para modificar descrições ou fotos de produtos de concorrentes, prejudicando a sua imagem ou reputação.
2. Um atacante com acesso à base de dados modifica os preços de referência (ReferencePrice) de produtos, criando expectativas irrealistas e causando confusão quando as ofertas forem criadas com preços muito diferentes.
3. Um atacante explora a funcionalidade de upload de ficheiros para substituir fotos de produtos por imagens contendo código malicioso que será executado quando visualizadas no browser.
4. Um concorrente utiliza a funcionalidade de importação/exportação de produtos para obter um catálogo completo de todos os produtos da plataforma, usando esta informação para análise competitiva indevida.

##### 3.5 Armazenamento: Subscrições

Ameaças STRIDE Aplicáveis: T, I

Ameaças Identificadas:

- T (Tampering): Modificação não autorizada de dados de subscrições, alterando quantidades esperadas, associações com ofertas ou informações de CoProdutor.

- I (Information Disclosure): Acesso não autorizado a informações de subscrições, revelando padrões de compra de CoProdutores ou volumes de negócio de Produtores.

Vetores de Ataque:

- Ataques de injeção SQL na tabela Subscription para manipular registos

- Acesso direto não autorizado à base de dados

- Exploração de vulnerabilidades na API /api/Subscription

- Modificação do campo ExpectedQuantity após a confirmação

- Alteração da associação entre subscrições e ofertas através do campo ProductOfferId

- Manipulação do campo CoProducerId para associar subscrições a diferentes utilizadores

Agentes de Ameaça:

- CoProdutores maliciosos tentando alterar suas próprias subscrições

- Atacantes externos com acesso à aplicação

- Administradores de sistema com acesso privilegiado

- Produtores tentando manipular volumes de subscrições dos seus produtos

Casos de Abuso:

1. Um CoProdutor tecnicamente habilidoso modifica o campo ExpectedQuantity da sua subscrição após a confirmação, aumentando a quantidade sem pagar a diferença.
2. Um atacante com acesso à base de dados altera o campo ProductOfferId de subscrições existentes, redirecionando-as para ofertas diferentes e prejudicando o planeamento de produção.
3. Um administrador de sistema extrai dados de todas as subscrições para realizar análise de mercado não autorizada, identificando padrões de compra valiosos que vende a concorrentes.

##### 3.6 Armazenamento: Períodos de Subscrição

Ameaças STRIDE Aplicáveis: T, I

Ameaças Identificadas:

- T (Tampering): Modificação não autorizada de períodos de subscrição e datas de entrega, comprometendo todo o ciclo operacional da plataforma.

- I (Information Disclosure): Acesso prematuro a informações sobre períodos futuros, proporcionando vantagens competitivas indevidas.

Vetores de Ataque:

- Ataques de injeção SQL à tabela SubscriptionPeriod

- Acesso direto não autorizado à base de dados

- Exploração de vulnerabilidades na API /api/SubscriptionPeriod

- Manipulação dos campos StartDate, EndDate ou Duration

- Alteração não autorizada da coleção de DeliveryDates associada a um período

Agentes de Ameaça:

- Gestores desonestos com acesso legítimo ao sistema

- Atacantes externos que comprometeram a segurança

- Administradores de sistema com acesso privilegiado

- Produtores buscando informações antecipadas para vantagem competitiva

Casos de Abuso:

1. Um Gestor corrupto modifica as datas de entrega num período já publicado, criando conflitos logísticos para Produtores específicos.
2. Um atacante com acesso à base de dados altera o campo EndDate de períodos ativos, estendendo-os artificialmente ou encerrando-os prematuramente.
3. Um administrador de sistema acede antecipadamente a informações sobre novos períodos planeados e vende esta informação privilegiada a Produtores específicos.

#### 4. Entidades Externas

##### 4.1 Entidade Externa: Gestor

Ameaças STRIDE Aplicáveis: S, R

Ameaças Identificadas:

- S (Spoofing): Falsificação da identidade de um Gestor para obter acesso administrativo ao sistema AMAP, permitindo controlo sobre períodos de subscrição, monitorização de pagamentos e outras operações privilegiadas.

- R (Repudiation): Negação de ações administrativas realizadas, especialmente criação ou modificação de períodos de subscrição, alteração de datas de entrega ou outras operações críticas para o funcionamento do sistema.

Vetores de Ataque:

- Roubo ou comprometimento de credenciais de acesso de utilizadores com perfil UserType.Manager

- Exploração de vulnerabilidades no processo de autenticação do endpoint /api/Account/login

- Ataques de phishing direcionados a contas de Gestor

- Manipulação de tokens JWT para falsificar claims de autorização

- Acesso não autorizado a sessões ativas em dispositivos desprotegidos

- Ausência de autenticação de dois fatores para operações administrativas críticas

- Falhas no registo (logging) de ações administrativas no sistema

- Exploração de vulnerabilidades no AccountController para elevação de privilégios

Agentes de Ameaça:

- Ex-funcionários com conhecimento do sistema e potenciais credenciais ativas

- Atacantes externos motivados a obter controlo administrativo

- Produtores tentando obter privilégios elevados para manipular períodos ou visualizar informações privilegiadas

- Insiders maliciosos com acesso físico a dispositivos de Gestores

- Atacantes sofisticados capazes de comprometer contas através de engenharia social

- Funcionários descontentes com intenção de causar danos ao sistema

Casos de Abuso:

1. Um ex-funcionário que anteriormente tinha perfil de Gestor (UserType.Manager) utiliza credenciais que não foram revogadas após a sua saída para aceder ao endpoint /api/SubscriptionPeriod/SubscriptionPeriod e criar períodos de subscrição com datas de entrega impossíveis, causando confusão e problemas logísticos para Produtores e CoProdutores.
2. Um atacante realiza um ataque de phishing direcionado enviando um email falso sobre uma suposta violação de segurança da plataforma AMAP. Quando o Gestor clica no link e insere as suas credenciais, o atacante captura-as e utiliza-as para aceder ao sistema, criando contas adicionais com privilégios administrativos para manter acesso permanente.
3. Um Produtor tecnicamente habilidoso explora uma vulnerabilidade no controlo de acesso baseado em roles, manipulando o seu token JWT para incluir claims de UserType.Manager. Com este acesso elevado, acede ao endpoint /api/PaymentsInfo para visualizar informações financeiras confidenciais sobre outros Produtores e CoProdutores.
4. Um Gestor realiza modificações nos períodos de subscrição através da API /api/SubscriptionPeriod, favorecendo certos Produtores com datas de entrega mais convenientes. Quando questionado sobre estas alterações, nega ter efetuado tais mudanças, aproveitando-se da ausência de logs detalhados ou assinaturas digitais que comprovem a autoria das ações.
5. Um atacante com acesso físico temporário ao computador de um Gestor que deixou a sua sessão aberta consegue aceder ao sistema administrativo e modificar configurações críticas, como os métodos de pagamento aceites ou os períodos de subscrição ativos, sem deixar vestígios da sua identidade real.
6. Um funcionário de TI com acesso aos sistemas aproveita uma falha no controlo de acesso para criar contas de Gestor fantasma que não aparecem nas listagens normais, usando estas contas para monitorizar todas as atividades do sistema e extrair informações comercialmente sensíveis sem ser detetado.

##### 4.2 Entidade Externa: Produtor

Ameaças STRIDE Aplicáveis: S, R

Ameaças Identificadas:

- S (Spoofing): Falsificação da identidade de um Produtor para criar produtos, ofertas ou interagir com CoProdutores, potencialmente danificando a reputação do Produtor legítimo ou realizando atividades fraudulentas.

- R (Repudiation): Negação por parte de um Produtor de ter criado ofertas específicas, definido certos preços ou concordado com condições particulares, após estas terem sido subscritas por CoProdutores.

Vetores de Ataque:

- Comprometimento de credenciais de contas com perfil UserType.Producer

- Ataques de phishing direcionados a Produtores ativos na plataforma

- Exploração de vulnerabilidades no processo de registo para criar contas de Produtor fraudulentas

- Reutilização de credenciais vazadas de outros serviços

- Acesso não autorizado a dispositivos de Produtores com sessões ativas

- Exploração de falhas no ProductController ou ProductOfferController

- Ataques de força bruta contra o endpoint /api/Account/login

- Manipulação de tokens de sessão para aceder como Produtor específico

Agentes de Ameaça:

- Produtores concorrentes tentando prejudicar a reputação de outros Produtores

- Atacantes externos visando ganhos financeiros através de fraudes

- Indivíduos mal-intencionados criando contas falsas de Produtor

- Produtores desonestos que negam intencionalmente suas próprias ações

- Hackers contratados para sabotar operações de Produtores específicos

- Atacantes motivados pela disrupção do mercado ou danos à plataforma

Casos de Abuso:

1. Um atacante compromete a conta de um Produtor bem estabelecido através de um ataque de phishing e utiliza o acesso ao endpoint /api/Product para adicionar produtos de qualidade inferior ou descrições enganosas. Isto danifica a reputação do Produtor legítimo e resulta em reclamações quando os CoProdutores recebem produtos que não correspondem às descrições originais.
2. Um Produtor desonesto cria deliberadamente ofertas com preços artificialmente baixos através do endpoint /api/ProductOffer, atraindo numerosas subscrições. Quando chega o momento de entregar, alega ter ocorrido um "erro técnico" no sistema ao definir os preços, tentando renegociar valores mais elevados ou entregando quantidades inferiores às acordadas.
3. Um atacante cria uma conta falsa de Produtor, passando-se por um agricultor local respeitado. Utiliza a API /api/Product para cadastrar produtos aparentemente legítimos e depois cria ofertas atraentes via /api/ProductOffer. Após receber pagamentos adiantados dos CoProdutores (utilizando o método PaymentType.BankTransfer), desaparece sem entregar nenhum produto.
4. Um Produtor concorrente compromete a conta de outro Produtor e modifica intencionalmente os campos DeliveryDate nas suas ofertas para datas impossíveis ou conflituantes, prejudicando a sua capacidade de cumprir compromissos e danificando a sua reputação junto dos CoProdutores.
5. Um Produtor cria ofertas com condições específicas, mas após várias subscrições e quando os preços de mercado aumentam, tenta negar as condições originais alegando não ter definido aqueles preços ou quantidades. Na ausência de logs detalhados ou assinaturas digitais nas transações, consegue repudiar parcialmente os termos originais.
6. Um atacante que obteve credenciais de vários Produtores modifica as suas ofertas para aceitar apenas pagamentos difíceis de rastrear (PaymentType.Money), facilitando um esquema de fraude onde os pagamentos são desviados sem deixar provas digitais.
7. Um indivíduo mal-intencionado cria várias contas falsas de Produtor, explora vulnerabilidades no sistema para estabelecer reputação artificial e, em seguida, cria grande volume de ofertas atraentes para um período futuro. Após acumular um valor significativo em pagamentos antecipados, todas as contas desaparecem num ataque coordenado de "exit scam".

##### 4.3 Entidade Externa: CoProdutor

Ameaças STRIDE Aplicáveis: S, R

Ameaças Identificadas:

- S (Spoofing): Falsificação da identidade de um CoProdutor para realizar subscrições fraudulentas, aceder a informações pessoais ou modificar detalhes de entregas e pagamentos.

- R (Repudiation): Negação por parte de um CoProdutor de ter realizado uma subscrição específica ou um pagamento, após ter recebido os produtos, tentando evitar obrigações financeiras.

Vetores de Ataque:

- Comprometimento de credenciais de contas com perfil UserType.CoProducer

- Ataques de phishing direcionados a CoProdutores ativos na plataforma

- Exploração de vulnerabilidades no processo de registo para criar múltiplas contas fraudulentas

- Acesso não autorizado a sessões de CoProdutores em dispositivos desprotegidos

- Engenharia social para obter acesso a contas legítimas

- Ataques de força bruta contra o endpoint /api/Account/login

- Manipulação de tokens JWT para falsificar a identidade de um CoProdutor

- Exploração de falhas no SubscriptionController ou no registo de transações de pagamento

Agentes de Ameaça:

- Atacantes visando benefícios financeiros através de fraudes

- Indivíduos tentando obter produtos sem efetuar o pagamento correspondente

- CoProdutores desonestos que repudiam intencionalmente suas próprias transações

- Concorrentes tentando manipular a disponibilidade de produtos

- Hackers procurando dados pessoais e financeiros para uso malicioso

- Atacantes oportunistas explorando falhas no sistema de verificação de pagamentos

Casos de Abuso:

1. Um atacante compromete a conta de um CoProdutor através de credenciais obtidas num ataque de phishing e utiliza o acesso ao endpoint /api/Subscription para modificar as informações de entrega, redirecionando produtos para outro local. O CoProdutor legítimo não recebe os produtos e o sistema mostra que a entrega foi realizada.
2. Um CoProdutor faz uma subscrição através do endpoint /api/Subscription, define o método de pagamento como transferência bancária (PaymentType.BankTransfer), recebe os produtos e depois nega ter autorizado a transação. Na ausência de provas robustas de pagamento ou assinaturas digitais, consegue repudiar a transação e evitar o pagamento.
3. Um atacante explora vulnerabilidades no processo de registo para criar dezenas de contas falsas de CoProdutor e utiliza-as coordenadamente para subscrever todas as ofertas de produtos populares ou limitados assim que ficam disponíveis. Isto impede que CoProdutores legítimos tenham acesso a estas ofertas e potencialmente cria um mercado negro ou simplesmente sabota o sistema.
4. Um CoProdutor desonesto realiza uma subscrição para uma grande quantidade de produtos, recebe-os e efetua um pagamento. Posteriormente, contacta o seu banco alegando não reconhecer a transação, realizando um chargeback. Devido à falta de provas suficientes de entrega ou autorização, o sistema AMAP acaba por sofrer perda financeira.
5. Um atacante que obteve acesso à conta de um CoProdutor monitoriza as suas subscrições regulares para identificar padrões de compra. Utiliza esta informação para criar um perfil de consumo detalhado que pode ser vendido para fins de marketing ou para fraudes mais sofisticadas.
6. Um indivíduo mal-intencionado cria uma conta falsa de CoProdutor e utiliza meios fraudulentos para estabelecer um histórico de transações aparentemente legítimas. Gradualmente aumenta o volume de compras até realizar uma grande subscrição de produtos valiosos. Após indicar falsamente que o pagamento foi realizado e receber os produtos, desaparece sem deixar rasto.
7. Um CoProdutor explora uma vulnerabilidade no sistema de verificação de pagamentos, manipulando os dados enviados ao endpoint /api/Payment para fazer parecer que um pagamento foi concluído quando na realidade não foi. Isto permite que obtenha produtos sem efetivamente pagar por eles.
8. Um atacante com conhecimentos técnicos compromete várias contas de CoProdutores e utiliza-as para criar um padrão artificial de alta procura para certos produtos. Isto pode manipular os preços ou criar escassez artificial, que posteriormente pode ser explorada para benefício financeiro.

##### 4.4 Entidade Externa: Sistema de Email

Ameaças STRIDE Aplicáveis: S, I

Ameaças Identificadas:

-  (Spoofing): Falsificação do sistema de email da AMAP para enviar notificações maliciosas a Produtores, CoProdutores e Gestores, potencialmente comprometendo as suas credenciais ou dispositivos.

- I (Information Disclosure): Intercepção ou acesso não autorizado a notificações contendo informações sensíveis sobre períodos de subscrição, ofertas, pagamentos ou dados pessoais dos utilizadores.

Vetores de Ataque:

- Comprometimento das credenciais SMTP utilizadas pelo método SendEmailAsync na classe EmailSender

- Modificação maliciosa das configurações de email no ficheiro appsettings.json

- Interceptação de comunicações entre o servidor AMAP e o serviço de email

- Ataques de Man-in-the-Middle na infraestrutura de rede

- Ataques de DNS poisoning para redirecionar o tráfego SMTP

- Comprometimento do próprio serviço de email utilizado pela aplicação

- Criação de domínios de phishing similares ao domínio oficial da AMAP

- Exploração de vulnerabilidades na biblioteca de email utilizada pela aplicação

- Acesso não autorizado aos templates de email ou ao código que gera as notificações

Agentes de Ameaça:

- Atacantes com acesso à infraestrutura de rede ou de email

- Administradores de sistema maliciosos com acesso às configurações

- Desenvolvedores internos com acesso ao código fonte

- Phishers especializados em imitar comunicações oficiais

- Concorrentes visando informações privilegiadas

- Hackers tentando comprometer contas de utilizador através de engenharia social

- Entidades maliciosas interessadas em interromper comunicações críticas

Casos de Abuso:

1. Um atacante compromete as credenciais SMTP armazenadas em appsettings.json e utiliza-as para enviar emails fraudulentos em nome da AMAP. Estes emails contêm links de phishing que direcionam Produtores e CoProdutores para uma página falsa que imita o login do sistema, onde as suas credenciais são capturadas.
2. Um administrador de sistema malicioso modifica o código no método SendEmailAsync da classe EmailSender para encaminhar cópias de todas as notificações para um endereço de email externo. Isto permite ao atacante recolher informações confidenciais, como detalhes sobre novos períodos de subscrição antes do seu anúncio oficial ou informações sobre pagamentos.
3. Um atacante realiza um ataque de DNS poisoning, redirecionando o tráfego SMTP da aplicação AMAP para um servidor controlado pelo atacante. Isto permite interceptar todas as comunicações por email, extrair informações sensíveis e potencialmente modificar o conteúdo antes de reencaminhar as mensagens para os destinatários legítimos.
4. Um phisher cria uma campanha sofisticada utilizando um domínio muito similar ao oficial (por exemplo, ama-p.com em vez de amap.com), enviando notificações falsas sobre problemas de pagamento. Estas notificações incluem um link para uma página falsa, onde os CoProdutores são incentivados a inserir novamente os seus dados de pagamento, que são então capturados pelo atacante.
5. Um atacante com acesso à base de dados extrai endereços de email de todos os utilizadores e depois utiliza um serviço externo para enviar notificações falsas em massa, contendo malware disfarçado de "atualização necessária" da plataforma AMAP.
6. Um concorrente compromete o sistema de email para monitorizar as notificações enviadas aos Produtores sobre novos períodos de subscrição, obtendo assim informação privilegiada que permite planear estrategicamente a sua própria produção ou ofertas.
7. Um desenvolvedor interno insere código malicioso na função que notifica os Produtores após a criação de um novo período de subscrição, alterando subtilmente dados críticos como datas de entrega ou condições, sem modificar a base de dados. Isto causa confusão quando os Produtores tentam planear a sua produção com base nas informações recebidas.

#### 5. Limites de Confiança

##### 5.1 Limite: Database Boundary

Ameaças STRIDE Aplicáveis: T, I,

Ameaças Identificadas:

- T (Tampering): Manipulação não autorizada de dados quando cruzam o limite entre a aplicação e a base de dados, comprometendo a integridade das informações armazenadas.

- I (Information Disclosure): Exposição não autorizada de dados sensíveis durante a comunicação entre a aplicação e a base de dados, violando a confidencialidade das informações.

Vetores de Ataque:

- Ataques de injeção SQL explorando consultas parametrizadas mal implementadas nos repositórios da aplicação

- Exploração de vulnerabilidades no Entity Framework Core utilizado para acesso à base de dados

- Manipulação de queries LINQ para obter dados não autorizados

- Acesso direto não autorizado à base de dados contornando a camada de aplicação

- Exploração de configurações de conexão inseguras no appsettings.json

- Manipulação de transações de base de dados para comprometer a integridade dos dados

- Ataques de Man-in-the-Middle na comunicação entre a aplicação e a base de dados

- Exploração de falhas no controlo de acesso implementado no DbContext

Agentes de Ameaça:

- Desenvolvedores com conhecimento do modelo de dados e da estrutura das entidades

- Administradores de TI com acesso aos servidores de aplicação ou de base de dados

- Atacantes externos que conseguiram comprometer a aplicação

- Insiders com acesso parcial ao sistema e conhecimento da arquitetura

- Utilizadores tentando elevar privilégios através de vulnerabilidades na camada de dados

- Hackers especializados em ataques a bases de dados SQL Server

Casos de Abuso:

1. Um atacante identifica uma vulnerabilidade de injeção SQL num dos métodos de filtragem dos repositórios que não utiliza corretamente consultas parametrizadas do Entity Framework. Através desta vulnerabilidade, executa o ataque ' OR 1=1; -- para obter acesso a todos os registos da tabela AspNetUsers, incluindo informações de perfil e hashes de passwords.
2. Um administrador de sistemas com acesso ao servidor de aplicação extrai as credenciais de conexão à base de dados do ficheiro appsettings.json e utiliza-as para estabelecer uma conexão direta à base de dados, contornando toda a lógica de autorização implementada na aplicação. Isto permite-lhe aceder, modificar ou eliminar dados sensíveis como informações de pagamento na tabela Payment.
3. Um desenvolvedor malicioso insere um backdoor na camada de acesso a dados da aplicação, modificando as classes de repositório que gerem o acesso a entidades como Subscription ou ProductOffer. Este backdoor permite enviar cópias dos dados sensíveis para um servidor externo controlado pelo atacante, sempre que determinadas consultas são executadas.
4. Um utilizador com conhecimentos técnicos explora uma vulnerabilidade no mecanismo de filtragem baseado em UserType implementado no DbContext. Manipulando as consultas LINQ, consegue contornar as restrições que normalmente limitariam o acesso apenas aos dados relacionados com o seu próprio utilizador, obtendo assim acesso a informações de outros CoProdutores ou Produtores.
5. Um atacante que conseguiu acesso ao servidor de aplicação intercepta a comunicação entre a aplicação e a base de dados. Mesmo sem conseguir desencriptar o tráfego, observa padrões de comunicação que revelam informações sobre operações sensíveis, como criação de novos períodos ou processamento de pagamentos.
6. Um administrador de base de dados malicioso modifica os procedimentos armazenados ou funções que são utilizados pela aplicação, inserindo lógica que subtilmente altera os dados quando determinadas condições são atendidas. Por exemplo, poderia manipular os valores de pagamento ou redirecionar subscrições para beneficiar certos Produtores.
7. Um atacante explora uma vulnerabilidade no controlo de transações da base de dados, conseguindo iniciar transações pendentes que bloqueiam recursos críticos. Isto pode resultar num ataque de negação de serviço efetivo, impedindo que operações legítimas como pagamentos ou subscrições sejam processadas.
8. Um desenvolvedor interno com acesso ao código fonte explora falhas na implementação dos mapeamentos do Entity Framework, manipulando as relações entre entidades como Subscription, Payment e ProductOffer para criar inconsistências na base de dados que podem ser posteriormente exploradas para fraudes.

##### 5.2 Limite: Perímetro de Autenticação

Ameaças STRIDE Aplicáveis: S, T

Ameaças Identificadas:

- S (Spoofing): Falsificação de identidade para contornar o processo de autenticação e obter acesso não autorizado ao sistema.

- T (Tampering): Manipulação dos dados de autenticação durante a transmissão ou processamento, permitindo comprometer o mecanismo de verificação de identidade.

Vetores de Ataque:

- Ataques de força bruta contra o endpoint /api/Account/login

- Interceptação de tokens JWT utilizados para autenticação

- Manipulação de claims dentro dos tokens de autenticação

- Exploração de vulnerabilidades no ASP.NET Core Identity

- Ataques de session fixation ou session hijacking

- Roubo de cookies de autenticação através de ataques XSS

- Manipulação dos cabeçalhos de autenticação em requisições API

Agentes de Ameaça:

- Atacantes externos tentando obter acesso não autorizado

- Ex-utilizadores tentando recuperar acesso após desativação

- Utilizadores tentando elevar seus privilégios de acesso

- Atacantes especializados em segurança de autenticação web

Casos de Abuso:

1. Um atacante utiliza uma ferramenta automatizada para realizar ataques de força bruta contra o endpoint /api/Account/login, tentando descobrir credenciais válidas de Gestores para obter acesso administrativo.
2. Um atacante que conseguiu um token JWT válido manipula as claims no payload, alterando o valor de UserType de "CoProducer" para "Manager", tentando assim elevar seus privilégios de acesso.
3. Um utilizador com conhecimentos técnicos explora uma falha na implementação do refresh token para manter acesso perpétuo ao sistema, mesmo após a tentativa de revogação dos seus privilégios.

##### 5.3 Limite: Controlo de Acesso por Perfil

Ameaças STRIDE Aplicáveis: E, I 

Ameaças Identificadas:

- E (Elevation of Privilege): Contornar os mecanismos de controlo de acesso baseados em perfil (UserType) para aceder a funcionalidades restritas.

- I (Information Disclosure): Obter acesso a informações que deveriam estar restritas ao perfil do utilizador, violando a confidencialidade.

Vetores de Ataque:

- Manipulação de atributos de autorização como [Authorize(Roles = nameof(UserType.Manager))]

- Exploração de falhas nas verificações de autorização nos controladores

- Acesso direto a endpoints que não verificam adequadamente permissões

- Falsificação de tokens com claims de autorização manipuladas

- Ataques horizontais, tentando aceder a recursos de outros utilizadores do mesmo nível

Agentes de Ameaça:

- CoProdutores tentando aceder a funcionalidades de Produtores

- Produtores tentando aceder a funcionalidades de Gestores

- Utilizadores tentando aceder a dados de outros utilizadores do mesmo tipo

- Atacantes que comprometeram contas de nível básico

Casos de Abuso:

1. Um Produtor descobriu que o endpoint /api/PaymentsInfo verifica a autorização apenas na interface e não no backend. Enviando requisições diretas à API, consegue aceder a informações financeiras que deveriam estar restritas a Gestores.
2. Um CoProdutor manipula os parâmetros de requisição ao acessar o endpoint /api/Subscription para visualizar ou modificar subscrições de outros CoProdutores, explorando uma falha na verificação de propriedade dos recursos.

##### 5.4 Limite: Integração com Sistema de Email

Ameaças STRIDE Aplicáveis: T, I 

Ameaças Identificadas:

- T (Tampering): Manipulação do conteúdo das mensagens durante a transmissão entre o sistema AMAP e o serviço de email.

- I (Information Disclosure): Intercepção de informações sensíveis contidas nas notificações enviadas pelo sistema.

Vetores de Ataque:

- Interceptação da comunicação SMTP entre o sistema e o servidor de email

- Comprometimento das credenciais de email no appsettings.json

- Manipulação dos templates de email antes do envio

- Ataques de Man-in-the-Middle na comunicação com o servidor SMTP

- Exploração de configurações inseguras na comunicação com o serviço de email

Agentes de Ameaça:

- Atacantes com acesso à rede ou infraestrutura

- Insiders com acesso às configurações do sistema

- Entidades capazes de interceptar tráfego de rede

Casos de Abuso:

1. Um atacante com acesso à rede intercepta a comunicação SMTP não encriptada entre o sistema AMAP e o servidor de email, conseguindo capturar o conteúdo de todas as notificações enviadas, incluindo informações sobre novos períodos de subscrição e confirmações de pagamento.
2. Um administrador de sistema malicioso modifica os templates de email utilizados para notificações, inserindo links maliciosos ou alterando informações críticas como datas de entrega ou valores de pagamento.

### Tabela Resumo de Ameaças STRIDE

| Elemento DFD                      | Ameaças STRIDE | Principais Vetores de Ataque                                 |
| --------------------------------- | -------------- | ------------------------------------------------------------ |
| **Processos**                     |                |                                                              |
| 1.1 Autenticação                  | S, E           | Roubo de credenciais, manipulação de tokens JWT, ataques de phishing |
| 1.2 Gestão de Períodos            | T, E           | Manipulação da API, exploração de falhas RBAC, injeção SQL   |
| 1.3 Gestão de Produtos            | T, I           | Manipulação da API, upload de ficheiros maliciosos, exploração de importação CSV |
| 1.4 Criação de Ofertas            | S, T           | Falsificação de identidade, modificação de ofertas após subscrições |
| 1.5 Subscrição                    | T, R           | Alteração de parâmetros, negação de transações               |
| 1.6 Pagamento                     | T, R           | Modificação de registos, repúdio de pagamentos               |
| 1.7 Controlo de Pagamentos        | I, E           | Exploração de falhas no controlo de acesso, manipulação de JWT |
| 1.8 Notificação                   | T, I           | Comprometimento do sistema de email, manipulação de conteúdo |
| **Fluxos de Dados**               |                |                                                              |
| 2.1 Credenciais → Autenticação    | T, I           | Sniffing de rede, MitM, ataques XSS                          |
| 2.2 Dados de Período → Gestão     | T, I           | Intercepção de comunicações, manipulação de parâmetros       |
| 2.3 Subscrição → Pagamento        | T, I           | Manipulação de valores de pagamento, intercepção de dados financeiros |
| **Armazenamentos**                |                |                                                              |
| 3.1 Utilizadores                  | T, I           | Injeção SQL, acesso direto à BD, manipulação de perfis       |
| 3.2 Ofertas de Produtos           | T, I           | Modificação não autorizada de ofertas, acesso prematuro a informações |
| 3.3 Pagamentos                    | T, I           | Alteração de registos de pagamento, exposição de dados financeiros |
| 3.4 Produtos                      | T, I           | Manipulação de descrições/fotos, acesso a dados confidenciais |
| 3.5 Subscrições                   | T, I           | Modificação de quantidades, acesso a padrões de compra       |
| 3.6 Períodos de Subscrição        | T, I           | Alteração de datas, acesso prematuro a informações de períodos |
| **Entidades Externas**            |                |                                                              |
| 4.1 Gestor                        | S, R           | Falsificação de identidade, repúdio de ações administrativas |
| 4.2 Produtor                      | S, R           | Criação de contas falsas, negação de termos de ofertas       |
| 4.3 CoProdutor                    | S, R           | Falsificação de identidade, repúdio de subscrições/pagamentos |
| 4.4 Sistema de Email              | S, I           | Comprometimento de credenciais SMTP, intercepção de mensagens |
| **Limites de Confiança**          |                |                                                              |
| 5.1 Database Boundary             | T, I           | Injeção SQL, acesso direto à BD, manipulação de queries      |
| 5.2 Perímetro de Autenticação     | S, T           | Falsificação de identidade, manipulação de tokens            |
| 5.3 Controlo de Acesso por Perfil | E, I           | Contorno de verificações RBAC, acesso a dados restritos      |
| 5.4 Integração Sistema de Email   | T, I           | Intercepção SMTP, manipulação de templates                   |

## Conclusão

A análise STRIDE identificou diversas ameaças potenciais ao sistema AMAP, aplicadas a cada elemento do DFD. As ameaças mais relevantes estão relacionadas com autenticação, manipulação de dados de pagamento, e proteção de informações sensíveis.

Os vetores de ataque principais incluem: injeção SQL, ataques de Man-in-the-Middle, engenharia social, manipulação de parâmetros de requisição, e exploração de vulnerabilidades de autenticação. Os agentes de ameaça mais significativos são atacantes externos, insiders maliciosos, e utilizadores legítimos que tentam abusar do sistema.

Esta análise fornece a base para o desenvolvimento de estratégias de mitigação e requisitos de segurança nas próximas fases do projeto, garantindo que o sistema AMAP esteja protegido contra as ameaças identificadas.

---

## Avaliação de Risco

### Metodologia de Avaliação

A avaliação de risco foi realizada utilizando a metodologia DREAD (ou outra metodologia simples), atribuindo pontuações aos seguintes critérios: Dano, Reprodutibilidade, Exploração, Atingibilidade e Descoberta.

# Avaliação de Risco Utilizando DREAD para o Sistema AMAP

## 1. Introdução à Metodologia
Para avaliar o risco das ameaças identificadas no sistema AMAP, aplicaremos a metodologia DREAD conforme sugerido pelo professor. Esta abordagem permite uma avaliação quantitativa dos riscos através de cinco fatores de risco:

- **Damage potential (Potencial de dano):** Quão significativo seria o dano se a ameaça fosse explorada?
- **Reproducibility (Reprodutibilidade):** Quão fácil é reproduzir o ataque?
- **Exploitability (Facilidade de exploração):** Quão fácil é executar o ataque?
- **Affected users (Usuários afetados):** Que percentagem de usuários seria afetada?
- **Discoverability (Facilidade de descoberta):** Quão fácil é descobrir esta vulnerabilidade?

## 2. Critérios de Avaliação DREAD
Para cada fator, utilizaremos uma escala de 1-10, onde 1 representa o menor risco e 10 o maior:

### Damage Potential (D)
- **1-3:** Dano mínimo, sem impacto significativo
- **4-6:** Dano moderado, impacto temporário nas operações
- **7-8:** Dano significativo, impacto nas finanças ou reputação
- **9-10:** Dano crítico, comprometimento do sistema ou risco legal

### Reproducibility (R)
- **1-3:** Muito difícil de reproduzir, requer condições específicas raras
- **4-6:** Reproduzível com esforço considerável
- **7-8:** Reproduzível com algum esforço e conhecimento específico
- **9-10:** Facilmente reproduzível em qualquer momento

### Exploitability (E)
- **1-3:** Requer conhecimento avançado e múltiplos pré-requisitos
- **4-6:** Requer conhecimento técnico moderado e acesso específico
- **7-8:** Requer habilidades básicas e poucas barreiras
- **9-10:** Muito fácil de explorar, não requer conhecimento técnico especial

### Affected Users (A)
- **1-3:** Afeta pouquíssimos usuários individuais
- **4-6:** Afeta um grupo específico de usuários
- **7-8:** Afeta a maioria dos usuários
- **9-10:** Afeta todos os usuários do sistema

### Discoverability (D)
- **1-3:** Extremamente difícil de descobrir
- **4-6:** Descobrível apenas com análise direcionada
- **7-8:** Facilmente descobrível com ferramentas comuns
- **9-10:** Óbvia de detectar, visível por qualquer pessoa

## 3. Avaliação de Risco das Principais Ameaças

### Processo de Autenticação (1.1)
**Spoofing (S):** Falsificação de identidade para acesso não autorizado

| Fator DREAD | Pontuação | Justificativa |
|-------------|-----------|---------------|
| Damage | 10 | Comprometimento de uma conta de Gestor permite controle administrativo total do sistema |
| Reproducibility | 8 | Ataques de phishing e credential stuffing são amplamente reproduzíveis |
| Exploitability | 7 | Técnicas como phishing são relativamente fáceis de implementar |
| Affected Users | 9 | Afeta todos os usuários do sistema, principalmente quando comprometidas contas de Gestores |
| Discoverability | 8 | Vulnerabilidades de autenticação são comumente pesquisadas e detectadas |
| **Pontuação DREAD** | **8.4** | **CRÍTICO** |

**Elevation of Privilege (E):** Escalação de privilégios

| Fator DREAD | Pontuação | Justificativa |
|-------------|-----------|---------------|
| Damage | 9 | Permite acesso a funcionalidades administrativas restritas |
| Reproducibility | 6 | Requer conhecimento específico do sistema de tokens JWT |
| Exploitability | 5 | Necessita compreensão do mecanismo de autorização e autenticação |
| Affected Users | 8 | Impacta potencialmente todos os usuários se explorado |
| Discoverability | 5 | Requer análise específica do sistema de autorização |
| **Pontuação DREAD** | **6.6** | **ALTO** |

### Processo de Gestão de Períodos de Subscrição (1.2)
**Tampering (T):** Modificação não autorizada de períodos de subscrição

| Fator DREAD | Pontuação | Justificativa |
|-------------|-----------|---------------|
| Damage | 9 | Compromete todo o planeamento de distribuição e entrega de produtos |
| Reproducibility | 7 | Reproduzível ao explorar vulnerabilidades no controlo de acesso |
| Exploitability | 6 | Requer conhecimento técnico e acesso específico ao sistema |
| Affected Users | 8 | Afeta todos os Produtores e CoProdutores que dependem das datas dos períodos |
| Discoverability | 6 | Requer análise específica dos mecanismos de gestão de períodos |
| **Pontuação DREAD** | **7.2** | **ALTO** |

**Elevation of Privilege (E):** Acesso indevido às funcionalidades de gestão de períodos

| Fator DREAD | Pontuação | Justificativa |
|-------------|-----------|---------------|
| Damage | 8 | Permite manipulação de períodos fundamentais para o funcionamento do sistema |
| Reproducibility | 6 | Requer exploração de vulnerabilidades específicas no RBAC |
| Exploitability | 5 | Necessita conhecimento sobre o mecanismo de controlo de acesso |
| Affected Users | 8 | Impacta potencialmente todos os utilizadores do sistema |
| Discoverability | 5 | Requer análise específica do sistema de autorização |
| **Pontuação DREAD** | **6.4** | **ALTO** |

### Gestão de Produtos e Quantidades (1.3)
**Tampering (T):** Alteração não autorizada de informações de produtos

| Fator DREAD | Pontuação | Justificativa |
|-------------|-----------|---------------|
| Damage | 8 | Compromete a confiança na plataforma e pode resultar em perdas financeiras |
| Reproducibility | 9 | Facilmente reproduzível quando explorada uma vulnerabilidade de validação |
| Exploitability | 7 | Viável para Produtores ou atacantes com acesso ao sistema |
| Affected Users | 7 | Afeta CoProdutores e Produtores relacionados aos produtos comprometidos |
| Discoverability | 8 | Vulnerabilidades de validação de dados são frequentemente identificáveis |
| **Pontuação DREAD** | **7.8** | **ALTO** |

**Information Disclosure (I):** Divulgação não autorizada de informações

| Fator DREAD | Pontuação | Justificativa |
|-------------|-----------|---------------|
| Damage | 6 | Exposição de dados de produtos pode dar vantagens competitivas indevidas |
| Reproducibility | 7 | Reproduzível em sistemas com controle de acesso inadequado |
| Exploitability | 6 | Requer algum conhecimento específico do sistema |
| Affected Users | 5 | Afeta principalmente Produtores cujas informações são expostas |
| Discoverability | 6 | Necessita análise específica dos mecanismos de controle de acesso |
| **Pontuação DREAD** | **6.0** | **ALTO** |

### Processo de Pagamento (1.6)
**Tampering (T):** Alteração não autorizada de informações de pagamento

| Fator DREAD | Pontuação | Justificativa |
|-------------|-----------|---------------|
| Damage | 10 | Impacto financeiro direto e comprometimento da integridade transacional |
| Reproducibility | 7 | Reproduzível em sistemas com validação inadequada |
| Exploitability | 6 | Requer conhecimento técnico para interceptar/modificar comunicações |
| Affected Users | 8 | Afeta Produtores e CoProdutores envolvidos nas transações |
| Discoverability | 6 | Requer análise focada nas transações de pagamento |
| **Pontuação DREAD** | **7.4** | **ALTO** |

**Repudiation (R):** Negação de ter realizado um pagamento

| Fator DREAD | Pontuação | Justificativa |
|-------------|-----------|---------------|
| Damage | 9 | Perdas financeiras diretas e problemas de confiança na plataforma |
| Reproducibility | 8 | Bastante viável na ausência de mecanismos robustos de não-repúdio |
| Exploitability | 7 | Relativamente simples de executar se o sistema não possui logs adequados |
| Affected Users | 7 | Afeta principalmente Produtores que não recebem pagamentos |
| Discoverability | 8 | Facilmente descobrível em testes básicos de pagamento |
| **Pontuação DREAD** | **7.8** | **ALTO** |

### Database Boundary (5.1)
**Information Disclosure (I):** Exposição de dados sensíveis

| Fator DREAD | Pontuação | Justificativa |
|-------------|-----------|---------------|
| Damage | 10 | Exposição de todos os dados do sistema, incluindo informações financeiras e pessoais |
| Reproducibility | 7 | Reproduzível quando identificadas vulnerabilidades de injeção SQL |
| Exploitability | 6 | Requer conhecimento técnico de SQL e da estrutura do banco |
| Affected Users | 10 | Potencialmente afeta todos os usuários do sistema |
| Discoverability | 8 | Vulnerabilidades de injeção SQL são comumente detectáveis com ferramentas automatizadas |
| **Pontuação DREAD** | **8.2** | **CRÍTICO** |

**Tampering (T):** Manipulação não autorizada de dados

| Fator DREAD | Pontuação | Justificativa |
|-------------|-----------|---------------|
| Damage | 10 | Compromete a integridade de todo o sistema |
| Reproducibility | 6 | Requer identificação de vulnerabilidades específicas |
| Exploitability | 5 | Necessita conhecimento técnico avançado e acesso específico |
| Affected Users | 9 | Pode afetar todos os usuários dependendo dos dados comprometidos |
| Discoverability | 5 | Requer análise direcionada das consultas e transações |
| **Pontuação DREAD** | **7.0** | **ALTO** |

### Gestor (4.1)
**Spoofing (S):** Falsificação da identidade de um Gestor

| Fator DREAD | Pontuação | Justificativa |
|-------------|-----------|---------------|
| Damage | 10 | Acesso completo às funcionalidades administrativas críticas |
| Reproducibility | 7 | Reproduzível através de ataques de phishing bem estruturados |
| Exploitability | 5 | Requer direcionar especificamente Gestores, que são usuários mais cautelosos |
| Affected Users | 9 | Impacta potencialmente todos os usuários do sistema |
| Discoverability | 6 | Necessita reconhecimento de alvos específicos e engenharia social |
| **Pontuação DREAD** | **7.4** | **ALTO** |

### Sistema de Email (4.4)
**Information Disclosure (I):** Interceptação de notificações

| Fator DREAD | Pontuação | Justificativa |
|-------------|-----------|---------------|
| Damage | 7 | Exposição de informações potencialmente sensíveis sobre períodos e operações |
| Reproducibility | 6 | Requer acesso à infraestrutura de rede ou email |
| Exploitability | 5 | Necessita conhecimentos técnicos específicos e posição privilegiada |
| Affected Users | 8 | Pode afetar grande parte dos usuários que recebem comunicações |
| Discoverability | 5 | Requer monitoramento específico do tráfego SMTP |
| **Pontuação DREAD** | **6.2** | **ALTO** |

## 4. Matriz de Risco Consolidada
Organizando as ameaças avaliadas em ordem decrescente de risco:

| Ameaça | Pontuação DREAD | Nível de Risco |
|--------|----------------|---------------|
| Autenticação - Spoofing | 8.4 | CRÍTICO |
| Database Boundary - Information Disclosure | 8.2 | CRÍTICO |
| Fluxo Credenciais Login - Information Disclosure | 8.2 | CRÍTICO |
| Gestão de Produtos - Tampering | 7.8 | ALTO |
| Processo de Pagamento - Repudiation | 7.8 | ALTO |
| Gestor - Spoofing | 7.4 | ALTO |
| Processo de Pagamento - Tampering | 7.4 | ALTO |
| Criação de Ofertas - Spoofing | 7.4 | ALTO |
| Criação de Ofertas - Tampering | 7.4 | ALTO |
| Fluxo Credenciais Login - Tampering | 7.4 | ALTO |
| Gestão de Períodos - Tampering | 7.2 | ALTO |
| Processo de Subscrição - Repudiation | 7.2 | ALTO |
| Database Boundary - Tampering | 7.0 | ALTO |
| Processo de Subscrição - Tampering | 7.0 | ALTO |
| Fluxo Subscrição para Pagamento - Tampering | 7.0 | ALTO |
| Autenticação - Elevation of Privilege | 6.6 | ALTO |
| Controlo de Pagamentos - Information Disclosure | 6.6 | ALTO |
| Fluxo Dados Período - Tampering | 6.6 | ALTO |
| Gestão de Períodos - Elevation of Privilege | 6.4 | ALTO |
| Controlo de Pagamentos - Elevation of Privilege | 6.4 | ALTO |
| Processo de Notificação - Information Disclosure | 6.4 | ALTO |
| Sistema de Email - Information Disclosure | 6.2 | ALTO |
| Processo de Notificação - Tampering | 6.2 | ALTO |
| Gestão de Produtos - Information Disclosure | 6.0 | ALTO |
| Fluxo Dados Período - Information Disclosure | 6.0 | ALTO |
| Fluxo Subscrição para Pagamento - Information Disclosure | 6.0 | ALTO |

## 5. Análise dos Resultados e Justificativa da Priorização
A avaliação de risco DREAD mostra que as ameaças mais críticas para o sistema AMAP estão relacionadas com:

1. **Autenticação e controlo de acesso:** Falsificação de identidade (spoofing) representa um risco crítico pois compromete todo o modelo de segurança do sistema. O comprometimento de contas de Gestor oferece controlo administrativo completo. A interceção de credenciais durante a transmissão também foi classificada como crítica.

2. **Segurança do banco de dados:** Vulnerabilidades que permitem acesso não autorizado a dados sensíveis representam um risco crítico pois podem comprometer todas as informações do sistema, incluindo dados pessoais e financeiros.

3. **Integridade de transações e operações:** A manipulação de dados (tampering) em várias componentes do sistema, incluindo produtos, períodos, ofertas e subscrições, representa um risco alto para a integridade das operações. O repúdio de pagamentos e subscrições impacta diretamente a confiabilidade financeira do sistema.

4. **Fluxos de comunicação interna e externa:** Interceptação ou modificação de dados durante a transmissão entre componentes do sistema representa um risco significativo, especialmente para credenciais de login e informações financeiras.

A diferenciação na priorização destas ameaças deve-se principalmente a:

- **Potencial de dano (Damage):** Ameaças que impactam a integridade financeira ou comprometem todo o sistema receberam pontuações mais altas.
- **Utilizadores afetados (Affected Users):** Ameaças que afetam todos ou a maioria dos utilizadores foram priorizadas sobre aquelas que impactam apenas grupos específicos.
- **Reprodutibilidade e Exploitabilidade:** Ameaças facilmente reproduzíveis e exploráveis representam maior risco imediato.

As ameaças foram classificadas em três níveis de prioridade:

1. **Crítico (DREAD ≥ 8.0):** Requerem mitigação imediata e monitorização contínua.
   - Autenticação - Spoofing (8.4)
   - Database Boundary - Information Disclosure (8.2)
   - Fluxo Credenciais Login - Information Disclosure (8.2)

2. **Alto (6.0 ≤ DREAD < 8.0):** Necessitam de mitigação prioritária e verificações regulares.
   - Inclui ameaças à integridade dos dados, repúdio de transações e manipulação de fluxos.

3. **Médio (4.0 ≤ DREAD < 6.0):** Requerem monitorização e mitigação conforme recursos disponíveis.
   - Nenhuma ameaça avaliada foi classificada neste nível, demonstrando a criticidade geral do sistema.

---

## Mitigações Propostas

### Abordagem de Mitigação

As mitigações propostas estão organizadas por processo e risco específico, focando exclusivamente no backend do sistema AMAP, considerando sua arquitetura em camadas ASP.NET Core, Entity Framework Core e SQL Server.

### Mitigações por Processo e Risco Específico

#### Processo 1.1: Autenticação

**Risco: Spoofing (8.4 - CRÍTICO)**
1. **Fortalecimento de credenciais:**
   - Implementar políticas de password robustas no `AccountController`: mínimo 12 caracteres, combinação obrigatória de maiúsculas, minúsculas, números e caracteres especiais
   - Modificar o `UserManager<AppUser>` para utilizar PBKDF2 com 10.000+ iterações para hashing de passwords
   - Verificar passwords contra base de dados de passwords comprometidas (integrar API HaveIBeenPwned)

2. **Proteção contra ataques de força bruta:**
   - Implementar rate limiting no endpoint `/api/Account/login` com bloqueio progressivo
   - Configurar o ASP.NET Identity para bloquear contas após 5 tentativas falhadas por 15 minutos
   - Implementar CAPTCHA em tentativas suspeitas de login, utilizando reCAPTCHA v3

3. **Prevenção de roubo de sessão:**
   - Implementar fingerprinting de dispositivo nas claims do JWT
   - Manter histórico de dispositivos autenticados por usuário
   - Enviar notificações automáticas para o email do utilizador quando um novo dispositivo é detectado

**Risco: Elevation of Privilege (6.6 - ALTO)**
1. **Segurança de tokens JWT:**
   - Reduzir o tempo de expiração do token para 15 minutos no serviço `JwtService`
   - Assinar tokens com algoritmo RS256 utilizando chaves assimétricas
   - Validar todas as claims do token em cada requisição com filtros de autorização personalizados

2. **Gestão de sessões segura:**
   - Implementar refresh tokens com rotação única armazenados numa tabela específica
   - Manter registo central de tokens na base de dados com capacidade de revogação imediata
   - Destruir todos os tokens ativos quando um utilizador altera a sua password

3. **Controlo de privilégios:**
   - Verificar permissões baseadas no UserType em cada endpoint usando middleware especializado
   - Validar as claims de UserType contra a informação armazenada na base de dados em cada requisição 
   - Implementar logs detalhados de alterações de UserType com aprovação explícita de Gestores

#### Processo 1.2: Gestão de Períodos de Subscrição

**Risco: Tampering (7.2 - ALTO)**
1. **Validação reforçada de dados:**
   - Implementar validações rigorosas em `SubscriptionPeriodController` para datas e períodos
   - Adicionar verificações de integridade de dados e regras de negócio no serviço correspondente
   - Criar regras de validação para garantir que datas de início, fim e entrega seguem a lógica de negócio

2. **Proteção contra manipulação de períodos:**
   - Implementar bloqueio automático de períodos após a publicação no `SubscriptionPeriodController`
   - Criar uma tabela de auditoria específica para registar todas as operações em períodos
   - Gerar hashes criptográficos dos períodos para detectar alterações não autorizadas

3. **Notificações de modificações:**
   - Enviar notificações automáticas via `EmailSender` para todos os Produtores quando houver alterações
   - Implementar mecanismo de confirmação para alterações críticas em períodos já publicados
   - Registar quem fez cada alteração com timestamp e dados completos (antes/depois)

**Risco: Elevation of Privilege (6.4 - ALTO)**
1. **Reforço do controlo de acesso:**
   - Implementar múltiplas camadas de autorização no `SubscriptionPeriodController`
   - Utilizar políticas de autorização personalizadas específicas para gestão de períodos
   - Verificar explicitamente o UserType.Manager em cada método do controlador

2. **Segregação de funcionalidades:**
   - Separar endpoints administrativos em área protegida com autorização adicional
   - Implementar verificações secundárias para operações críticas (política de quatro olhos)
   - Criar logs específicos para tentativas de acesso não autorizado à gestão de períodos

#### Processo 1.3: Gestão de Produtos e Quantidades

**Risco: Tampering (7.8 - ALTO)**
1. **Proteção da integridade dos produtos:**
   - Implementar validação rigorosa no `ProductController` com regras de negócio específicas
   - Criar checksums ou hashes de produtos para detectar modificações não autorizadas
   - Desenvolver sistema de versões para produtos, mantendo histórico de alterações

2. **Validação de uploads:**
   - Implementar validação rigorosa para uploads de imagens de produtos
   - Verificar e sanitizar metadados de imagens antes do armazenamento
   - Restringir tipos de ficheiros permitidos com validação de MIME type e conteúdo

3. **Proteção contra manipulação de dados:**
   - Implementar bloqueio de alterações em produtos após subscrição
   - Notificar automaticamente CoProdutores quando houver alterações em produtos subscritos
   - Validar modificações de preços contra regras de negócio estabelecidas

**Risco: Information Disclosure (6.0 - ALTO)**
1. **Controlo de acesso aos dados:**
   - Implementar filtros automáticos no Entity Framework Core baseados no perfil do utilizador
   - Criar serviço dedicado para controlar o acesso a informações de produtos
   - Desenvolver mascaramento de informações sensíveis baseado no perfil do utilizador

2. **Proteção de dados confidenciais:**
   - Implementar controlo de visibilidade de produtos baseado no estado (rascunho/publicado)
   - Limitar o acesso a dados completos de produtos apenas aos proprietários
   - Registar todos os acessos a informações de produtos em log de auditoria

#### Processo 1.4: Criação de Ofertas

**Risco: Spoofing (7.4 - ALTO)**
1. **Verificação de identidade do Produtor:**
   - Implementar validação cruzada no `ProductOfferController` para garantir que o Produtor é dono do produto
   - Criar mecanismo de assinatura digital de ofertas pelo Produtor
   - Implementar verificações em múltiplas camadas para confirmar a identidade do Produtor

2. **Proteção contra falsificação:**
   - Adicionar tokens anti-falsificação em todas as operações de criação de ofertas
   - Implementar sistema de verificação para confirmar a propriedade dos produtos nas ofertas
   - Enviar notificações ao Produtor quando novas ofertas forem criadas em seu nome

**Risco: Tampering (7.4 - ALTO)**
1. **Integridade das ofertas:**
   - Implementar controlo de versão para ofertas no `ProductOfferController`
   - Registar hashes criptográficos das ofertas para detectar alterações não autorizadas
   - Bloquear alterações em ofertas após o início de subscrições, exceto em campos específicos

2. **Validação reforçada:**
   - Adicionar validações rigorosas de preço, quantidade e datas no controlador e na camada de serviço
   - Implementar regras de negócio específicas para alterações de ofertas após publicação
   - Criar sistema de notificação automática para alterações em ofertas já subscritas

3. **Auditoria de alterações:**
   - Implementar log detalhado de todas as modificações em ofertas
   - Registar o autor, data/hora e valores antes/depois de cada alteração
   - Criar relatórios de alterações para análise pelos Gestores

#### Processo 1.5: Subscrição

**Risco: Tampering (7.0 - ALTO)**
1. **Proteção de dados de subscrição:**
   - Implementar validação rigorosa no `SubscriptionController` para todos os campos
   - Criar tokens únicos para identificar cada subscrição e prevenir manipulação
   - Implementar verificações de integridade entre subscrição e oferta relacionada

2. **Prevenção de alterações não autorizadas:**
   - Bloquear alterações em campos críticos após confirmação da subscrição
   - Implementar workflow de aprovação para modificações posteriores
   - Validar a consistência dos dados em cada etapa do processo de subscrição

**Risco: Repudiation (7.2 - ALTO)**
1. **Mecanismos de não-repúdio:**
   - Implementar assinaturas digitais para subscrições utilizando chaves privadas
   - Criar timestamps confiáveis (RFC 3161) para todas as operações de subscrição
   - Manter logs imutáveis de todas as ações do CoProdutor no processo de subscrição

2. **Auditoria completa:**
   - Registar todas as etapas do processo de subscrição em log dedicado
   - Armazenar evidências de consentimento do CoProdutor (IP, User-Agent, timestamp)
   - Criar funcionalidade de exportação de provas de subscrição para resolução de disputas

#### Processo 1.6: Pagamento

**Risco: Tampering (7.4 - ALTO)**
1. **Integridade dos dados de pagamento:**
   - Implementar checksums para verificar a integridade dos dados no `PaymentController`
   - Criar tokens de transação únicos e não sequenciais para cada pagamento
   - Validar a consistência entre os dados de subscrição e pagamento em cada etapa

2. **Proteção contra manipulação:**
   - Bloquear alterações em pagamentos após confirmação
   - Implementar processo formal para estornos ou cancelamentos
   - Criar fluxo seguro para modificações excepcionais com múltiplas aprovações

**Risco: Repudiation (7.8 - ALTO)**
1. **Evidências de pagamento:**
   - Implementar trilhas de auditoria detalhadas para todas as operações financeiras
   - Gerar recibos digitais assinados para cada transação concluída
   - Armazenar provas de pagamento com assinaturas digitais e carimbos de tempo

2. **Não-repúdio de transações:**
   - Implementar confirmações explícitas multi-etapa para pagamentos
   - Criar confirmações secundárias via canais alternativos (email, SMS)
   - Manter logs criptograficamente assinados de todas as ações do utilizador

3. **Rastreabilidade completa:**
   - Registar o fluxo completo desde a subscrição até o pagamento
   - Implementar IDs de correlação entre subscrições e pagamentos
   - Manter histórico imutável de todas as transações financeiras

#### Processo 1.7: Controlo de Pagamentos

**Risco: Information Disclosure (6.6 - ALTO)**
1. **Proteção de dados financeiros:**
   - Implementar mascaramento de dados sensíveis no `PaymentsInfoController`
   - Criar views específicas por perfil para limitar a exposição de informações financeiras
   - Implementar encriptação para dados financeiros sensíveis em trânsito e armazenamento

2. **Controlo de acesso granular:**
   - Desenvolver políticas de autorização específicas para acesso a dados financeiros
   - Implementar filtros automáticos baseados no perfil e responsabilidades do utilizador
   - Manter logs detalhados de todos os acessos a informações de pagamento

**Risco: Elevation of Privilege (6.4 - ALTO)**
1. **Segregação de responsabilidades:**
   - Implementar verificação estrita de roles no `PaymentsInfoController`
   - Criar níveis de acesso diferenciados para funções específicas de gestão financeira
   - Implementar autorização baseada em políticas para operações de maior sensibilidade

2. **Validação reforçada de autorização:**
   - Verificar permissões em múltiplas camadas da aplicação
   - Implementar verificações secundárias para operações financeiras críticas
   - Criar sistema de alertas para tentativas de acesso não autorizado

#### Processo 1.8: Notificação

**Risco: Tampering (6.2 - ALTO)**
1. **Integridade das notificações:**
   - Implementar assinaturas digitais para o conteúdo de emails enviados pelo `EmailSender`
   - Criar templates pré-aprovados e imutáveis para comunicações críticas
   - Implementar validação do conteúdo das mensagens antes do envio

2. **Proteção do canal de comunicação:**
   - Configurar o serviço `EmailSender` para utilizar TLS 1.2+ para comunicação SMTP
   - Implementar DKIM, SPF e DMARC para autenticação de emails
   - Validar certificados do servidor SMTP antes de enviar informações sensíveis

**Risco: Information Disclosure (6.4 - ALTO)**
1. **Proteção de dados em notificações:**
   - Minimizar a inclusão de dados sensíveis em emails, preferindo links seguros
   - Criar políticas de conteúdo para diferentes tipos de notificações
   - Implementar mascaramento automático de informações sensíveis em comunicações

2. **Gestão segura de credenciais:**
   - Armazenar credenciais SMTP em Azure Key Vault em vez de configurações
   - Implementar rotação periódica de senhas de serviços de email
   - Monitorizar logs do serviço de email para detetar atividades suspeitas

#### Processo 5.1: Database Boundary

**Risco: Information Disclosure (8.2 - CRÍTICO)**
1. **Proteção de dados sensíveis:**
   - Implementar encriptação transparente de dados (TDE) na base de dados SQL Server
   - Criar encriptação por campo para dados sensíveis como informações de pagamento
   - Desenvolver mascaramento dinâmico de dados para informações financeiras e pessoais

2. **Prevenção de vazamentos:**
   - Utilizar exclusivamente Entity Framework Core parametrizado para todas as consultas
   - Auditar queries SQL diretas e proibir uso de `FromSqlRaw` sem parametrização
   - Implementar filtragem automática de dados confidenciais em todos os repositórios

3. **Controlo de acesso à base de dados:**
   - Implementar princípio de privilégio mínimo para a conta de acesso à base de dados
   - Criar utilizador dedicado de aplicação com permissões restritas no SQL Server
   - Segmentar acesso a dados através de views e procedimentos armazenados específicos

**Risco: Tampering (7.0 - ALTO)**
1. **Integridade da base de dados:**
   - Implementar verificações de integridade referencial para todas as entidades relacionadas
   - Configurar triggers SQL para auditar modificações em tabelas críticas
   - Utilizar transações atómicas via Unit of Work para operações que afetam múltiplas tabelas

2. **Prevenção de injeção SQL:**
   - Auditar e modificar todas as queries dinâmicas para utilizar parâmetros
   - Implementar sanitização de entradas para todos os parâmetros usados em consultas
   - Utilizar ORMs com parametrização automática (Entity Framework Core)

3. **Proteção contra modificações não autorizadas:**
   - Configurar backups diferenciais frequentes e testes de restauro
   - Implementar monitoramento de alterações em tabelas críticas
   - Criar alertas para padrões anormais de modificações na base de dados

#### Processos 2.1-2.3: Fluxos de Dados Críticos

**Risco: Information Disclosure (8.2 - CRÍTICO para Fluxo 2.1, 6.0 - ALTO para outros)**
1. **Comunicações seguras:**
   - Configurar HTTPS obrigatório com HSTS no ASP.NET Core
   - Implementar TLS 1.2+ como requisito mínimo para todas as comunicações
   - Adicionar cabeçalhos de segurança como CSP, X-Content-Type-Options, X-Frame-Options

2. **Proteção de dados em trânsito:**
   - Implementar encriptação adicional para dados financeiros sensíveis
   - Criar canais seguros para transmissão de tokens e credenciais
   - Utilizar certificados de cliente para comunicações críticas

**Risco: Tampering (7.4 - ALTO para Fluxo 2.1, 7.0 - ALTO para outros)**
1. **Integridade dos dados transmitidos:**
   - Implementar assinaturas digitais para verificar a integridade dos dados
   - Utilizar checksums para dados críticos como credenciais e informações financeiras
   - Validar a integridade dos dados em cada camada da aplicação

2. **Proteção contra interceptação e modificação:**
   - Implementar certificate pinning para prevenir ataques MITM
   - Utilizar tokens anti-falsificação para todas as operações relevantes
   - Validar a origem e integridade das requisições em cada controlador

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