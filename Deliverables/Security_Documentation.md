# Documentação de Segurança e Integridade de Dados - AMAP

## 1. Visão Geral
O sistema AMAP implementa um conjunto abrangente de medidas de segurança e integridade de dados para proteger informações sensíveis de produtos e usuários. Esta documentação detalha as implementações de segurança em três áreas principais:
- Proteção contra alterações não autorizadas de produtos
- Proteção contra modificações não autorizadas de dados de usuários
- Proteção contra manipulação não autorizada de dados entre aplicação e base de dados

## 2. Arquitetura de Segurança

### 2.1 Camadas de Segurança
O sistema implementa segurança em múltiplas camadas:
1. **Camada de Apresentação (Controllers)**
   - Autorização baseada em roles
   - Validação de inputs
   - Sanitização de dados

2. **Camada de Negócio (Business Components)**
   - Validações de regras de negócio
   - Verificações de propriedade
   - Logging de operações sensíveis

3. **Camada de Dados (Repositories)**
   - Consultas parametrizadas
   - Validações de integridade
   - Princípio do menor privilégio

### 2.2 Componentes Principais
- `SecurityMonitoringService`: Monitoramento de atividades de segurança
- `EmailSender`: Sistema de notificações por email
- `ValidationService`: Serviço centralizado de validação
- `AccountController`: Gerenciamento de autenticação e autorização

## 3. Implementações de Segurança

### 3.1 Proteção de Produtos
#### 3.1.1 Controle de Acesso
```csharp
[Authorize(Roles = nameof(UserType.Producer))]
public class ProductController : BaseController
{
    // Implementação do controle de acesso baseado em roles
}
```

#### 3.1.2 Validação de Dados
```csharp
public class Product
{
    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(100, ErrorMessage = "Product name cannot be longer than 100 characters.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Reference price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Reference price must be greater than zero.")]
    public double ReferencePrice { get; set; }
}
```

#### 3.1.3 Verificação de Propriedade
```csharp
public void UpdateProduct(int id, Product product, string producerId)
{
    if (string.IsNullOrEmpty(producerId))
        throw new ArgumentException("Producer not authenticated.");

    var existingProduct = _unitOfWork.Product.Get(
        p => p.Id == id && p.ProducerId == producerId && !p.IsDeleted
    );

    if (existingProduct == null)
        throw new KeyNotFoundException("Product not found or you are not authorized to update it.");
}
```

### 3.2 Proteção de Dados de Utilizadores
#### 3.2.1 Autenticação e Autorização
- Implementação de MFA para utilizadores Manager
- Sistema de roles (Producer, Manager, etc.)
- Verificação de identidade em operações sensíveis

#### 3.2.2 Notificações de Segurança
```csharp
private async Task SendSecurityAlertAsync(AppUser user, string alertType)
{
    var subject = $"AMAP Security Alert: {alertType}";
    var body = $@"
        <h2>Security Alert</h2>
        <p>Dear {user.Name},</p>
        <p>A security event has been detected on your account.</p>
        <p><strong>Details:</strong></p>
        <ul>
            <li>Type: {alertType}</li>
            <li>Time: {DateTime.UtcNow}</li>
        </ul>
    ";
    await _emailSender.SendEmailAsync(user.Email, subject, body);
}
```

#### 3.2.3 Histórico de Alterações
```csharp
public class AuthActivity
{
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public AuthActivityType ActivityType { get; set; }
}
```

### 3.3 Proteção de Dados na Persistência
#### 3.3.1 Consultas Parametrizadas
```csharp
var product = _unitOfWork.Product.Get(
    p => p.Id == id && !p.IsDeleted,
    "Composites.SubProduct"
);
```

#### 3.3.2 Validações de Integridade
```csharp
if (productOffer.SubscriptionPeriod?.EndDate < DateTime.Now)
{
    throw new InvalidOperationException("Subscription period has ended.");
}
```

## 4. Logging e Monitoramento

### 4.1 Sistema de Logging
```csharp
_logger.LogInformation("Product created - ID: {ProductId}, Name: {ProductName}, Producer: {ProducerId} at {Time}",
    product.Id, product.Name, producerId, DateTime.UtcNow);
```

### 4.2 Monitoramento de Segurança
- Rastreamento de atividades de autenticação
- Deteção de atividades suspeitas
- Limpeza automática de dados antigos

## 5. Boas Práticas Implementadas

### 5.1 Validação de Dados
- Validações em múltiplas camadas
- Sanitização de inputs
- Verificação de tipos de dados

### 5.2 Controle de Acesso
- Princípio do menor privilégio
- Verificação de propriedade
- Roles e permissões granulares

### 5.3 Segurança de Dados
- Tokens seguros
- Assinaturas digitais
- Criptografia de dados sensíveis

## 6. Recomendações de Manutenção

### 6.1 Atualizações de Segurança
- Manter todas as dependências atualizadas
- Revisar regularmente as permissões de utilizadores
- Monitorar logs de segurança

### 6.2 Monitoramento
- Revisar logs de segurança diariamente
- Investigar atividades suspeitas
- Manter backup dos logs de segurança

## 7. Requisitos de Segurança Implementados

### 7.1 Alteração não autorizada de informações de produtos (1.3 T)
- ✅ Controlo de acesso baseado em funções
- ✅ Validação rigorosa de inputs
- ✅ Notificação de alterações
- ✅ Verificação de propriedade

### 7.2 Modificação não autorizada de dados de utilizador (3.1 T)
- ✅ Verificação de identidade antes de alterações
- ✅ Notificação por email de alterações
- ✅ Histórico de alterações
- ✅ Validação rigorosa de inputs
- ✅ Controlo de acesso granular

### 7.3 Manipulação não autorizada de dados entre aplicação e base de dados (5.1 T)
- ✅ Consultas parametrizadas (Entity Framework)
- ✅ Validação de dados antes da persistência
- ✅ Princípio do menor privilégio
- ✅ Sanitização de inputs
- ✅ Verificações de integridade 