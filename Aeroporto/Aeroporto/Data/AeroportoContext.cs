using SistemaAereo.Models;
using Microsoft.EntityFrameworkCore;

namespace SistemaAereo.Data
{
    public class AeroportoContext : DbContext
    {
        public AeroportoContext(DbContextOptions<AeroportoContext> options) : base(options)
        {
        }

        // =============================================
        // DbSets - ENTIDADES PRINCIPAIS
        // =============================================

        public DbSet<Aeronave> Aeronaves { get; set; }
        public DbSet<Aeroporto> Aeroportos { get; set; }
        public DbSet<Voo> Voos { get; set; }
        public DbSet<Escala> Escalas { get; set; }
        public DbSet<Poltrona> Poltronas { get; set; }
        public DbSet<ClientePreferencial> ClientesPreferenciais { get; set; }
        public DbSet<Passagem> Passagens { get; set; }

        // =============================================
        // CONFIGURAÇÃO DO MODELO
        // =============================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

        private void ConfigurarRelacionamentosVoos(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Voo>()
                .HasOne(v => v.AeroportoOrigem)
                .WithMany(a => a.VoosOrigem)
                .HasForeignKey(v => v.AeroportoOrigemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Voo>()
                .HasOne(v => v.AeroportoDestino)
                .WithMany(a => a.VoosDestino)
                .HasForeignKey(v => v.AeroportoDestinoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Voo>()
                .HasOne(v => v.Aeronave)
                .WithMany(a => a.Voos)
                .HasForeignKey(v => v.AeronaveId);
        }

        private void ConfigurarRelacionamentosEscalas(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Escala>()
                .HasOne(e => e.Voo)
                .WithMany(v => v.Escalas)
                .HasForeignKey(e => e.VooId);

            modelBuilder.Entity<Escala>()
                .HasOne(e => e.Aeroporto)
                .WithMany(a => a.Escalas)
                .HasForeignKey(e => e.AeroportoId);
        }

        private void ConfigurarRelacionamentosPoltronas(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Poltrona>()
                .HasOne(p => p.Voo)
                .WithMany(v => v.Poltronas)
                .HasForeignKey(p => p.VooId);
        }

        private void ConfigurarRelacionamentosPassagens(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Passagem>()
                .HasOne(p => p.Voo)
                .WithMany(v => v.Passagens)
                .HasForeignKey(p => p.VooId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Passagem>()
                .HasOne(p => p.Cliente)
                .WithMany()
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Passagem>()
                .HasOne(p => p.Poltrona)
                .WithMany(p => p.Passagens)
                .HasForeignKey(p => p.PoltronaId)
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

        private void ConfigurarIndicesClientes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientePreferencial>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<ClientePreferencial>()
                .HasIndex(c => c.CPF)
                .IsUnique();
        }

        private void ConfigurarIndicesAeroportos(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Aeroporto>()
                .HasIndex(a => a.CodigoIATA)
                .IsUnique();
        }

        private void ConfigurarIndicesVoos(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Voo>()
                .HasIndex(v => v.NumeroVoo)
                .IsUnique();
        }

        private void ConfigurarIndicesPassagens(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Passagem>()
                .HasIndex(p => p.NumeroBilhete)
                .IsUnique();
        }
    }
}