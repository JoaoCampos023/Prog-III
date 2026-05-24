using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAereo.Models
{
    public class Voo
    {
        // =============================================
        // PROPRIEDADES PRINCIPAIS
        // =============================================

        [Key]
        public int VooId { get; set; }

        [Required(ErrorMessage = "O número do voo é obrigatório")]
        [StringLength(10, ErrorMessage = "O número do voo deve ter no máximo 10 caracteres")]
        [Display(Name = "Número do Voo")]
        public string NumeroVoo { get; set; }

        [Required(ErrorMessage = "O aeroporto de origem é obrigatório")]
        [Display(Name = "Aeroporto de Origem")]
        public int AeroportoOrigemId { get; set; }

        [Required(ErrorMessage = "O aeroporto de destino é obrigatório")]
        [Display(Name = "Aeroporto de Destino")]
        public int AeroportoDestinoId { get; set; }

        [Required(ErrorMessage = "A aeronave é obrigatória")]
        [Display(Name = "Aeronave")]
        public int AeronaveId { get; set; }

        // =============================================
        // PROPRIEDADES DE HORÁRIO
        // =============================================

        [Required(ErrorMessage = "O horário de saída é obrigatório")]
        [Display(Name = "Horário de Saída")]
        public DateTime HorarioSaida { get; set; }

        [Required(ErrorMessage = "O horário de chegada é obrigatório")]
        [Display(Name = "Horário de Chegada Previsto")]
        public DateTime HorarioChegadaPrevisto { get; set; }

        // =============================================
        // RELACIONAMENTOS
        // =============================================

        [ForeignKey("AeroportoOrigemId")]
        public virtual Aeroporto AeroportoOrigem { get; set; }

        [ForeignKey("AeroportoDestinoId")]
        public virtual Aeroporto AeroportoDestino { get; set; }

        [ForeignKey("AeronaveId")]
        public virtual Aeronave Aeronave { get; set; }

        public virtual ICollection<Escala> Escalas { get; set; }
        public virtual ICollection<Poltrona> Poltronas { get; set; }
        public virtual ICollection<Passagem> Passagens { get; set; }

        // =============================================
        // CONSTRUTOR
        // =============================================

        public Voo()
        {
            Escalas = new HashSet<Escala>();
            Poltronas = new HashSet<Poltrona>();
            Passagens = new HashSet<Passagem>();
        }
    }
}