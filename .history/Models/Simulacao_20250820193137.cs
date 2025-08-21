using System;
using System.Data.SQLite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreditsimulacaoApi.Models
{
    [Table("SIMULACAO")]
    public class Simulacao
    {
        [Key]
        public int idSimulacao { get; set; }

        [Required]
        public decimal valorDesejado { get; set; }

        [Required]
        public int idProduto { get; set; }

        [Required]
        public int noProduto { get; set; }

        [Required]
        public int prazo { get; set; }

        [Required]
        public DateTime DataSimulacao { get; set; }

        [Required]
        public decimal valorTotalParcelas { get; set; }

    }
}
