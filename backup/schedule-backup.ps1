# Script para agendar backups do banco de dados
# Autor: Equipa AMAP
# Data: 2025

# Obter o diretório atual do script
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path

# Nome da tarefa agendada
$taskName = "AMAP_Database_Backup"

# Criar a ação (executar o script de backup)
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
    -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$scriptPath\backup-database.ps1`""

# Criar o trigger (executar diariamente às 2:00 AM)
$trigger = New-ScheduledTaskTrigger -Daily -At 2AM

# Configurar as configurações da tarefa
$settings = New-ScheduledTaskSettingsSet `
    -StartWhenAvailable `
    -DontStopOnIdleEnd `
    -RestartInterval (New-TimeSpan -Minutes 1) `
    -RestartCount 3

# Registrar a tarefa (requer privilégios de administrador)
try {
    Register-ScheduledTask -TaskName $taskName `
        -Action $action `
        -Trigger $trigger `
        -Settings $settings `
        -Description "Backup diário automático do banco de dados AMAP" `
        -Force

    Write-Host "Tarefa agendada criada com sucesso!"
    Write-Host "A tarefa '$taskName' será executada diariamente às 2:00 AM"
}
catch {
    Write-Error "Erro ao criar tarefa agendada: $_"
    Write-Host "`nPara criar a tarefa manualmente:"
    Write-Host "1. Abra o 'Agendador de Tarefas' (Task Scheduler)"
    Write-Host "2. Clique em 'Criar Tarefa Básica'"
    Write-Host "3. Nome: AMAP_Database_Backup"
    Write-Host "4. Trigger: Diário às 2:00 AM"
    Write-Host "5. Ação: Iniciar um programa"
    Write-Host "6. Programa: PowerShell.exe"
    Write-Host "7. Argumentos: -NoProfile -ExecutionPolicy Bypass -File `"$scriptPath\backup-database.ps1`""
    Write-Host "8. Marque 'Abrir as Propriedades avançadas'"
    Write-Host "9. Na aba 'Configurações', marque:"
    Write-Host "   - Executar tarefa o mais rápido possível após um início agendado falhar"
    Write-Host "   - Se a tarefa falhar, reiniciar a cada: 1 minuto"
    Write-Host "   - Tentar novamente até: 3 vezes"
} 