using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.Enums;

namespace SistemaAereo.Data.Context
{
    public class AirportsContext : IdentityDbContext<User>
    {
        public AirportsContext(DbContextOptions<AirportsContext> options) : base(options)
        {
        }

        // =============================================
        // DbSets - ENTIDADES PRINCIPAIS
        // =============================================

        public DbSet<Aircraft> Aircrafts { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Stopover> Stopovers { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        // =============================================
        // CONFIGURAÇÃO DO MODELO
        // =============================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigurarRelacionamentos(modelBuilder);
            ConfigurarIndices(modelBuilder);
        }

        // =============================================
        // MÉTODOS DE CONFIGURAÇÃO DE RELACIONAMENTOS
        // =============================================

        private void ConfigurarRelacionamentos(ModelBuilder modelBuilder)
        {
            ConfigurarRelacionamentosVoos(modelBuilder);
            ConfigurarRelacionamentosEscalas(modelBuilder);
            ConfigurarRelacionamentosPoltronas(modelBuilder);
            ConfigurarRelacionamentosPassagens(modelBuilder);
        }

        /// <summary>
        /// Configura os relacionamentos da entidade Flight (Voo)
        /// </summary>
        private void ConfigurarRelacionamentosVoos(ModelBuilder modelBuilder)
        {
            // Flight -> DepartureAirport (Origem)
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.DepartureAirport)
                .WithMany(a => a.DepartureFlights)
                .HasForeignKey(f => f.DepartureAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            // Flight -> ArrivalAirport (Destino)
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.ArrivalAirport)
                .WithMany(a => a.ArrivalFlights)
                .HasForeignKey(f => f.ArrivalAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            // Flight -> Aircraft (Aeronave)
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.Aircraft)
                .WithMany(a => a.Flights)
                .HasForeignKey(f => f.AircraftId);
        }

        /// <summary>
        /// Configura os relacionamentos da entidade Stopover (Escala)
        /// </summary>
        private void ConfigurarRelacionamentosEscalas(ModelBuilder modelBuilder)
        {
            // Stopover -> Flight
            modelBuilder.Entity<Stopover>()
                .HasOne(s => s.Flight)
                .WithMany(f => f.Stopovers)
                .HasForeignKey(s => s.FlightId);

            // Stopover -> Airport
            modelBuilder.Entity<Stopover>()
                .HasOne(s => s.Airport)
                .WithMany(a => a.Stopovers)
                .HasForeignKey(s => s.AirportId);
        }

        /// <summary>
        /// Configura os relacionamentos da entidade Seat (Poltrona)
        /// </summary>
        private void ConfigurarRelacionamentosPoltronas(ModelBuilder modelBuilder)
        {
            // Seat -> Flight
            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Flight)
                .WithMany(f => f.Seats)
                .HasForeignKey(s => s.FlightId);

            // Configurar RowVersion para controle de concorrência
            modelBuilder.Entity<Seat>()
                .Property(s => s.RowVersion)
                .IsRowVersion();
        }

        /// <summary>
        /// Configura os relacionamentos da entidade Ticket (Passagem)
        /// </summary>
        private void ConfigurarRelacionamentosPassagens(ModelBuilder modelBuilder)
        {
            // Ticket -> Flight
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Flight)
                .WithMany(f => f.Tickets)
                .HasForeignKey(t => t.FlightId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket -> Customer (Cliente)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Customer)
                .WithMany()
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket -> Seat (Poltrona)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Seat)
                .WithMany(s => s.Tickets)
                .HasForeignKey(t => t.SeatId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        // =============================================
        // MÉTODOS DE CONFIGURAÇÃO DE ÍNDICES
        // =============================================

        private void ConfigurarIndices(ModelBuilder modelBuilder)
        {
            ConfigurarIndicesClientes(modelBuilder);
            ConfigurarIndicesAeroportos(modelBuilder);
            ConfigurarIndicesVoos(modelBuilder);
            ConfigurarIndicesPassagens(modelBuilder);
        }

        /// <summary>
        /// Configura índices únicos para a entidade Customer (Cliente)
        /// </summary>
        private void ConfigurarIndicesClientes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.CPF)
                .IsUnique();
        }

        /// <summary>
        /// Configura índice único para a entidade Airport (Aeroporto) - Código IATA
        /// </summary>
        private void ConfigurarIndicesAeroportos(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Airport>()
                .HasIndex(a => a.IATACode)
                .IsUnique();
        }

        /// <summary>
        /// Configura índice único para a entidade Flight (Voo) - Número do Voo
        /// </summary>
        private void ConfigurarIndicesVoos(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Flight>()
                .HasIndex(f => f.FlightNumber)
                .IsUnique();
        }

        /// <summary>
        /// Configura índice único para a entidade Ticket (Passagem) - Número do Bilhete
        /// </summary>
        private void ConfigurarIndicesPassagens(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.TicketNumber)
                .IsUnique();
        }
    }
}