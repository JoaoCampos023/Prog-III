using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Data.Seed;
using SistemaAereo.Repositories;
using SistemaAereo.Repositories.Interfaces;
using SistemaAereo.Services;
using SistemaAereo.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configuração do Entity Framework
builder.Services.AddDbContext<AeroportoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro dos repositórios
builder.Services.AddScoped<IVooRepository, VooRepository>();
builder.Services.AddScoped<IAeronaveRepository, AeronaveRepository>();
builder.Services.AddScoped<IAeroportoRepository, AeroportoRepository>();
builder.Services.AddScoped<IClientePreferencialRepository, ClientePreferencialRepository>();
builder.Services.AddScoped<IPassagemRepository, PassagemRepository>();
builder.Services.AddScoped<IPoltronaRepository, PoltronaRepository>();

// Registro dos serviços
builder.Services.AddScoped<IPoltronaService, PoltronaService>();

var app = builder.Build();

// Inicializar banco de dados
DbInitializer.Initialize(app);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();