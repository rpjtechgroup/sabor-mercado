# Feature Specification: E-mail, Google Sign-In e Suporte

**Feature Branch**: `009-email-google-support`  
**Created**: 2026-06-16  
**Status**: In progress

## User Scenarios

### US1 — Entrar com Google (P1)

Usuário autentica com botão oficial Google na página de conta; conta existente com mesmo e-mail é vinculada.

### US2 — Enviar feedback (P1)

Usuário reporta bug, sugestão, crítica ou pede suporte; e-mail chega em rpjtechgroup@gmail.com com diagnóstico do app.

### US3 — Feedback offline (P2)

Sem rede, o formulário informa que é necessário conexão; fluxo de compra não é afetado.

## Functional Requirements

- **FR-001**: `IEmailSender` + SMTP Gmail via MailKit; config `Email` em appsettings/secrets.
- **FR-002**: `POST /api/v1/auth/google` valida ID token GIS e emite JWT.
- **FR-003**: `UserAccount.GoogleSubjectId` opcional; vínculo automático por e-mail.
- **FR-004**: `POST /api/v1/feedback` com rate-limit; envia e-mail com diagnóstico.
- **FR-005**: PWA: `GoogleSignInButton`, `SupportFab` + `SupportDialog`.
- **FR-006**: Diagnóstico nunca inclui chave Gemini, senhas ou tokens.
