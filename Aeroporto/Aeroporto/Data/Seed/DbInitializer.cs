using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;

namespace SistemaAereo.Data.Seed
{
    public static class DbInitializer
    {
        /// <summary>
        /// Inicializa o banco de dados com dados padrão
        /// </summary>
        public static void Initialize(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<AirportsContext>();

                context.Database.EnsureCreated();

                if (IsDatabaseEmpty(context))
                {
                    AddInitialData(context);
                }
            }
        }

        /// <summary>
        /// Verifica se o banco de dados está vazio
        /// </summary>
        private static bool IsDatabaseEmpty(AirportsContext context)
        {
            return !context.Aircrafts.Any() && !context.Airports.Any();
        }

        /// <summary>
        /// Adiciona dados iniciais ao banco de dados
        /// </summary>
        private static void AddInitialData(AirportsContext context)
        {
            var aircrafts = new[]
            {
                new Aircraft { AircraftType = "Boeing 737", NumberOfSeats = 180 },
                new Aircraft { AircraftType = "Airbus A320", NumberOfSeats = 150 }
            };
            context.Aircrafts.AddRange(aircrafts);

            var airports = new[]
            {
                new Airport { Name = "Aeroporto Internacional do Rio de Janeiro", IATACode = "GIG", City = "Rio de Janeiro", Country = "Brasil" },
                new Airport { Name = "Aeroporto Santos Dumont", IATACode = "SDU", City = "Rio de Janeiro", Country = "Brasil" }
            };
            context.Airports.AddRange(airports);

            context.SaveChanges();
        }
    }
}