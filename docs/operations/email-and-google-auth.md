# E-mail e login Google — configuração operacional

Este guia cobre **como obter** a senha de app e o Client ID do Google e **onde colocar** cada valor (GitHub Secrets para deploy automático).

---

## Onde cada valor fica

| Valor | GitHub Secret (deploy automático) | Arquivo na VM (deploy manual) |
|-------|-----------------------------------|-------------------------------|
| Senha do PostgreSQL | `SABOR_DB_PASS` | `sabormercado.secrets` |
| Chave JWT da API | `SABOR_JWT_KEY` | `sabormercado.secrets` |
| Senha de app Gmail | `SABOR_SMTP_PASSWORD` | `sabormercado.secrets` |
| OAuth Client ID | `SABOR_GOOGLE_CLIENT_ID` | `sabormercado.secrets` |
| E-mail remetente (opcional) | `SABOR_SMTP_USER` | padrão `rpjtechgroup@gmail.com` |

**Deploy pelo GitHub Actions** (`push` em `main`): configure tudo em  
**Settings → Secrets and variables → Actions → Repository secrets**.

O workflow [`.github/workflows/deploy-main.yml`](../../.github/workflows/deploy-main.yml) envia esses secrets para a VM e grava `/home/ubuntu/deploy/sabormercado.secrets` a cada deploy.

**Deploy manual** (`scripts/deploy.ps1` na sua máquina): continua usando o arquivo `sabormercado.secrets` na VM (ou variáveis de ambiente equivalentes).

---

## Parte 1 — Senha de app Gmail (envio de e-mail)

Conta: **rpjtechgroup@gmail.com**

### Passo 1 — Ativar verificação em 2 etapas

1. Abra [myaccount.google.com](https://myaccount.google.com/) logado como `rpjtechgroup@gmail.com`.
2. Menu **Segurança** (ou **Security**).
3. Em **Como você faz login no Google**, clique em **Verificação em duas etapas**.
4. Siga o assistente até concluir (SMS ou app Authenticator).

Sem 2 etapas ativas, a opção **Senhas de app** não aparece.

### Passo 2 — Criar senha de app

1. Ainda em **Segurança**, procure **Senhas de app**  
   (link direto: [myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords)).
2. Se pedir a senha da conta de novo, informe.
3. Em **Selecionar o app**, escolha **Mail** (ou **Outro** e digite `Sabor Mercado`).
4. Em **Selecionar o dispositivo**, escolha **Outro** → nome `Sabor Mercado VM`.
5. Clique em **Gerar**.
6. O Google mostra **16 caracteres** (ex.: `abcd efgh ijkl mnop`). **Copie e guarde** — não dá para ver de novo.

### Passo 3 — Colocar no GitHub

1. Repositório no GitHub → **Settings** → **Secrets and variables** → **Actions**.
2. **New repository secret**
3. Nome: `SABOR_SMTP_PASSWORD`
4. Valor: a senha de 16 caracteres (**pode colar com ou sem espaços**).

(Opcional) Secret `SABOR_SMTP_USER` = `rpjtechgroup@gmail.com` — só se quiser outro remetente.

---

## Parte 2 — Client ID do Google (botão “Entrar com Google”)

### Passo 1 — Projeto no Google Cloud

1. Acesse [console.cloud.google.com](https://console.cloud.google.com/).
2. No topo, clique no seletor de projeto → **Novo projeto**.
3. Nome: `Sabor Mercado` → **Criar**.
4. Aguarde e **selecione** esse projeto.

### Passo 2 — Tela de consentimento OAuth

1. Menu ☰ → **APIs e serviços** → **Tela de consentimento OAuth**.
2. Tipo de usuário: **Externo** → **Criar**.
3. Preencha:
   - **Nome do app:** `Sabor Mercado`
   - **E-mail de suporte:** `rpjtechgroup@gmail.com`
   - **E-mail do desenvolvedor:** `rpjtechgroup@gmail.com`
4. **Salvar e continuar** nas etapas de escopo (os padrões `email` / `profile` / `openid` bastam para login).
5. Em **Usuários de teste** (enquanto o app estiver em “Teste”), adicione `rpjtechgroup@gmail.com` e qualquer outro e-mail que for testar login.
6. **Salvar** até concluir.

> Em modo **Teste**, só os e-mails listados em “Usuários de teste” conseguem entrar com Google. Para liberar para qualquer um, depois publique o app na mesma tela (verificação do Google pode ser necessária).

### Passo 3 — Credencial OAuth (Web)

1. Menu ☰ → **APIs e serviços** → **Credenciais**.
2. **+ Criar credenciais** → **ID do cliente OAuth**.
3. Se pedir, configure a tela de consentimento (passo 2).
4. **Tipo de aplicativo:** `Aplicativo da Web`.
5. **Nome:** `Sabor Mercado PWA`.
6. **Origens JavaScript autorizadas** — adicione **exatamente** (sem barra no final, sem `/mercado`):

   ```
   https://rpjtechgroup.ddns.net
   http://localhost:5052
   ```

7. **URIs de redirecionamento:** deixe **vazio** (o botão oficial GIS não usa redirect no nosso fluxo).
8. **Criar**.
9. Copie o **ID do cliente** (formato `123456789-xxxx.apps.googleusercontent.com`).

### Passo 4 — Colocar no GitHub

1. **New repository secret**
2. Nome: `SABOR_GOOGLE_CLIENT_ID`
3. Valor: o ID copiado acima.

---

## Parte 3 — Secrets já existentes (confira no GitHub)

O deploy automático precisa destes secrets (além dos dois novos):

| Secret | O que é |
|--------|---------|
| `VM_HOST` | IP da VM (`204.216.162.78`) |
| `VM_USER` | `ubuntu` |
| `VM_SSH_KEY` | Chave privada SSH (conteúdo do `.key`) |
| `SABOR_DB_PASS` | Senha do PostgreSQL na VM |
| `SABOR_JWT_KEY` | String longa (≥ 32 caracteres) para assinar JWT |

Se `SABOR_DB_PASS` e `SABOR_JWT_KEY` existirem **só** no arquivo da VM hoje, **cadastre-os também no GitHub** para o Actions não depender só do arquivo antigo.

---

## Parte 4 — Deploy

### Automático (recomendado)

1. Cadastre todos os secrets acima.
2. Faça `push` na branch `main` (ou rode o workflow **Deploy to VM** manualmente em Actions).

### Manual (sua máquina)

Edite `/home/ubuntu/deploy/sabormercado.secrets` na VM (modelo em [`sabormercado.secrets.example`](sabormercado.secrets.example)) e rode:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/deploy.ps1
```

---

## Parte 5 — Testar

1. **E-mail:** abra o app → botão **?** (Fale conosco) → envie uma mensagem de teste. Deve chegar em `rpjtechgroup@gmail.com`.
2. **Google:** abra `/mercado/conta` → botão **Sign in with Google**. Se o app OAuth estiver em “Teste”, use um e-mail cadastrado em usuários de teste.

### Erros comuns

| Sintoma | Causa provável |
|---------|----------------|
| Botão Google não aparece | `SABOR_GOOGLE_CLIENT_ID` vazio ou deploy sem atualizar `appsettings.json` do PWA |
| `origin_mismatch` no Google | Origem errada — use `https://rpjtechgroup.ddns.net` sem `/mercado` |
| Login Google “acesso bloqueado” | App em Teste e seu e-mail não está em usuários de teste |
| Feedback 503 | `SABOR_SMTP_PASSWORD` ausente ou senha de app inválida |
| `Invalid credentials` SMTP | 2 etapas desativadas ou senha de app revogada — gere outra |

---

## Notas de decisão (para consulta futura)

### Uma credencial OAuth para produção + debug

Foi orientado usar **um único** OAuth Client ID no Google Cloud com **duas origens JavaScript**:

| Origem | Uso |
|--------|-----|
| `https://rpjtechgroup.ddns.net` | Produção (app publicado em `/mercado/`) |
| `http://localhost:5052` | Desenvolvimento local (`dotnet run` no projeto Web) |

Isso **não** torna a credencial “só de debug”: é o padrão recomendado pelo Google para o mesmo app em prod e dev. A origem é o esquema + host + porta; o path `/mercado` **não** entra na origem.

Quando o app OAuth sair do modo **Teste** e for publicado, a mesma credencial continua válida em produção.

### `SABOR_DB_PASS`, `SABOR_JWT_KEY` e `SABOR_SMTP_USER`

| Secret | Precisa criar algo novo? |
|--------|--------------------------|
| `SABOR_DB_PASS` | **Não**, se o deploy em produção já funcionava. A senha do PostgreSQL foi definida no primeiro deploy na VM. Copie o **mesmo valor** que já está em `/home/ubuntu/deploy/sabormercado.secrets` para o GitHub Secret (uma vez), para o Actions não sobrescrever com vazio. |
| `SABOR_JWT_KEY` | **Não**, mesma lógica — já existia na VM. Copie para o GitHub se ainda não cadastrou. **Não troque** sem motivo (invalida tokens de usuários logados). |
| `SABOR_SMTP_USER` | **Não precisa fazer nada.** Opcional; se omitir, o deploy usa `rpjtechgroup@gmail.com` automaticamente. |

Resumo: para a feature 009 você **cria** `SABOR_SMTP_PASSWORD` e `SABOR_GOOGLE_CLIENT_ID`. Os outros dois (`DB_PASS`, `JWT_KEY`) são **reaproveitados** do que já rodava na VM → só espelhar no GitHub.

---

## Desenvolvimento local (opcional)

Crie arquivos **não commitados**:

`src/SaborMercado.Api/appsettings.Development.json`:

```json
{
  "Email": { "Password": "sua-senha-de-app" },
  "GoogleAuth": { "ClientId": "….apps.googleusercontent.com" }
}
```

`src/SaborMercado.Web/wwwroot/appsettings.Development.json`:

```json
{
  "GoogleClientId": "….apps.googleusercontent.com"
}
```
