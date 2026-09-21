using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocadoraVeiculos.Models
{
    [Table("Fabricantes")]
    public class Fabricante
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do fabricante é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        public string Nome { get; set; }

        [StringLength(50)]
        public string PaisOrigem { get; set; }

        public virtual ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }
}
