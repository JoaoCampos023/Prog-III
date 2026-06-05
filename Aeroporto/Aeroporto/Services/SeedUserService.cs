using Microsoft.AspNetCore.Identity;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Services.Interfaces;

namespace SistemaAereo.Services
{
    // Serviço estático para criar usuários padrão do sistema
    // Executado na inicialização da aplicação (Program.cs)
    public static class SeedUserService
    {
        // Cria o usuário administrador e funcionário padrão
        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            // Cria um escopo para acessar os serviços do container de DI
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var avatarService = scope.ServiceProvider.GetRequiredService<IAvatarService>();
            var context = scope.ServiceProvider.GetRequiredService<AirportsContext>();

            // Garante que o banco de dados foi criado
            await context.Database.EnsureCreatedAsync();

            // =============================================
            // CRIAÇÃO DAS ROLES (PERFIS DE ACESSO)
            // =============================================

            // Lista de roles disponíveis no sistema
            string[] roles = { "Admin", "Funcionario", "User" };

            // Cria cada role se não existir
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // =============================================
            // CRIAÇÃO DO USUÁRIO ADMINISTRADOR
            // =============================================

            var adminEmail = "admin@sistema.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Gera avatar para o administrador
                var avatarUrl = avatarService.GerarAvatarUrl("Administrador", 128);

                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrador do Sistema",
                    EmailConfirmed = true,  // Confirma o email automaticamente
                    IsActive = true,
                    AvatarUrl = avatarUrl
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    // Adiciona o usuário à role Admin
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // =============================================
            // CRIAÇÃO DO USUÁRIO FUNCIONÁRIO
            // =============================================

            var funcionarioEmail = "funcionario@sistema.com";
            var funcionarioUser = await userManager.FindByEmailAsync(funcionarioEmail);

            if (funcionarioUser == null)
            {
                // Gera avatar para o funcionário
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
                    // Adiciona o usuário à role Funcionario
                    await userManager.AddToRoleAsync(funcionarioUser, "Funcionario");
                }
            }

            // =============================================
            // CRIAÇÃO DO USUÁRIO COMUM (OPCIONAL)
            // =============================================

            var userEmail = "usuario@sistema.com";
            var commonUser = await userManager.FindByEmailAsync(userEmail);

            if (commonUser == null)
            {
                // Gera avatar para o usuário comum
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
                    // Adiciona o usuário à role User
                    await userManager.AddToRoleAsync(commonUser, "User");
                }
            }
        }
    }
}