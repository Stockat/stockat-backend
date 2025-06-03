using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.Entities;

public class Stock
{
    [Required(ErrorMessage = "Stock Id is Required")]
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Product Id is Required")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Quantity Id is Required")]
    [Range(1, int.MaxValue, ErrorMessage = $"Quantity must be between 1:2147483647")]
    public int Quantity { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; }
    public virtual ICollection<StockDetails> StockDetails { get; set; }


}
public class StockDetails
{
    [Key]
    public int Id { get; set; }

    // ForeinKey

    [Required(ErrorMessage = "Stock Id is Required")]
    public int StockId { get; set; }

    [Required(ErrorMessage = "Feature Id is Required")]
    public int FeatureId { get; set; }

    [Required(ErrorMessage = "Feature Value Id is Required")]
    public int FeatureValueId { get; set; }

    // Navigation Properties
    public virtual Stock Stock { get; set; }
    public virtual FeatureValue FeatureValue { get; set; }
    public virtual Feature Feature { get; set; }


}
