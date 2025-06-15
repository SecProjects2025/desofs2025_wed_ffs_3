# AMAP - Backend Setup Guide

Bem-vindo ao repositório do sistema AMAP! Este guia irá ajudá-lo a configurar o projeto localmente, incluindo a base de dados e o ambiente necessário para rodar a aplicação. Além disso, este arquivo contém a estrutura do repositório.

---

## **Estrutura do Repositório**

O repositório está organizado da seguinte forma:

- **`amap-ui/`**: Contém o frontend do sistema AMAP (não utilizado nesta cadeira).  
- **`AMAP/`**: Contém o backend do sistema, incluindo toda a lógica de negócio e integração com a base de dados.  
- **`Derivables/`**: Pasta com entregáveis organizados por fases:
  - **`Fase1/`**:  
    - Arquivos `.json` dos DFDs (Diagramas de Fluxo de Dados), que podem ser abertos no programa **Threat Dragon**.  
    - Checklist do **ASVS** com os requisitos de segurança.  
    - Relatório (`README`) em formato **Markdown** e **PDF** referente à Fase 1.  
  - **`Fase2 Sprint1/`**:
    - `dependency-check-report.html`: Relatório de verificação de vulnerabilidades nas                 dependências.
    - `coverage-report.html`: Relatório de cobertura de testes.
    - `README.md`: Relatório técnico em formato Markdown.
    - `README.pdf`: Relatório técnico em formato PDF.
    - `v4-ASVS-checklist-en.xlsx`: Checklist atualizada com os requisitos ASVS.
  - **`Fase2 Sprint2/`**: (ainda vazia).  
- **`Imagens/`**: Contém todas as imagens utilizadas no relatório.
- backups/: Contém scripts para backup, restauro e agendamento automático da base de dados do projeto.
  - backup-database.ps1: Script PowerShell para realizar backups automáticos da base de dados SQL Server.
  - restore-database.ps1: Script PowerShell para restaurar a base de dados a partir de um ficheiro de backup.
  - schedule-backup.ps1: Script PowerShell para agendar a execução automática do backup diário da base de dados.
- Dockerfile: Script que define como construir a imagem Docker da aplicação.
- docker-compose.development.yml: Configuração Docker Compose para ambiente de desenvolvimento.
- docker-compose.production.yml: Configuração Docker Compose para ambiente de produção.
- docker-compose.staging.yml: Configuração Docker Compose para ambiente de staging (pré-produção).
- **`README.md`**: Este arquivo, que contém a estrutura do repositório e o passo a passo para configurar o backend.

---

## **Pré-requisitos**

Antes de começar, certifique-se de que tem os seguintes softwares instalados no seu computador:

1. **Visual Studio 2022** com os seguintes workloads:
   - ASP.NET and web development
   - .NET desktop development
2. **SQL Server** e **SQL Server Management Studio (SSMS)**  
   - [Tutorial para instalação do SQL Server e SSMS](https://www.youtube.com/watch?v=8vTCyhDyRjg&list=PL82C6-O4XrHfZoh2ZH7-HCPyh9oHeYPnz)
3. **.NET 6 SDK** (ou superior)
4. **Git** para clonar o repositório.

---

## **Passo a Passo para Configuração**

### **1. Clone o Repositório**
Abra o terminal ou Git Bash e execute o seguinte comando para clonar o repositório:

```bash
git clone https://github.com/seu-usuario/desofs2025_wed_ffs_3.git
```

Entre na pasta do repositório:

```bash
cd desofs2025_wed_ffs_3
```

Abra a pasta do backend:

```bash
cd AMAP
```

Abra o arquivo de solução no Visual Studio 2022:

```bash
AMAP.sln
```

> **Nota:** O comando acima abrirá automaticamente o projeto no Visual Studio 2022.

---

### **2. Configuração da Base de Dados**

#### **2.1 Criação da Base de Dados**
1. Abra o **SQL Server Management Studio (SSMS)** e conecte-se ao servidor local.
2. Crie uma nova base de dados com o nome `AMAP_DB`:
   - Clique com o botão direito em **Databases** → **New Database**.
   - Insira o nome `AMAP_DB` e clique em **OK**.

> **Nota:** Não é necessário criar tabelas manualmente, elas serão geradas automaticamente.

---

#### **2.2 Configuração da Connection String**
1. No Visual Studio, abra o arquivo `appsettings.json` localizado na pasta `Services`.
2. Substitua a connection string existente pela sua. Exemplo:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=NOME_DO_PC;Database=AMAP_DB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

> **Nota:** Comente as connection strings existentes (se necessário) e insira a sua. Certifique-se de que o nome da base de dados seja `AMAP_DB`.

---

#### **2.3 Configuração das Migrações**
1. Navegue até a pasta `Data` e localize a subpasta `Migrations`.
2. **Apague o arquivo existente** dentro da pasta `Migrations` (se houver).
3. Abra o **Package Manager Console** no Visual Studio:
   - Vá para **Tools** → **NuGet Package Manager** → **Package Manager Console**.
4. Certifique-se de que o **Default Project** no Package Manager Console está definido como `AMAP.DataAccess`.  
   - Caso contrário, selecione-o no menu suspenso.

5. Execute o seguinte comando para criar uma nova migração:

```bash
Add-Migration InitialMigration
```

5. Aplique as migrações à base de dados com o comando:

```bash
Update-Database
```

> **Nota:** Este comando criará automaticamente as tabelas na base de dados.

---

### **3. Seed dos Dados**
Ao iniciar o projeto, os dados iniciais (como utilizadores) serão automaticamente inseridos na base de dados.

---

### **4. Executar o Projeto**
1. No Visual Studio, defina o projeto `AMAP.Api` como **Startup Project**:
   - Clique com o botão direito no projeto → **Set as Startup Project**.
2. Pressione `F5` ou clique em **Run** para iniciar o servidor.

O backend estará disponível em: `https://localhost:7262/swagger/index.html`.

---

## **Funcionalidades Principais**

- **Gestão de períodos de subscrição**
- **Catálogo de produtos**
- **Gestão de ofertas**
- **Gestão de subscrições**
- **Processamento de pagamentos**
- **Notificações por e-mail**
- **Controlo de acesso baseado em perfis**

---

## **Problemas Comuns**

### **Erro ao Conectar à Base de Dados**
- Verifique se a connection string no `appsettings.json` está correta.
- Certifique-se de que o SQL Server está em execução.

### **Erro ao Aplicar Migrações**
- Certifique-se de que a base de dados `AMAP_DB` foi criada.
- Apague a pasta `Migrations` e recrie a migração.

## **Licença**
Este projeto está licenciado sob a licença MIT. Consulte o arquivo `LICENSE` para mais detalhes.

---

## **Autores**

- **Diana Marques** 
- **Diogo Costa** 
- **Pedro Costa**
- **Pedro Vilarinho** 
- **Tiago Tavares** 

---
