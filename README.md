# GestaoWeb

Sistema de gestão de tarefas desenvolvido em ASP.NET Core 8 MVC com SQL Server 2022.

**Funcionalidades:** autenticação por e-mail e senha · dois perfis (gestor e colaborador) · cadastro de usuários com foto · agendamento e acompanhamento de tarefas · notificações por e-mail

## Pré-requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Execução

```bash
# 1. Clonar
git clone https://github.com/gabrielspereira2604/gestao-web.git
cd gestao-web

# 2. Configuração (senha padrão já preenchida, nenhuma edição necessária)
Copy-Item .env.example .env
Copy-Item GestaoWeb/appsettings.Development.json.example GestaoWeb/appsettings.Development.json

# 3. Banco de dados + migrations (aguarda SQL Server e aplica automaticamente)
# instale dotnet-ef se necessário: dotnet tool install -g dotnet-ef
.\setup.ps1

# 4. Rodar
dotnet run --project GestaoWeb
```

A URL aparece no terminal. Acesse no navegador.

**Credenciais do gestor inicial**

| Email | Senha |
|---|---|
| ti@leveinvestimentos.com.br | teste123 |

## Email (opcional)

Sem configuração, os emails são simulados no console (`[EMAIL SIMULADO]`). Para habilitar, preencha a seção `Smtp` em `GestaoWeb/appsettings.Development.json`:

```json
"Smtp": {
  "Host": "smtp.seuprovedor.com",
  "Port": 587,
  "User": "usuario@dominio.com",
  "Password": "senha",
  "From": "noreply@dominio.com"
}
```

## Testes

```bash
dotnet test GestaoWeb.Tests
```

## Estrutura

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
└── wwwroot/uploads/     # Fotos de perfil
GestaoWeb.Tests/
├── Controllers/         # Testes com Moq
├── Repositories/        # Testes com EF InMemory
└── Services/            # Testes do EmailService
docs/sql/
├── schema.sql           # DDL completo (alternativa às migrations)
└── seed.sql             # Usuário gestor inicial
```
