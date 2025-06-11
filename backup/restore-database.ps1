# Script para restauro automático do banco de dados SQL Server
# Autor: Equipa AMAP
# Data: 2024

param(
    [Parameter(Mandatory = $true)]
    [string]$BackupFile, # Caminho completo do arquivo de backup
    
    [Parameter(Mandatory = $false)]
    [string]$ServerName = "DESKTOP-37QTCQN\SQLEXPRESS", # Nome do servidor SQL
    
    [Parameter(Mandatory = $false)]
    [string]$DatabaseName = "AMAP_DB"  # Nome do banco de dados
)

# Verificar se o arquivo de backup existe
if (-not (Test-Path $BackupFile)) {
    Write-Error "Arquivo de backup não encontrado: $BackupFile"
    exit 1
}

# Comando SQL para restauro
$restoreQuery = @"
USE [master]
GO

-- Matar todas as conexões com o banco de dados
ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
GO

-- Restaurar o banco de dados
RESTORE DATABASE [$DatabaseName] 
FROM DISK = N'$BackupFile' 
WITH REPLACE, 
     RECOVERY,
     STATS = 10
GO

-- Voltar para modo multi-usuário
ALTER DATABASE [$DatabaseName] SET MULTI_USER
GO
"@

try {
    # Executar restauro
    Write-Host "Iniciando restauro do banco de dados $DatabaseName..."
    Write-Host "Usando arquivo de backup: $BackupFile"
    
    Invoke-Sqlcmd -ServerInstance $ServerName -Query $restoreQuery -TrustServerCertificate
    Write-Host "Restauro concluído com sucesso!"

    # Registrar o restauro em um arquivo de log
    $logFile = "C:\backup\AMAP\restore_log.txt"
    $logEntry = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Restauro realizado: $BackupFile"
    Add-Content -Path $logFile -Value $logEntry
}
catch {
    Write-Error "Erro durante o restauro: $_"
    exit 1
}

Write-Host "Processo de restauro finalizado com sucesso!" 