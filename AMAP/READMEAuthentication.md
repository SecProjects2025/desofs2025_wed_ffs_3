# Sistema de Repúdio - AMAP

Este documento descreve as funcionalidades do sistema de repúdio implementado no AMAP, garantindo autenticidade, não-repúdio e integridade das operações de subscrição e pagamento.

---

## 1. Repúdio: Negação de ter realizado subscrição

### 1.1 Criação de Assinatura

**Endpoint:**  
`POST /api/subscription`

**Descrição:**  
Cria uma nova assinatura, gerando um token de confirmação, enviando email ao co-produtor e inicializando campos de repúdio.

**Parâmetros:**  
```json
{
    "productOfferId": 1,
    "expectedQuantity": 5,
    "paymentType": "Money"
}
```

**Resposta:**  
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

---

### 1.2 Confirmação de Email

**Endpoint:**  
`POST /api/Subscription/confirm`

**Descrição:**  
Confirma o email do co-produtor através do token gerado na criação da assinatura.

**Como testar:**  
1. Copie o token da resposta da criação.
2. Faça POST para `/api/Subscription/confirm` com o token no corpo.

---

### 1.3 Aceitação de Termos e Condições

**Endpoint:**  
`POST /api/subscription/{id}/accept-terms`

**Descrição:**  
Permite ao co-produtor aceitar os termos, registando data/hora e assinatura digital.

---

### 1.4 Modelo de Dados: Subscription

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

---

### 1.5 Fluxo de Repúdio

1. Criação da assinatura (gera token, envia email, inicializa campos)
2. Confirmação de email (valida token, marca como confirmado)
3. Aceitação de termos (regista data/hora e assinatura digital)

---

## 2. Repúdio: Negação de ter realizado um pagamento

### 2.1 Criação de Pagamento

**Endpoint:**  
`POST /api/payment`

**Descrição:**  
Cria um pagamento, registando o método de pagamento (`Money`, `MBway`, `BankTransfer`) e inicializando o campo `Validated` como `false`.  
Campos `ValidationDate` e `ValidatedByProducerId` começam nulos.

---

### 2.2 Validação de Pagamento

**Endpoint:**  
`PUT /api/payment/{id}/validate`

**Descrição:**  
Permite a um Produtor validar um pagamento:
- Só pode ser feito por utilizadores com role `Producer`
- Marca `Validated` como `true`
- Regista data/hora da validação e o ID do Produtor

**Como testar:**  
1. Crie um pagamento (token de CoProducer)
2. Obtenha o ID do pagamento
3. Autentique-se como Producer
4. Faça PUT para `/api/payment/{id}/validate` com token de Producer

**Respostas possíveis:**  
- 200 OK: Pagamento validado
- 404 Not Found: Pagamento não encontrado
- 401/403: Não autorizado
- 400: Já validado

---

### 2.3 Modelo de Dados: Payment

```csharp
public class Payment
{
    public int Id { get; set; }
    public string? CoProducerId { get; set; }
    public int SubscriptionId { get; set; }
    public PaymentType PaymentMethod { get; set; }
    public bool Validated { get; set; }
    public DateTime? ValidationDate { get; set; }
    public string? ValidatedByProducerId { get; set; }
}
```

---

## 3. Integridade: Alteração não autorizada de registos de pagamentos

### 3.1 Histórico de Validações de Pagamento

**Entidade:**  
`PaymentValidationHistory`

**Descrição:**  
Regista cada evento de validação de pagamento:
- Uma entrada por validação bem-sucedida
- Inclui ID do pagamento, ID do Produtor e timestamp
- Garante trilha de auditoria e integridade

**Modelo:**  
```csharp
public class PaymentValidationHistory
{
    public int Id { get; set; }
    [Required]
    public int PaymentId { get; set; }
    [ForeignKey("PaymentId")]
    public Payment Payment { get; set; }
    [Required]
    public string? ProducerId { get; set; } // Stores the Producer's user ID (string GUID)
    [Required]
    public DateTime ValidationTimestamp { get; set; }
}
```

---

## 4. Segurança e Implementação

- Tokens de confirmação são únicos e de uso único
- Assinaturas digitais para aceitação de termos
- Datas/horas em UTC
- Validação e sanitização de campos sensíveis
- Entity Framework Core para persistência
- Migrações automáticas
- Tratamento de exceções e logs de auditoria

---

## 5. Configuração de Email

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


