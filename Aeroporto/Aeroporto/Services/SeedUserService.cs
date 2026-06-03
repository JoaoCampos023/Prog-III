using Microsoft.AspNetCore.Identity;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Services.Interfaces;

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
            var avatarService = scope.ServiceProvider.GetRequiredService<IAvatarService>();
            var context = scope.ServiceProvider.GetRequiredService<AirportsContext>();

            // Garantir que o banco de dados foi criado
            await context.Database.EnsureCreatedAsync();

            // Criar roles se não existirem (apenas as 3 principais)
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
                var avatarUrl = avatarService.GerarAvatarUrl("Administrador", 128);

                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrador do Sistema",
                    EmailConfirmed = true,
                    IsActive = true,
                    AvatarUrl = avatarUrl
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
                var avatarUrl = avatarService.GerarAvatarUrl("Funcionario", 128);

                funcionarioUser = new User
                {
                    UserName = funcionarioEmail,
                    Email = funcionarioEmail,
                    FullName = "Funcionário do Sistema",
                    EmailConfirmed = true,
                    IsActive = true,
                    AvatarUrl = avatarUrl
                };

                var result = await userManager.CreateAsync(funcionarioUser, "Func@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(funcionarioUser, "Funcionario");
                }
            }

            // Criar usuário Comum padrão (opcional)
            var userEmail = "usuario@sistema.com";
            var commonUser = await userManager.FindByEmailAsync(userEmail);

            if (commonUser == null)
            {
                var avatarUrl = avatarService.GerarAvatarUrl("Usuario", 128);

                commonUser = new User
                {
                    UserName = userEmail,
                    Email = userEmail,
                    FullName = "Usuário Comum",
                    EmailConfirmed = true,
                    IsActive = true,
                    AvatarUrl = avatarUrl
                };

                var result = await userManager.CreateAsync(commonUser, "User@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(commonUser, "User");
                }
            }
        }
    }
}