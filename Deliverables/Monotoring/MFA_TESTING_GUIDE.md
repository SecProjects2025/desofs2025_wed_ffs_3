# Guia de Testes MFA

Este guia explica como testar o fluxo de Autenticação Multi-Fator (MFA) para gestores usando o Postman ou outro cliente de API.

## 1. Configuração MFA (Gerar QR Code/Segredo)

**Pedido:**
```
POST /api/auth/mfa/setup
Cabeçalhos:
  Authorization: Bearer <token_gestor>
Corpo (JSON):
  {
    "userId": "<id_utilizador_gestor>"
  }
```
**Resposta:**
- Devolve um URL para um QR code (abrir no browser) ou uma chave secreta para usar numa app autenticadora (ex: Google Authenticator).

## 2. Ler o QR Code ou Introduzir o Segredo
- Usa o URL do QR code ou a chave secreta na tua app autenticadora para adicionar a conta.

## 3. Ativar MFA (Verificar e Ativar MFA)

**Pedido:**
```
POST /api/auth/mfa/enable
Cabeçalhos:
  Authorization: Bearer <token_gestor>
Corpo (JSON):
  {
    "userId": "<id_utilizador_gestor>",
    "code": "<código de 6 dígitos da app>"
  }
```
**Resposta:**
- Sucesso se o código for válido e o MFA for ativado para o utilizador.

## 4. Verificação MFA Durante o Login

**Pedido:**
```
POST /api/auth/mfa/verify
Corpo (JSON):
  {
    "userId": "<id_utilizador_gestor>",
    "code": "<código de 6 dígitos da app>"
  }
```
**Resposta:**
- Sucesso se o código for válido e o login for concluído.

## Notas
- Substitui `<token_gestor>` e `<id_utilizador_gestor>` pelos valores reais do teu ambiente.
- Os códigos são baseados no tempo e mudam a cada 30 segundos na app autenticadora.
- Se tiveres problemas, garante que a hora do sistema está correta tanto no servidor como no teu dispositivo. 