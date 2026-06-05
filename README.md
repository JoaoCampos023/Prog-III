# ✈️ SkyLine Aviation - Sistema de Gestão Aérea

![.NET Version](https://img.shields.io/badge/.NET-9.0-blue)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core_MVC-9.0-purple)
![SQL Server](https://img.shields.io/badge/SQL_Server-2019+-red)
![License](https://img.shields.io/badge/License-MIT-green)

## 📋 Sobre o Projeto

O **SkyLine Aviation** é um sistema de gestão aérea desenvolvido para gerenciar operações de uma companhia aérea, incluindo:

- ✈️ Cadastro e gerenciamento de **voos**
- 🎫 Emissão e controle de **passagens**
- 👥 Cadastro de **clientes preferenciais**
- 🛩️ Gerenciamento de **aeronaves** e **aeroportos**
- 💺 Controle de **poltronas** por voo
- 🔐 Autenticação e autorização de usuários

## 🚀 Tecnologias Utilizadas

### Backend

| Tecnologia 	        | Versão | Descrição 		      |
|-----------------------|--------|----------------------------|
| .NET 		        | 9.0    | Framework principal        |
| ASP.NET Core MVC      | 9.0    | Framework web 	      |
| Entity Framework Core | 9.0    | ORM para acesso a dados    |
| SQL Server            |   -    | Banco de dados relacional  |
| ASP.NET Core Identity | 9.0    | Autenticação e autorização |

### Frontend

| Tecnologia 	       | 	Descrição 		    |
|----------------------|------------------------------------|
| Bootstrap 5          | Framework CSS responsivo           |
| jQuery 	       | Manipulação do DOM  		    |
| DataTables           | Tabelas interativas com paginação  |
| Font Awesome 6       | Ícones vetoriais 		    |
| InputMask 	       | Máscaras para campos de formulário |
| Google Fonts (Inter) | Fonte personalizada 		    |
| Chart.js 	       | Gráficos interativos        	    |

### APIs Integradas

| API 		| Descrição 			| Endpoint 				     |
|---------------|-------------------------------|--------------------------------------------|
| ViaCEP 	| Busca de endereços por CEP    | `https://viacep.com.br/ws/{cep}/json/`     |
| DiceBear | Geração de avatares personalizados | `https://api.dicebear.com/9.x/{style}/svg` |



## 🎨 Design Patterns Implementados

| Pattern | Descrição | Status |
|---------|-----------|--------|
| **Repository** | Abstração da camada de acesso a dados | ✅ Completo |
| **Dependency Injection** | Inversão de controle via construtores | ✅ Completo |
| **Service Layer** | Encapsulamento de regras de negócio | ✅ Completo |
| **DTO** | Transferência de dados entre camadas | ✅ Completo |
| **Facade** | Simplificação de operações complexas | ✅ Completo |
| **Strategy** | Diferentes estratégias (avatares, poltronas) | ⚡ Parcial |
| **Builder** | Construção de objetos complexos | ⚡ Parcial |

## 🔐 Permissões e Roles

| Role | Permissões |
|------|-------------|
| **Admin** | Acesso total ao sistema + Gerenciamento de usuários |
| **Funcionario** | Acesso às operações do dia a dia (CRUD completo) |
| **User** | Acesso básico (consulta apenas + suas próprias passagens) |

## 📊 Funcionalidades Principais

### Dashboard
- Cards com estatísticas (voos, clientes, faturamento)
- Gráficos interativos (Chart.js)
- Próximos voos e passagens recentes
- Ações rápidas

### Voos
- CRUD completo com validações
- Geração automática de poltronas
- Mapa de ocupação
- Filtros por status (futuros/hoje/passados)

### Passagens
- Emissão com seleção de cliente, voo e poltrona
- Busca de poltronas disponíveis via AJAX
- Fluxo de check-in e embarque
- Cancelamento com liberação de poltrona
- Impressão de passagem otimizada

### Clientes
- Cadastro com validação de CPF e email único
- Busca automática de endereço por CEP (ViaCEP)
- Ativação/desativação de clientes
- Mala direta

### Relatórios
- Faturamento por período (com gráficos e exportação CSV)
- Ocupação de voos
- Clientes mais frequentes

### Gerenciamento de Usuários (Admin)
- Listagem de usuários
- Ativação/desativação
- Reset de senha
- Atribuição de roles

## 🔧 Pré-requisitos

Antes de executar o projeto, certifique-se de ter instalado:

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/pt-br/sql-server/sql-server-downloads) (ou SQL Server Express / LocalDB)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/) (opcional)

## ⚙️ Configuração do Ambiente

### 1. Clone o repositório

```bash
git clone https://github.com/seu-usuario/SkyLineAviation.git
cd SkyLineAviation
