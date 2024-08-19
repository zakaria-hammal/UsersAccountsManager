using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projet.Models
{
    public class Product
    {
        public int ProductID {get; set;}

        [Required]
        [StringLength(40)]
        public string? ProductName {get; set;}

        [Column("UnitPrice", TypeName = "Money")]
        public double? Cost {get; set;}

        [Column("UnitsInStock")]
        public short? Stock {get; set;}

        public bool Discontinued {get; set;}

        public int CategoryID {get; set;}

    }
}
