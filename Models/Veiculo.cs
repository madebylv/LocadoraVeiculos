using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocadoraVeiculos.Models
{
    [Table("Veiculos")]
    public class Veiculo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O modelo é obrigatório.")]
        [StringLength(100)]
        public string Modelo { get; set; }

        [Required(ErrorMessage = "O ano de fabricação é obrigatório.")]
        public int AnoFabricacao { get; set; }

        [Required(ErrorMessage = "A quilometragem é obrigatória.")]
        public int Quilometragem { get; set; }

        [Required(ErrorMessage = "A placa é obrigatória.")]
        [StringLength(8)]
        public string Placa { get; set; }

        [StringLength(30)]
        public string Cor { get; set; }

        [Required(ErrorMessage = "O fabricante é obrigatório.")]
        [ForeignKey("Fabricante")]
        public int FabricanteId { get; set; }
        public virtual Fabricante Fabricante { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        [ForeignKey("Categoria")]
        public int CategoriaId { get; set; }
        public virtual Categoria Categoria { get; set; }

        public virtual ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
    }
}
