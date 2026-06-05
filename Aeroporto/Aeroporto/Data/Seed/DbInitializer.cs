using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;

namespace SistemaAereo.Data.Seed
{
    // Classe estática para inicializar o banco de dados com dados padrão
    public static class DbInitializer
    {
        // Método público que inicia a inicialização do banco
        public static void Initialize(IApplicationBuilder app)
        {
            // Cria um escopo para acessar os serviços do container de DI
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                // Obtém o contexto do banco de dados
                var context = serviceScope.ServiceProvider.GetService<AirportsContext>();

                // Garante que o banco de dados foi criado (se não existir, cria)
                context.Database.EnsureCreated();

                // Verifica se o banco está vazio e adiciona dados iniciais se necessário
                if (IsDatabaseEmpty(context))
                {
                    AddInitialData(context);
                }
            }
        }

        // Verifica se o banco de dados está vazio (não tem aeronaves nem aeroportos)
        private static bool IsDatabaseEmpty(AirportsContext context)
        {
            return !context.Aircrafts.Any() && !context.Airports.Any();
        }

        // Adiciona os dados iniciais ao banco de dados
        private static void AddInitialData(AirportsContext context)
        {
            // Adiciona aeronaves padrão
            var aircrafts = new[]
            {
                new Aircraft { AircraftType = "Boeing 737", NumberOfSeats = 180 },
                new Aircraft { AircraftType = "Airbus A320", NumberOfSeats = 150 }
            };
            context.Aircrafts.AddRange(aircrafts);

            // Adiciona aeroportos padrão
            var airports = new[]
            {
                new Airport { Name = "Aeroporto Internacional do Rio de Janeiro", IATACode = "GIG", City = "Rio de Janeiro", Country = "Brasil" },
                new Airport { Name = "Aeroporto Santos Dumont", IATACode = "SDU", City = "Rio de Janeiro", Country = "Brasil" }
            };
            context.Airports.AddRange(airports);

            // Salva as alterações no banco de dados
            context.SaveChanges();
        }
    }
}