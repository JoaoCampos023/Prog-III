using Microsoft.AspNetCore.Identity;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;

namespace SistemaAereo.Services
{
    public static class SeedUserService
    {
        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = scope.ServiceProvider.GetRequiredService<AeroportoContext>();

            // Garantir que o banco de dados foi criado
            await context.Database.EnsureCreatedAsync();

            // Criar roles se não existirem
            string[] roles = { "Admin", "Funcionario", "Usuario" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Criar usuário Admin padrão
            var adminEmail = "admin@sistema.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new Usuario
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NomeCompleto = "Administrador do Sistema",
                    EmailConfirmed = true,
                    Ativo = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Criar usuário Funcionário padrão
            var funcionarioEmail = "funcionario@sistema.com";
            var funcionarioUser = await userManager.FindByEmailAsync(funcionarioEmail);

            if (funcionarioUser == null)
            {
                funcionarioUser = new Usuario
                {
                    UserName = funcionarioEmail,
                    Email = funcionarioEmail,
                    NomeCompleto = "Funcionário do Sistema",
                    EmailConfirmed = true,
                    Ativo = true
                };

                var result = await userManager.CreateAsync(funcionarioUser, "Func@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(funcionarioUser, "Funcionario");
                }
            }
        }
    }
}