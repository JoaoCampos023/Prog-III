using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.Entities
{
    // Representa uma poltrona/assento em um voo específico
    public class Seat
    {
        // Identificador único da poltrona
        [Key]
        public int SeatId { get; set; }

        // ID do voo ao qual esta poltrona pertence
        [Required]
        public int FlightId { get; set; }

        // Número da poltrona (ex: 12A, 1B, 23F)
        [Required]
        [StringLength(10)]
        [Display(Name = "Número da Poltrona")]
        public string SeatNumber { get; set; }

        // Indica se a poltrona está disponível para venda
        [Required]
        [Display(Name = "Disponível")]
        public bool IsAvailable { get; set; } = true;

        // Localização da poltrona na fileira (Janela, Corredor, Meio)
        [Required]
        [StringLength(20)]
        [Display(Name = "Localização")]
        public string Location { get; set; }

        // Classe da poltrona (Primeira, Executiva, Econômica)
        [StringLength(20)]
        [Display(Name = "Tipo")]
        public string Class { get; set; }

        // Preço da passagem para esta poltrona
        [DataType(DataType.Currency)]
        [Display(Name = "Preço")]
        public decimal Price { get; set; }

        // Controle de concorrência (evita venda duplicada)
        [Timestamp]
        public byte[] RowVersion { get; set; }

        // Lista de passagens vendidas para esta poltrona
        public virtual ICollection<Ticket> Tickets { get; set; }

        // Voo ao qual esta poltrona pertence
        public virtual Flight Flight { get; set; }

        // Construtor - inicializa a coleção de passagens
        public Seat()
        {
            Tickets = new HashSet<Ticket>();
        }
    }
}