# Sistema de Backup e Restauro - AMAP

Este diretório contém scripts para automatizar o processo de backup e restauro da base de dados SQL Server do projeto AMAP.

## Requisitos

- PowerShell 5.1 ou superior
- SQL Server Express instalado
- Módulo SqlServer do PowerShell instalado (instalar via `Install-Module -Name SqlServer`)

## Scripts Disponíveis

### 1. Backup do Banco de Dados (`backup-database.ps1`)

Este script realiza o backup completo da base de dados AMAP_DB.

#### Configurações
- Servidor SQL: DESKTOP-37QTCQN\SQLEXPRESS (ou alterar conforme necessário)
- Nome da base de dados: AMAP_DB
- Diretório de backup: C:\backup\AMAP
- Retenção de backups: 7 dias

#### Como Executar
```powershell
.\backup-database.ps1
```

#### Funcionalidades
- Cria backup completo da base de dados
- Gera nome de arquivo com timestamp
- Remove backups antigos automaticamente
- Mantém log de todas as operações
- Tratamento de erros

### 2. Restauro da Base de Dados (`restore-database.ps1`)

Este script permite restaurar a base de dados a partir de um arquivo de backup.

#### Como Executar
```powershell
.\restore-database.ps1 -BackupFile "C:\backup\AMAP\AMAP_20240315_120000.bak"
```

Parâmetros opcionais:
- `-ServerName`: Nome do servidor SQL
- `-DatabaseName`: Nome da base de dados

#### Funcionalidades
- Restaura backup completo da base de dados
- Elimina conexões existentes antes do restauro
- Retorna ao modo multi-utilizador após o restauro
- Mantém log de todas as operações
- Tratamento de erros

## Agendamento de Backups

Para automatizar os backups, podemos usar o Agendador de Tarefas do Windows:

1. Abra o Agendador de Tarefas
2. Crie uma nova tarefa básica
3. Configure para executar diariamente
4. Ação: Iniciar um programa
5. Programa/script: `powershell.exe`
6. Argumentos: `-ExecutionPolicy Bypass -File "C:\caminho\para\backup-database.ps1"`

## Logs

Os scripts mantêm dois arquivos de log:
- `backup_log.txt`: Registra todos os backups realizados
- `restore_log.txt`: Registra todos os restauros realizados

## Boas Práticas

1. Sempre teste os backups realizando um restauro em um ambiente de teste
2. Mantenha cópias dos backups em local diferente do servidor
3. Verifique regularmente os logs para garantir que os backups estão a ser realizados
4. Ajuste o período de retenção conforme necessário
5. Mantenha documentação atualizada sobre o processo de backup/restauro

## Suporte

Em caso de problemas:
1. Verifique os logs em `C:\backup\AMAP\`
2. Certifique-se que o SQL Server está a correr
3. Verifique as permissões do utilizador que executa os scripts
4. Consulte a documentação do SQL Server para mais detalhes 