# GestaoWeb

Sistema de gestão de tarefas para equipes financeiras. Desenvolvido em ASP.NET Core 8 MVC com SQL Server 2022.

## Funcionalidades

- Autenticação com email e senha
- Dois perfis: **gestor** e **colaborador**
- Gestores criam e acompanham tarefas; colaboradores atualizam o status das tarefas atribuídas
- Upload de foto de perfil
- Notificações por email (nova tarefa e conclusão)

## Pré-requisitos

| Ferramenta | Versão mínima | Download |
|---|---|---|
| .NET SDK | 8.0 | https://dotnet.microsoft.com/download |
| Docker Desktop | qualquer versão recente | https://www.docker.com/products/docker-desktop |
| Git | qualquer versão recente | https://git-scm.com |

## Instalação e execução

### 1. Clonar o repositório

```bash
git clone https://github.com/gabrielspereira2604/gestao-web.git
cd gestao-web
```

### 2. Copiar os arquivos de configuração

```bash
# Linux/macOS
cp .env.example .env
cp GestaoWeb/appsettings.Development.json.example GestaoWeb/appsettings.Development.json

# Windows (PowerShell)
Copy-Item .env.example .env
Copy-Item GestaoWeb/appsettings.Development.json.example GestaoWeb/appsettings.Development.json
```

Os arquivos já vêm com uma senha padrão (`Leve@2026!`) pronta para uso local. Nenhuma edição é necessária.

> Se quiser usar uma senha diferente, altere `DB_PASSWORD` no `.env` **e** `Password=` na connection string do `appsettings.Development.json` para o mesmo valor antes de continuar.

> `appsettings.Development.json` é ignorado pelo Git (`.gitignore`) para não expor credenciais.

### 4. Subir o banco de dados

```bash
docker compose up -d
```

O SQL Server leva cerca de 20–30 segundos para inicializar. Para confirmar que está pronto antes de continuar:

```bash
# Aguarda até o container responder (repita se necessário)
docker exec gestao-web-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Leve@2026!" -No -Q "SELECT 1" 2>/dev/null && echo "SQL Server pronto"
```

### 5. Aplicar as migrations

Caso não tenha a ferramenta EF instalada, instale primeiro:

```bash
dotnet tool install -g dotnet-ef
```

Em seguida, aplique as migrations:

```bash
dotnet ef database update --project GestaoWeb
```

> **Alternativa sem EF CLI**: execute os scripts `docs/sql/schema.sql` e depois `docs/sql/seed.sql` diretamente no SQL Server (SSMS, Azure Data Studio, `sqlcmd` etc.). Nesse caso, pule o passo 5 — o banco já estará pronto com o usuário inicial criado.

### 6. Executar a aplicação

```bash
dotnet run --project GestaoWeb
```

A URL de acesso será exibida no terminal (ex: `https://localhost:5001`). Abra-a no navegador.

### Credenciais do usuário gestor inicial

| Campo | Valor |
|---|---|
| Email | ti@leveinvestimentos.com.br |
| Senha | teste123 |

> O usuário gestor é criado automaticamente na primeira inicialização. Ao usar os scripts SQL alternativos, o mesmo usuário é inserido por `docs/sql/seed.sql`.

---

## Configuração de email (opcional)

Por padrão, o envio de email está desabilitado. Para habilitá-lo, preencha a seção `Smtp` no `GestaoWeb/appsettings.Development.json`:

```json
"Smtp": {
  "Host": "smtp.seuprovedor.com",
  "Port": 587,
  "User": "usuario@dominio.com",
  "Password": "senha-do-smtp",
  "From": "noreply@dominio.com"
}
```

Quando `Host` está em branco, os emails são simulados: o conteúdo completo (destinatário, assunto e corpo) é exibido no console com o prefixo `[EMAIL SIMULADO]`.

---

## Executar os testes

```bash
dotnet test GestaoWeb.Tests
```

25 testes unitários cobrindo repositories, controllers e email service.

---

## Estrutura do projeto

```
GestaoWeb/
├── Controllers/         # AccountController, UsersController, TasksController
├── Data/                # ApplicationDbContext, SeedData
├── Models/
│   ├── Domain/          # AppUser, TaskItem, WorkTaskStatus
│   └── ViewModels/      # ViewModels por feature
├── Repositories/        # IUserRepository, ITaskRepository e implementações
├── Services/            # IEmailService, EmailService, SmtpSettings
├── Views/               # Razor Views organizadas por controller
└── wwwroot/
    ├── css/site.css     # Estilos customizados (UIkit como base via CDN)
    └── uploads/         # Fotos de perfil enviadas pelos usuários
GestaoWeb.Tests/
├── Controllers/         # Testes dos controllers com Moq
├── Helpers/             # InMemoryFactory (EF InMemory + UserManager)
├── Repositories/        # Testes dos repositories com EF InMemory
└── Services/            # Testes do EmailService
docs/
└── sql/
    ├── schema.sql       # DDL completo (alternativa às migrations)
    └── seed.sql         # Usuário gestor inicial (alternativa ao SeedData.cs)
```
