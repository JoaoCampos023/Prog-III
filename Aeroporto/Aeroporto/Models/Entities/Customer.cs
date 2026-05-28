using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.Entities
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(20)]
        [Display(Name = "Telefone")]
        public string Phone { get; set; }

        [StringLength(14)]
        [Display(Name = "CPF")]
        public string CPF { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Data de Nascimento")]
        public DateTime? BirthDate { get; set; }

        [StringLength(200)]
        [Display(Name = "Endereço")]
        public string Address { get; set; }

        [StringLength(100)]
        [Display(Name = "Cidade")]
        public string City { get; set; }

        [StringLength(2)]
        [Display(Name = "Estado")]
        public string State { get; set; }

        [StringLength(9)]
        [Display(Name = "CEP")]
        public string ZipCode { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;
    }
}