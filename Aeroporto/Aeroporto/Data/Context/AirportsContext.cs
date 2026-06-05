using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;

namespace SistemaAereo.Data.Context
{
    // Contexto principal do banco de dados - herda IdentityDbContext para autenticação
    public class AirportsContext : IdentityDbContext<User>
    {
        public AirportsContext(DbContextOptions<AirportsContext> options) : base(options)
        {
        }

        // =============================================
        // DbSets - ENTIDADES PRINCIPAIS
        // =============================================

        // Conjunto de aeronaves
        public DbSet<Aircraft> Aircrafts { get; set; }

        // Conjunto de aeroportos
        public DbSet<Airport> Airports { get; set; }

        // Conjunto de voos
        public DbSet<Flight> Flights { get; set; }

        // Conjunto de escalas (conexões entre voos)
        public DbSet<Stopover> Stopovers { get; set; }

        // Conjunto de poltronas
        public DbSet<Seat> Seats { get; set; }

        // Conjunto de clientes preferenciais
        public DbSet<Customer> Customers { get; set; }

        // Conjunto de passagens (tickets)
        public DbSet<Ticket> Tickets { get; set; }

        // =============================================
        // CONFIGURAÇÃO DO MODELO
        // =============================================

        // Método chamado pelo EF Core ao criar o modelo do banco de dados
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Chama o método base para configurar as tabelas do Identity
            base.OnModelCreating(modelBuilder);

            ConfigurarRelacionamentos(modelBuilder);
            ConfigurarIndices(modelBuilder);
        }

        // =============================================
        // MÉTODOS DE CONFIGURAÇÃO DE RELACIONAMENTOS
        // =============================================

        // Configura todos os relacionamentos entre as entidades
        private void ConfigurarRelacionamentos(ModelBuilder modelBuilder)
        {
            ConfigurarRelacionamentosVoos(modelBuilder);
            ConfigurarRelacionamentosEscalas(modelBuilder);
            ConfigurarRelacionamentosPoltronas(modelBuilder);
            ConfigurarRelacionamentosPassagens(modelBuilder);
        }

        // Configura os relacionamentos da entidade Flight (Voo)
        private void ConfigurarRelacionamentosVoos(ModelBuilder modelBuilder)
        {
            // Flight -> DepartureAirport (Origem)
            // Um voo tem um aeroporto de origem, um aeroporto pode ter muitos voos de origem
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.DepartureAirport)
                .WithMany(a => a.DepartureFlights)
                .HasForeignKey(f => f.DepartureAirportId)
                .OnDelete(DeleteBehavior.Restrict);  // Impede exclusão em cascata

            // Flight -> ArrivalAirport (Destino)
            // Um voo tem um aeroporto de destino, um aeroporto pode ter muitos voos de destino
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.ArrivalAirport)
                .WithMany(a => a.ArrivalFlights)
                .HasForeignKey(f => f.ArrivalAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            // Flight -> Aircraft (Aeronave)
            // Um voo tem uma aeronave, uma aeronave pode ter muitos voos
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.Aircraft)
                .WithMany(a => a.Flights)
                .HasForeignKey(f => f.AircraftId);
        }

        // Configura os relacionamentos da entidade Stopover (Escala)
        private void ConfigurarRelacionamentosEscalas(ModelBuilder modelBuilder)
        {
            // Stopover -> Flight
            // Uma escala pertence a um voo, um voo pode ter várias escalas
            modelBuilder.Entity<Stopover>()
                .HasOne(s => s.Flight)
                .WithMany(f => f.Stopovers)
                .HasForeignKey(s => s.FlightId);

            // Stopover -> Airport
            // Uma escala ocorre em um aeroporto
            modelBuilder.Entity<Stopover>()
                .HasOne(s => s.Airport)
                .WithMany(a => a.Stopovers)
                .HasForeignKey(s => s.AirportId);
        }

        // Configura os relacionamentos da entidade Seat (Poltrona)
        private void ConfigurarRelacionamentosPoltronas(ModelBuilder modelBuilder)
        {
            // Seat -> Flight
            // Uma poltrona pertence a um voo
            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Flight)
                .WithMany(f => f.Seats)
                .HasForeignKey(s => s.FlightId);

            // Configura RowVersion para controle de concorrência (evita venda duplicada)
            modelBuilder.Entity<Seat>()
                .Property(s => s.RowVersion)
                .IsRowVersion();
        }

        // Configura os relacionamentos da entidade Ticket (Passagem)
        private void ConfigurarRelacionamentosPassagens(ModelBuilder modelBuilder)
        {
            // Ticket -> Flight
            // Uma passagem pertence a um voo
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Flight)
                .WithMany(f => f.Tickets)
                .HasForeignKey(t => t.FlightId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket -> Customer (Cliente)
            // Uma passagem pertence a um cliente
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Customer)
                .WithMany()
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket -> Seat (Poltrona)
            // Uma passagem está associada a uma poltrona específica
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Seat)
                .WithMany(s => s.Tickets)
                .HasForeignKey(t => t.SeatId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        // =============================================
        // MÉTODOS DE CONFIGURAÇÃO DE ÍNDICES
        // =============================================

        // Configura todos os índices do banco de dados
        private void ConfigurarIndices(ModelBuilder modelBuilder)
        {
            ConfigurarIndicesClientes(modelBuilder);
            ConfigurarIndicesAeroportos(modelBuilder);
            ConfigurarIndicesVoos(modelBuilder);
            ConfigurarIndicesPassagens(modelBuilder);
        }

        // Configura índices únicos para a entidade Customer (Cliente)
        private void ConfigurarIndicesClientes(ModelBuilder modelBuilder)
        {
            // Garante que o email seja único no banco
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // Garante que o CPF seja único no banco
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.CPF)
                .IsUnique();
        }

        // Configura índice único para a entidade Airport (Aeroporto) - Código IATA
        private void ConfigurarIndicesAeroportos(ModelBuilder modelBuilder)
        {
            // Garante que o código IATA seja único no banco
            modelBuilder.Entity<Airport>()
                .HasIndex(a => a.IATACode)
                .IsUnique();
        }

        // Configura índice único para a entidade Flight (Voo) - Número do Voo
        private void ConfigurarIndicesVoos(ModelBuilder modelBuilder)
        {
            // Garante que o número do voo seja único no banco
            modelBuilder.Entity<Flight>()
                .HasIndex(f => f.FlightNumber)
                .IsUnique();
        }

        // Configura índice único para a entidade Ticket (Passagem) - Número do Bilhete
        private void ConfigurarIndicesPassagens(ModelBuilder modelBuilder)
        {
            // Garante que o número do bilhete seja único no banco
            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.TicketNumber)
                .IsUnique();
        }
    }
}