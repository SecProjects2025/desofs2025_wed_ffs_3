# Sistema de Repúdio - AMAP

Este documento descreve as funcionalidades do sistema de repúdio implementado no AMAP, que garante a autenticidade e não-repúdio das assinaturas através de termos e condições e confirmação de email.

## Funcionalidades

### 1. Criação de Assinatura

#### Endpoint
```
POST /api/subscription
```

#### Descrição
Cria uma nova assinatura com suporte ao sistema de repúdio. O sistema:
- Gera um token único de confirmação
- Envia email de confirmação ao co-produtor
- Inicializa os campos de repúdio

#### Parâmetros
```json
{
    "productOfferId": 1,
    "expectedQuantity": 5,
    "paymentType": "Money"
}
```

#### Resposta
```json
{
    "id": 2,
    "coProducerId": "1cc4c021-19e7-49dd-aeb0-e31a8f382f66",
    "productOfferId": 1,
    "expectedQuantity": 5,
    "paymentType": "Money",
    "termsAccepted": false,
    "termsAcceptedAt": "2025-05-25T00:28:12.5111303Z",
    "emailConfirmed": false,
    "emailConfirmedAt": null,
    "confirmationToken": "vKeAQGXwMvpeim/crxxr5+oWy/Fl86BGETYfctNpLPQ=",
    "digitalSignature": "sCuRfy3NhmSc0bjjkCw9JKX/Xpl0INqPUex20s/iqi0="
}
```

### 2. Confirmação de Email

#### Endpoint
```
POST https://localhost:7262/api/Subscription/confirm
```

#### Descrição
Confirma o email do co-produtor através de um token único gerado durante a criação da assinatura. O sistema:
- Valida o token de confirmação
- Marca o email como confirmado
- Registra a data e hora da confirmação
- Remove o token após uso

#### Parâmetros
```json
"vKeAQGXwMvpeim/crxxr5+oWy/Fl86BGETYfctNpLPQ="
```

#### Teste
Para testar a confirmação:
1. Copie o token de confirmação da resposta da criação da assinatura
2. Faça uma requisição POST para `https://localhost:7262/api/Subscription/confirm`
3. Cole o token como corpo da requisição (apenas o token, sem aspas)
4. O sistema retornará:
   - 200 OK: "Subscription confirmed successfully"
   - 404 Not Found: "Invalid confirmation token"
   - 500 Internal Server Error: "An error occurred while confirming the subscription"

### 3. Aceitação de Termos e Condições

#### Endpoint
```
POST /api/subscription/{id}/accept-terms
```

#### Descrição
Permite que um co-produtor aceite os termos e condições de uma assinatura. O sistema registra:
- Data e hora da aceitação
- Assinatura digital do co-produtor
- Status de aceitação

#### Parâmetros
- `id`: ID da assinatura

#### Resposta
- 200 OK: "Terms accepted successfully"
- 404 Not Found: "Subscription not found"
- 500 Internal Server Error: "An error occurred while accepting the terms"

## Modelo de Dados

### Subscription
```csharp
public class Subscription
{
    public int Id { get; set; }
    public string? CoProducerId { get; set; }
    public int ProductOfferId { get; set; }
    public int ExpectedQuantity { get; set; }
    public PaymentType PaymentType { get; set; }
    
    [Required]
    public bool TermsAccepted { get; set; }
    
    [Required]
    public DateTime TermsAcceptedAt { get; set; }
    
    [Required]
    public bool EmailConfirmed { get; set; }
    
    public DateTime? EmailConfirmedAt { get; set; }
    
    public string? ConfirmationToken { get; set; }
    
    public string? DigitalSignature { get; set; }
}
```

## Fluxo de Repúdio

1. **Criação da Assinatura**
   - Sistema gera token único
   - Envia email de confirmação
   - Inicializa campos de repúdio como false

2. **Confirmação de Email**
   - Co-produtor recebe email com link de confirmação
   - Sistema valida token e marca email como confirmado
   - Token é removido após uso

3. **Aceitação de Termos**
   - Co-produtor aceita termos e condições
   - Sistema registra data/hora e assinatura digital
   - Assinatura é marcada como completa

## Configuração de Email

O sistema utiliza o Mailtrap para envio de emails em ambiente de desenvolvimento:

```json
"MailSettings": {
    "Host": "smtp.mailtrap.io",
    "Port": 2525,
    "Username": "392da75e956865",
    "Password": "sua-senha",
    "FromEmail": "noreply@amap.com",
    "FromName": "AMAP Notifications"
}
```

## Segurança

- Tokens de confirmação são únicos e de uso único
- Assinaturas digitais são geradas para cada aceitação de termos
- Datas e horários são registrados em UTC
- Campos sensíveis são validados e sanitizados

## Notas de Implementação

- O sistema utiliza Entity Framework Core para persistência
- Migrações automáticas para atualização da base de dados
- Tratamento de exceções em todos os endpoints
- Logs de auditoria para ações importantes 