# VetPlus Care — Sistema Web LH PET

Sistema web para gerenciamento de clínica veterinária, desenvolvido em ASP.NET Core MVC com C#. Possui painel administrativo para funcionários e portal de autoatendimento para clientes.

---

## Tecnologias

- **Backend:** ASP.NET Core MVC (.NET 9) com C#
- **Banco de dados:** MySQL com Entity Framework Core 9 + Pomelo
- **Autenticação:** Cookie Authentication (área administrativa) + JWT Bearer (portal do cliente)
- **Segurança de senhas:** BCrypt.Net
- **Frontend:** Razor Views (admin), HTML + Bootstrap + JavaScript (portal cliente)

---

## Funcionalidades

### Área Administrativa (Razor MVC)
- Login com redirecionamento automático após autenticação
- Suporte a senha temporária com fluxo de redefinição obrigatório
- Recuperação de senha via e-mail
- Gerenciamento de usuários (Admin/Funcionário): criação, ativação e desativação de contas
- Envio automático de e-mail com senha temporária ao criar usuário
- Painel com dados do usuário logado (nome, e-mail, perfil)
- Controle de acesso por perfil com `[Authorize(Roles = "...")]`

### Portal do Cliente (SPA com JWT)
- Registro de conta com validação de CPF e e-mail únicos
- Login com geração de token JWT
- Gerenciamento de perfil (nome, telefone, e-mail)
- Cadastro, edição e remoção de pets com espécie, raça e data de nascimento
- Cálculo dinâmico de idade do pet
- Agendamento de serviços (Consulta, Banho, Tosa) com verificação de horários disponíveis
- Visualização de agendamentos com status (Pendente, Concluído, Cancelado)

---

## Estrutura do Projeto

```
SistemaWeb_LH_PET/
├── Controllers/
│   ├── AutenticacaoController.cs   # Login/logout MVC (cookie)
│   ├── PainelController.cs         # Dashboard administrativo
│   ├── UsuariosController.cs       # CRUD de usuários (Admin)
│   ├── ApiAutenticacaoController.cs# Login/registro via API (JWT)
│   └── ApiClienteController.cs     # API do portal do cliente
├── Data/
│   └── ContextoBanco.cs            # DbContext
├── Models/                         # Entidades do domínio
├── Models/ViewModels/              # ViewModels e DTOs
├── Services/
│   └── IEmailService / EmailService
├── Validations/                    # Validações customizadas
├── Views/                          # Razor Views (área admin)
├── wwwroot/                        # Arquivos estáticos
│   └── portal-cliente.html         # Portal SPA do cliente
│   └── portal-cliente.js
└── Program.cs
```

---

## Configuração

### Pré-requisitos
- .NET 9 SDK
- MySQL 8+

### appsettings.json

Crie o arquivo `appsettings.json` na raiz do projeto com o seguinte conteúdo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=db_vetplus;User=root;Password=SUA_SENHA;"
  },
  "Jwt": {
    "Key": "sua-chave-secreta-com-no-minimo-32-caracteres"
  },
  "AdminInicial": {
    "Email": "admin@vetplus.com",
    "Senha": "SuaSenhaAdmin@2024"
  },
  "EmailSettings": {
    "SmtpHost": "smtp.seuservidor.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@vetplus.com",
    "SenderPassword": "SUA_SENHA_EMAIL"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> **Importante:** O `appsettings.json` **não deve ser commitado** se contiver credenciais reais de produção. Use variáveis de ambiente em produção.

### Banco de dados

Execute as migrations para criar as tabelas:

```bash
dotnet ef database update
```

O sistema criará automaticamente o usuário administrador inicial ao iniciar a aplicação, caso ele não exista no banco.

### Rodando o projeto

```bash
dotnet restore
dotnet run
```

A aplicação estará disponível em `http://localhost:5041`.

---

## Rotas principais

| Rota | Descrição |
|------|-----------|
| `/Autenticacao/Login` | Login da área administrativa |
| `/Painel` | Dashboard do admin/funcionário |
| `/Usuarios` | Gerenciamento de usuários (Admin) |
| `/portal-cliente.html` | Portal de autoatendimento do cliente |
| `POST /api/auth/login` | Login via API (retorna JWT) |
| `POST /api/auth/registrar` | Registro de novo cliente |
| `GET /api/cliente/perfil` | Dados do cliente logado |
| `GET /api/cliente/pets` | Lista de pets do cliente |
| `GET /api/cliente/agendamentos` | Agendamentos do cliente |
| `GET /api/cliente/horarios-disponiveis` | Horários livres para agendamento |

---

## Perfis de acesso

| Perfil | Acesso |
|--------|--------|
| `Admin` | Painel completo, gerenciamento de usuários |
| `Funcionario` | Painel (sem gerenciamento de usuários) |
| `Cliente` | Portal de autoatendimento via JWT |

---

## Autor

João Gaspar — [@gaspudo](https://github.com/gaspudo)
