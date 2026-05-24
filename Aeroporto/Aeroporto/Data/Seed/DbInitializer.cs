using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;

namespace SistemaAereo.Data.Seed
{
    public static class DbInitializer
    {
        public static void Initialize(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<AeroportoContext>();

                context.Database.EnsureCreated();

                if (BancoVazio(context))
                {
                    AdicionarDadosIniciais(context);
                }
            }
        }

        private static bool BancoVazio(AeroportoContext context)
        {
            return !context.Aeronaves.Any() && !context.Aeroportos.Any();
        }

        private static void AdicionarDadosIniciais(AeroportoContext context)
        {
            var aeronaves = new[]
            {
                new Aeronave { TipoAeronave = "Boeing 737", NumeroPoltronas = 180 },
                new Aeronave { TipoAeronave = "Airbus A320", NumeroPoltronas = 150 }
            };
            context.Aeronaves.AddRange(aeronaves);

            var aeroportos = new[]
            {
                new Aeroporto { Nome = "Aeroporto Internacional do Rio de Janeiro", CodigoIATA = "GIG", Cidade = "Rio de Janeiro", Pais = "Brasil" },
                new Aeroporto { Nome = "Aeroporto Santos Dumont", CodigoIATA = "SDU", Cidade = "Rio de Janeiro", Pais = "Brasil" }
            };
            context.Aeroportos.AddRange(aeroportos);

            context.SaveChanges();
        }
    }
}