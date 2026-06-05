using System.ComponentModel.DataAnnotations;

namespace SistemaAereo.Models.Entities
{
    // Representa um cliente preferencial do sistema
    public class Customer
    {
        // Identificador único do cliente
        [Key]
        public int CustomerId { get; set; }

        // Nome completo do cliente
        [Required]
        [StringLength(100)]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        // Email do cliente (único)
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        // Telefone para contato
        [StringLength(20)]
        [Display(Name = "Telefone")]
        public string Phone { get; set; }

        // CPF do cliente (único)
        [StringLength(14)]
        [Display(Name = "CPF")]
        public string CPF { get; set; }

        // Data de nascimento do cliente
        [DataType(DataType.Date)]
        [Display(Name = "Data de Nascimento")]
        public DateTime? BirthDate { get; set; }

        // =============================================
        // PROPRIEDADES DE ENDEREÇO
        // =============================================

        // Endereço completo (rua, número, complemento)
        [StringLength(200)]
        [Display(Name = "Endereço")]
        public string Address { get; set; }

        // Cidade onde o cliente reside
        [StringLength(100)]
        [Display(Name = "Cidade")]
        public string City { get; set; }

        // Estado (UF) onde o cliente reside
        [StringLength(2)]
        [Display(Name = "Estado")]
        public string State { get; set; }

        // Código postal (CEP)
        [StringLength(9)]
        [Display(Name = "CEP")]
        public string ZipCode { get; set; }

        // =============================================
        // PROPRIEDADES DE CONTROLE
        // =============================================

        // Data em que o cliente foi cadastrado
        [Display(Name = "Data de Cadastro")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // Indica se o cliente está ativo no sistema
        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;

        // Sexo do cliente (M, F, O)
        [StringLength(1)]
        [Display(Name = "Sexo")]
        public string Gender { get; set; }
    }
}