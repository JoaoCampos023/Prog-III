using Microsoft.AspNetCore.Identity;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;

namespace SistemaAereo.Services
{
    public static class SeedUserService
    {
        /// <summary>
        /// Cria o usuário administrador padrão do sistema
        /// </summary>
        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = scope.ServiceProvider.GetRequiredService<AirportsContext>();

            // Garantir que o banco de dados foi criado
            await context.Database.EnsureCreatedAsync();

            // Criar roles se não existirem
            string[] roles = { "Admin", "Funcionario", "User" };

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
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrador do Sistema",
                    EmailConfirmed = true,
                    IsActive = true
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
                funcionarioUser = new User
                {
                    UserName = funcionarioEmail,
                    Email = funcionarioEmail,
                    FullName = "Funcionário do Sistema",
                    EmailConfirmed = true,
                    IsActive = true
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