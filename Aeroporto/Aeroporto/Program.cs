using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SistemaAereo.Data.Context;
using SistemaAereo.Facades.Implementations;
using SistemaAereo.Facades.Interfaces;
using SistemaAereo.Models.Entities;
using SistemaAereo.Repositories;
using SistemaAereo.Repositories.Interfaces;
using SistemaAereo.Services;
using SistemaAereo.Services.Interfaces;

// =============================================
// CONFIGURAÇÃO INICIAL DA APLICAÇÃO
// =============================================

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços MVC (Controllers e Views) ao container de DI
builder.Services.AddControllersWithViews();

// =============================================
// CONFIGURAÇÃO DE LOCALIZAÇÃO (PORTUGUÊS)
// =============================================

// Configura a localização da aplicação para português do Brasil
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    // Define o português do Brasil como cultura padrão
    var supportedCultures = new[] { new CultureInfo("pt-BR") };
    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Habilita serviços de localização (para tradução de mensagens)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// =============================================
// CONFIGURAÇÃO DO BANCO DE DADOS (Entity Framework)
// =============================================

// Registra o contexto do banco de dados no container de DI
builder.Services.AddDbContext<AirportsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =============================================
// CONFIGURAÇÃO DO IDENTITY (AUTENTICAÇÃO)
// =============================================

// Configura o sistema de autenticação e autorização do ASP.NET Core Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // =============================================
    // CONFIGURAÇÕES DE SENHA
    // =============================================

    // Exige pelo menos um número na senha
    options.Password.RequireDigit = true;

    // Tamanho mínimo da senha
    options.Password.RequiredLength = 6;

    // Não exige caractere especial (opcional)
    options.Password.RequireNonAlphanumeric = false;

    // Exige pelo menos uma letra maiúscula
    options.Password.RequireUppercase = true;

    // Exige pelo menos uma letra minúscula
    options.Password.RequireLowercase = true;

    // =============================================
    // CONFIGURAÇÕES DE LOCKOUT (BLOQUEIO)
    // =============================================

    // Tempo de bloqueio após tentativas inválidas
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

    // Número máximo de tentativas antes do bloqueio
    options.Lockout.MaxFailedAccessAttempts = 5;

    // Permite bloqueio para novos usuários
    options.Lockout.AllowedForNewUsers = true;

    // =============================================
    // CONFIGURAÇÕES DE USUÁRIO
    // =============================================

    // Exige que o email seja único no sistema
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AirportsContext>() // Usa o AirportsContext para armazenar os dados
.AddDefaultTokenProviders() // Provedores de token para recuperação de senha, etc.
.AddErrorDescriber<PortugueseIdentityErrorDescriber>(); // Traduz mensagens de erro para português

// =============================================
// CONFIGURAÇÃO DO COOKIE DE AUTENTICAÇÃO
// =============================================

builder.Services.ConfigureApplicationCookie(options =>
{
    // Página de login (redireciona quando não autenticado)
    options.LoginPath = "/Account/Login";

    // Página de logout
    options.LogoutPath = "/Account/Logout";

    // Página de acesso negado (redireciona quando não autorizado)
    options.AccessDeniedPath = "/Account/AccessDenied";

    // Tempo de expiração do cookie (8 horas)
    options.ExpireTimeSpan = TimeSpan.FromHours(8);

    // Renova o cookie automaticamente com atividade do usuário
    options.SlidingExpiration = true;
});

// =============================================
// REGISTRO DOS REPOSITÓRIOS
// =============================================

// Registra cada repositório como Scoped (uma instância por requisição HTTP)
builder.Services.AddScoped<IAircraftRepository, AircraftRepository>();
builder.Services.AddScoped<IAirportRepository, AirportRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IFlightRepository, FlightRepository>();

// =============================================
// REGISTRO DOS SERVIÇOS
// =============================================

// Serviço de poltronas (Scoped)
builder.Services.AddScoped<ISeatService, SeatService>();

// Serviço ViaCEP - Configura o HttpClient para consumir a API
builder.Services.AddHttpClient<IViaCepService, ViaCepService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10); // Timeout de 10 segundos
    client.DefaultRequestHeaders.Add("Accept", "application/json"); // Aceita resposta JSON
});
builder.Services.AddScoped<IViaCepService, ViaCepService>();

// Serviço de Avatar - Configura o HttpClient para consumir a API DiceBear
builder.Services.AddHttpClient<IAvatarService, AvatarService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("Accept", "image/svg+xml"); // Aceita resposta SVG
});
builder.Services.AddScoped<IAvatarService, AvatarService>();

// =============================================
// REGISTRO DAS FACADES
// =============================================

// Fachadas para operações complexas (Scoped)
builder.Services.AddScoped<ITicketFacade, TicketFacade>();
builder.Services.AddScoped<IFlightFacade, FlightFacade>();

// =============================================
// CONSTRUÇÃO DA APLICAÇÃO
// =============================================

var app = builder.Build();

// =============================================
// CONFIGURAÇÃO DO PIPELINE DE REQUISIÇÕES
// =============================================

// Ativa a localização (português)
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

// =============================================
// INICIALIZAÇÃO DO BANCO DE DADOS
// =============================================

// Cria um escopo para resolver serviços durante a inicialização
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Cria o usuário administrador padrão (admin@sistema.com / Admin@123)
        await SeedUserService.SeedAdminUserAsync(services);
    }
    catch (Exception ex)
    {
        // Log do erro caso a criação do usuário admin falhe
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao criar usuário admin");
    }
}

// =============================================
// CONFIGURAÇÃO DO PIPELINE DE REQUISIÇÕES (CONTINUAÇÃO)
// =============================================

// Em ambiente de desenvolvimento, usa página de erro detalhada
if (!app.Environment.IsDevelopment())
{
    // Em produção, usa página de erro genérica
    app.UseExceptionHandler("/Home/Error");

    // Habilita HSTS (HTTP Strict Transport Security)
    app.UseHsts();
}

// Redireciona HTTP para HTTPS
app.UseHttpsRedirection();

// Serve arquivos estáticos (CSS, JS, imagens)
app.UseStaticFiles();

// Habilita roteamento
app.UseRouting();

// =============================================
// AUTENTICAÇÃO E AUTORIZAÇÃO
// =============================================

// Habilita autenticação (verifica quem é o usuário)
app.UseAuthentication();

// Habilita autorização (verifica o que o usuário pode fazer)
app.UseAuthorization();

// =============================================
// MAPEAMENTO DE ROTAS
// =============================================

// Rota padrão para as Views (HomeController/Index)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Rota para as APIs (api/controller/action)
app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller}/{action}/{id?}");

// =============================================
// EXECUÇÃO DA APLICAÇÃO
// =============================================

// Inicia a aplicação e começa a ouvir requisições HTTP
app.Run();