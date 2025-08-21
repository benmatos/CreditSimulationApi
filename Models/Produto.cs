using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreditsimulacaoApi.Models
{
    [Table("PRODUTO")]
    public class Produto
    {
        [Key]
        [Column("CO_PRODUTO")]
        public int CoProduto { get; set; }

        [Required]
        [Column("NO_PRODUTO")]
        [StringLength(200)]
        public string NoProduto { get; set; }

        [Required]
        [Column("PC_TAXA_JUROS")]
        public decimal PcTaxaJuros { get; set; }

        [Required]
        [Column("NU_MINIMO_MESES")]
        public short NuMinimoMeses { get; set; }

        [Column("NU_MAXIMO_MESES")]
        public short? NuMaximoMeses { get; set; }

        [Required]
        [Column("VR_MINIMO")]
        public decimal VrMinimo { get; set; }

        [Column("VR_MAXIMO")]
        public decimal? VrMaximo { get; set; }
    }
}
