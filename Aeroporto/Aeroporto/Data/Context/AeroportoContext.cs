using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Models.Entities;

namespace SistemaAereo.Data.Context
{
    public class AeroportoContext : IdentityDbContext<Usuario>
    {
        public AeroportoContext(DbContextOptions<AeroportoContext> options) : base(options)
        {
        }

        // DbSets existentes
        public DbSet<Aeronave> Aeronaves { get; set; }
        public DbSet<Aeroporto> Aeroportos { get; set; }
        public DbSet<Voo> Voos { get; set; }
        public DbSet<Escala> Escalas { get; set; }
        public DbSet<Poltrona> Poltronas { get; set; }
        public DbSet<ClientePreferencial> ClientesPreferenciais { get; set; }
        public DbSet<Passagem> Passagens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações existentes do seu sistema
            ConfigurarRelacionamentos(modelBuilder);
            ConfigurarIndices(modelBuilder);
        }

        private void ConfigurarRelacionamentos(ModelBuilder modelBuilder)
        {
            // Voo - AeroportoOrigem
            modelBuilder.Entity<Voo>()
                .HasOne(v => v.AeroportoOrigem)
                .WithMany(a => a.VoosOrigem)
                .HasForeignKey(v => v.AeroportoOrigemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Voo - AeroportoDestino
            modelBuilder.Entity<Voo>()
                .HasOne(v => v.AeroportoDestino)
                .WithMany(a => a.VoosDestino)
                .HasForeignKey(v => v.AeroportoDestinoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Voo - Aeronave
            modelBuilder.Entity<Voo>()
                .HasOne(v => v.Aeronave)
                .WithMany(a => a.Voos)
                .HasForeignKey(v => v.AeronaveId);

            // Escala - Voo
            modelBuilder.Entity<Escala>()
                .HasOne(e => e.Voo)
                .WithMany(v => v.Escalas)
                .HasForeignKey(e => e.VooId);

            // Escala - Aeroporto
            modelBuilder.Entity<Escala>()
                .HasOne(e => e.Aeroporto)
                .WithMany(a => a.Escalas)
                .HasForeignKey(e => e.AeroportoId);

            // Poltrona - Voo
            modelBuilder.Entity<Poltrona>()
                .HasOne(p => p.Voo)
                .WithMany(v => v.Poltronas)
                .HasForeignKey(p => p.VooId);

            // Passagem - Voo
            modelBuilder.Entity<Passagem>()
                .HasOne(p => p.Voo)
                .WithMany(v => v.Passagens)
                .HasForeignKey(p => p.VooId)
                .OnDelete(DeleteBehavior.Restrict);

            // Passagem - Cliente
            modelBuilder.Entity<Passagem>()
                .HasOne(p => p.Cliente)
                .WithMany()
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Passagem - Poltrona
            modelBuilder.Entity<Passagem>()
                .HasOne(p => p.Poltrona)
                .WithMany(p => p.Passagens)
                .HasForeignKey(p => p.PoltronaId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigurarIndices(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientePreferencial>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<ClientePreferencial>()
                .HasIndex(c => c.CPF)
                .IsUnique();

            modelBuilder.Entity<Aeroporto>()
                .HasIndex(a => a.CodigoIATA)
                .IsUnique();

            modelBuilder.Entity<Voo>()
                .HasIndex(v => v.NumeroVoo)
                .IsUnique();

            modelBuilder.Entity<Passagem>()
                .HasIndex(p => p.NumeroBilhete)
                .IsUnique();
        }
    }
}