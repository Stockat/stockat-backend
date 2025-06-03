using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.Entities;

public class Feature
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Feature Name is Required")]
    [MinLength(5, ErrorMessage = "Feature Name Length Must Be Greater than or equal 5 char")]
    [MaxLength(50, ErrorMessage = "Feature Name Length Must Be less than or equal 50 char")]
    public string Name { get; set; } = string.Empty;

    // ForeinKey

    [Required(ErrorMessage = "Product Id is Required")]
    public int ProductId { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; }
    public virtual ICollection<FeatureValue> FeatureValues { get; set; }
    public virtual ICollection<StockDetails> StockDetails { get; set; }
}

public class FeatureValue
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Feature Value is Required")]
    [MinLength(5, ErrorMessage = "Feature Value Length Must Be Greater than or equal 5 char")]
    [MaxLength(50, ErrorMessage = "Feature Value Length Must Be less than or equal 50 char")]
    public string Value { get; set; } = string.Empty;

    // ForeinKey
    [Required(ErrorMessage = "Feature Id is Required")]
    public int FeatureId { get; set; }

    // Navigation Properties
    public virtual Feature Feature { get; set; }
    public virtual ICollection<StockDetails> StockDetails { get; set; }

}
