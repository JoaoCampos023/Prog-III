using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data;

namespace SistemaAereo.Services
{
    public static class DbInitializer
    {
        // =============================================
        // MÉTODOS PÚBLICOS
        // =============================================

        public static void Initialize(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<AeroportoContext>();

                GarantirBancoCriado(context);
                InicializarDados(context);
            }
        }

        // =============================================
        // MÉTODOS PRIVADOS DE INICIALIZAÇÃO
        // =============================================

        private static void GarantirBancoCriado(AeroportoContext context)
        {
            context.Database.EnsureCreated();
        }

        private static void InicializarDados(AeroportoContext context)
        {
            if (BancoVazio(context))
            {
                AdicionarDadosIniciais(context);
            }
        }

        private static bool BancoVazio(AeroportoContext context)
        {
            return !context.Aeronaves.Any() && !context.Aeroportos.Any();
        }

        private static void AdicionarDadosIniciais(AeroportoContext context)
        {
            AdicionarAeronavesIniciais(context);
            AdicionarAeroportosIniciais(context);

            PersistirDados(context);
        }

        // =============================================
        // MÉTODOS DE ADIÇÃO DE DADOS ESPECÍFICOS
        // =============================================

        private static void AdicionarAeronavesIniciais(AeroportoContext context)
        {
            var aeronaves = new[]
            {
                new Models.Aeronave
                {
                    TipoAeronave = "Boeing 737",
                    NumeroPoltronas = 180
                },
                new Models.Aeronave
                {
                    TipoAeronave = "Airbus A320",
                    NumeroPoltronas = 150
                }
            };

            context.Aeronaves.AddRange(aeronaves);
        }

        private static void AdicionarAeroportosIniciais(AeroportoContext context)
        {
            var aeroportos = new[]
            {
                new Models.Aeroporto
                {
                    Nome = "Aeroporto Internacional do Rio de Janeiro",
                    CodigoIATA = "GIG",
                    Cidade = "Rio de Janeiro",
                    Pais = "Brasil"
                },
                new Models.Aeroporto
                {
                    Nome = "Aeroporto Santos Dumont",
                    CodigoIATA = "SDU",
                    Cidade = "Rio de Janeiro",
                    Pais = "Brasil"
                }
            };

            context.Aeroportos.AddRange(aeroportos);
        }

        // =============================================
        // MÉTODOS AUXILIARES
        // =============================================

        private static void PersistirDados(AeroportoContext context)
        {
            context.SaveChanges();
        }
    }
}