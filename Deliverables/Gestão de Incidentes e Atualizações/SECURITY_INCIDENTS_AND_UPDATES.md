# Gestão de Incidentes e Atualizações

## 1. Gestão de Incidentes

### 1.1. Identificação de Incidentes
- Um incidente é qualquer evento que comprometa a segurança, disponibilidade, integridade ou confidencialidade do sistema.
- Exemplos: falhas de segurança, acessos não autorizados, indisponibilidade do serviço, perda de dados.

### 1.2. Reporte de Incidentes
- Qualquer membro da equipa deve reportar imediatamente incidentes ao responsável de segurança (ou equipa designada).
- O reporte deve incluir:
  - Data e hora do incidente
  - Descrição detalhada do evento
  - Passos para reproduzir (se aplicável)
  - Registos relevantes e capturas de ecrã

### 1.3. Resposta a Incidentes
- O responsável deve:
  1. Avaliar a gravidade do incidente.
  2. Isolar os sistemas afetados, se necessário.
  3. Recolher evidências (registos, dumps, etc.).
  4. Comunicar o incidente à gestão e, se necessário, aos utilizadores afetados.
  5. Corrigir a vulnerabilidade ou restaurar o serviço.
  6. Documentar todas as ações tomadas.

### 1.4. Pós-Incidente
- Realizar uma análise pós-incidente para identificar causas e melhorar processos.
- Atualizar a documentação e procedimentos, se necessário.

---

## 2. Gestão de Atualizações e Patches

### 2.1. Atualizações de Software
- Verificar semanalmente/mensalmente por atualizações de:
  - Framework (.NET)
  - Bibliotecas NuGet
  - Dependências externas (ex: bases de dados, servidores)
- Utilizar ferramentas como:
  - `dotnet list package --outdated`
  - `dotnet list package --vulnerable`
  - Dependabot (GitHub)

### 2.2. Aplicação de Patches
- Antes de aplicar, testar as atualizações num ambiente de testes (staging).
- Aplicar patches críticos imediatamente após validação.
- Documentar todas as atualizações aplicadas (data, versão, motivo).

### 2.3. Comunicação
- Informar a equipa sobre atualizações relevantes.
- Se uma atualização afetar utilizadores, comunicar de forma clara e antecipada.

---

## 3. Registo e Documentação

- Manter um registo de todos os incidentes e atualizações aplicadas.
- Exemplo de registo (pode ser um ficheiro CSV, Excel ou Markdown):

| Data       | Tipo        | Descrição                         | Responsável   | Estado    |
|------------|-------------|-----------------------------------|---------------|-----------|
| 2024-06-10 | Incidente   | Falha de login para gestores      | Pedro Vilar   | Resolvido |
| 2024-06-12 | Atualização | Atualização do pacote xunit 2.9.3 | Pedro Costa   | Feito     |

---
