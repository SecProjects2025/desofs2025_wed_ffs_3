using AMAP.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace AMAP.Entities.Utilities
{
    public class SecurityMonitoringService
    {
        private readonly ILogger<SecurityMonitoringService> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly EmailSender _emailSender;
        
        private static readonly ConcurrentDictionary<string, List<AuthActivity>> _userActivities = 
            new ConcurrentDictionary<string, List<AuthActivity>>();
        
        // Limites para detecção de padrões anormais
        private const int MaxSessionsPerUser = 5;
        private const int MaxLoginAttemptsPerHour = 10;
        private const int UnusualLocationAlertThreshold = 2; // Número de IPs diferentes para gerar alerta
        private const int SuspiciousTimeWindowMinutes = 60;

        public SecurityMonitoringService(
            ILogger<SecurityMonitoringService> logger,
            UserManager<AppUser> userManager,
            EmailSender emailSender)
        {
            _logger = logger;
            _userManager = userManager;
            _emailSender = emailSender;
        }


        public async Task RecordAuthActivityAsync(string userId, string ipAddress, string userAgent, AuthActivityType activityType)
        {
            if (string.IsNullOrEmpty(userId))
                return;

            var activity = new AuthActivity
            {
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                ActivityType = activityType
            };

            // Adiciona a atividade ao histórico
            _userActivities.AddOrUpdate(
                userId,
                new List<AuthActivity> { activity },
                (key, existingList) =>
                {
                    existingList.Add(activity);
                    return existingList;
                });

            await DetectSuspiciousPatterns(userId);
        }

        private async Task DetectSuspiciousPatterns(string userId)
        {
            if (!_userActivities.TryGetValue(userId, out var activities))
                return;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return;

            var recentActivities = activities
                .Where(a => a.Timestamp > DateTime.UtcNow.AddMinutes(-SuspiciousTimeWindowMinutes))
                .ToList();

            // 1. Verificar múltiplos logins num curto período
            var recentLogins = recentActivities
                .Where(a => a.ActivityType == AuthActivityType.Login)
                .ToList();

            if (recentLogins.Count > MaxLoginAttemptsPerHour)
            {
                _logger.LogWarning("Excessive login activity detected for user {UserId}: {Count} logins in the last hour", 
                    userId, recentLogins.Count);
                await SendSecurityAlertAsync(user, "Atividade de login excessiva", 
                    $"Detectamos {recentLogins.Count} logins na sua conta na última hora.");
            }

            // 2. Verificar logins de múltiplas localizações (IPs diferentes)
            var uniqueIPs = recentLogins
                .Select(a => a.IpAddress)
                .Distinct()
                .ToList();

            if (uniqueIPs.Count >= UnusualLocationAlertThreshold)
            {
                _logger.LogWarning("Multiple IP login detected for user {UserId}: {Count} different IPs", 
                    userId, uniqueIPs.Count);
                await SendSecurityAlertAsync(user, "Logins de múltiplas localizações", 
                    $"Detectamos logins na sua conta a partir de {uniqueIPs.Count} endereços IP diferentes na última hora.");
            }

            // 3. Verificar sessões simultâneas ativas
            var activeSessions = activities
                .Where(a => a.ActivityType == AuthActivityType.Login)
                .Except(activities.Where(a => a.ActivityType == AuthActivityType.Logout))
                .ToList();

            if (activeSessions.Count > MaxSessionsPerUser)
            {
                _logger.LogWarning("Excessive active sessions detected for user {UserId}: {Count} active sessions", 
                    userId, activeSessions.Count);
                await SendSecurityAlertAsync(user, "Múltiplas sessões ativas", 
                    $"Detectamos {activeSessions.Count} sessões ativas na sua conta.");
            }
        }


        private async Task SendSecurityAlertAsync(AppUser user, string alertType, string message)
        {
            try
            {
                var subject = $"AMAP Alerta de Segurança: {alertType}";
                var body = $@"
                    <h2>Alerta de Segurança</h2>
                    <p>Caro(a) {user.Name},</p>
                    <p>{message}</p>
                    <p>Se esta atividade não foi realizada por si, recomendamos que altere imediatamente a sua senha e contacte o suporte.</p>
                    <p>Obrigado por ajudar a manter a sua conta segura.</p>
                    <p>Equipe de Segurança AMAP</p>
                ";

                await _emailSender.SendEmailAsync(user.Email, subject, body);
                _logger.LogInformation("Security alert email sent to {Email}: {AlertType}", user.Email, alertType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send security alert email to {Email}", user.Email);
            }
        }

        // Limpa dados antigos de monitoramento
        public void CleanupOldData()
        {
            var cutoffTime = DateTime.UtcNow.AddDays(-7); 

            foreach (var userId in _userActivities.Keys)
            {
                if (_userActivities.TryGetValue(userId, out var activities))
                {
                    var filteredActivities = activities
                        .Where(a => a.Timestamp >= cutoffTime)
                        .ToList();

                    _userActivities.TryUpdate(userId, filteredActivities, activities);
                }
            }
        }
    }


    public enum AuthActivityType
    {
        Login,
        Logout,
        PasswordChange,
        FailedLogin,
        MfaSetup,
        MfaVerification
    }


    public class AuthActivity
    {
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public AuthActivityType ActivityType { get; set; }
    }
} 