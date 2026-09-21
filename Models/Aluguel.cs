using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocadoraVeiculos.Models
{
    [Table("Alugueis")]
    public class Aluguel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O cliente é obrigatório.")]
        [ForeignKey("Cliente")]
        public int ClienteId { get; set; }
        public virtual Cliente Cliente { get; set; }

        [Required(ErrorMessage = "O veículo é obrigatório.")]
        [ForeignKey("Veiculo")]
        public int VeiculoId { get; set; }
        public virtual Veiculo Veiculo { get; set; }

        [Required(ErrorMessage = "A data de início é obrigatória.")]
        public DateTime DataInicio { get; set; }

        [Required(ErrorMessage = "A data prevista para devolução é obrigatória.")]
        public DateTime DataPrevistaDevolucao { get; set; }

        public DateTime? DataDevolucao { get; set; }

        [Required(ErrorMessage = "A quilometragem inicial é obrigatória.")]
        public int QuilometragemInicial { get; set; }

        public int? QuilometragemFinal { get; set; }

        [Required(ErrorMessage = "O valor da diária é obrigatório.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorDiaria { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValorTotal { get; set; }
    }
}
