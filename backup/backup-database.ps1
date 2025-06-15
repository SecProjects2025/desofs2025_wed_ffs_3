# Script para backup automático do banco de dados SQL Server
# Autor: Equipa AMAP
# Data: 2025

# Configurações
$serverName = "DESKTOP-37QTCQN\SQLEXPRESS"  # Servidor SQL Express
$databaseName = "AMAP_DB"     # Nome do banco de dados
$backupPath = "C:\backup\AMAP"  # Caminho onde os backups serão armazenados
$retentionDays = 7        # Número de dias para manter os backups

# Criar diretório de backup se não existir
if (-not (Test-Path $backupPath)) {
    New-Item -ItemType Directory -Path $backupPath -Force
}

# Gerar nome do arquivo de backup com timestamp
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFile = "$backupPath\${databaseName}_$timestamp.bak"

# Comando SQL para backup
$backupQuery = @"
BACKUP DATABASE [$databaseName] 
TO DISK = N'$backupFile' 
WITH NOFORMAT, 
     NOINIT,  
     NAME = N'$databaseName-Full Database Backup', 
     SKIP, 
     NOREWIND, 
     NOUNLOAD,  
     STATS = 10
"@

try {
    # Executar backup
    Write-Host "Iniciando backup do banco de dados $databaseName..."
    Invoke-Sqlcmd -ServerInstance $serverName -Query $backupQuery -TrustServerCertificate
    Write-Host "Backup concluído com sucesso: $backupFile"

    # Limpar backups antigos
    $cutoffDate = (Get-Date).AddDays(-$retentionDays)
    Get-ChildItem -Path $backupPath -Filter "*.bak" | 
    Where-Object { $_.LastWriteTime -lt $cutoffDate } | 
    ForEach-Object {
        Write-Host "Removendo backup antigo: $($_.FullName)"
        Remove-Item $_.FullName -Force
    }
}
catch {
    Write-Error "Erro durante o backup: $_"
    exit 1
}

# Registrar o backup em um arquivo de log
$logFile = "$backupPath\backup_log.txt"
$logEntry = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Backup realizado: $backupFile"
Add-Content -Path $logFile -Value $logEntry

Write-Host "Processo de backup finalizado com sucesso!" 