# GestaoWeb

Sistema de gestão de tarefas para equipes financeiras. Desenvolvido em ASP.NET Core 8 MVC com SQL Server 2022.

## Funcionalidades

- Autenticação com email e senha
- Dois perfis: **gestor** e **colaborador**
- Gestores criam e acompanham tarefas; colaboradores atualizam o status das tarefas atribuídas
- Upload de foto de perfil
- Notificações por email (nova tarefa e conclusão)

## Pré-requisitos

| Ferramenta | Versão mínima |
|---|---|
| .NET SDK | 8.0 |
| Docker Desktop | qualquer versão recente |
| Git | qualquer versão recente |

## Instalação e execução

### 1. Clonar o repositório

```bash
git clone <url-do-repositorio>
cd GestaoWeb
```

### 2. Configurar variáveis de ambiente

Copie o arquivo de exemplo e defina a senha do banco:

```bash
cp .env.example .env
```

Edite `.env` e substitua `SuaSenhaAqui` por uma senha forte:

```
DB_PASSWORD=MinhaSenh@Forte123
```

### 3. Subir o banco de dados

```bash
docker compose up -d
```

Aguarde alguns segundos até o SQL Server inicializar completamente.

### 4. Aplicar as migrations

```bash
cd GestaoWeb
dotnet ef database update
cd ..
```

> **Alternativa sem EF CLI**: execute os scripts `docs/sql/schema.sql` e depois `docs/sql/seed.sql` diretamente no SQL Server (SSMS, Azure Data Studio, `sqlcmd` etc.). Nesse caso, pule o passo 4 — o banco já estará pronto e o usuário inicial já terá sido criado.

### 5. Executar a aplicação

```bash
cd GestaoWeb
dotnet run
```

Acesse [https://localhost:5001](https://localhost:5001) no navegador.

### Credenciais do usuário gestor inicial

| Campo | Valor |
|---|---|
| Email | ti@leveinvestimentos.com.br |
| Senha | teste123 |

> O usuário gestor é criado automaticamente na primeira inicialização via `SeedData.cs`. Ao usar os scripts SQL alternativos, o mesmo usuário é inserido por `docs/sql/seed.sql`.

## Configuração de email (opcional)

Para habilitar o envio de notificações por email, preencha a seção `Smtp` em `GestaoWeb/appsettings.Development.json`:

```json
"Smtp": {
  "Host": "smtp.seuprovedor.com",
  "Port": 587,
  "User": "usuario@dominio.com",
  "Password": "senha-do-smtp",
  "From": "noreply@dominio.com"
}
```

Quando `Host` está em branco, o envio de email é silenciosamente ignorado e a aplicação funciona normalmente.

## Executar os testes

```bash
dotnet test GestaoWeb.Tests/GestaoWeb.Tests.csproj
```

25 testes unitários cobrindo repositories, controllers e email service.

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
