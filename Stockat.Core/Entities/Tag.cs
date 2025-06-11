using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.Entities;

public class Tag
{
    public int Id { get; set; }

    public string Name { get; set; }

    // Navigation Properties 
    public virtual ICollection<ProductTag> ProductTags { get; set; }


}

public class ProductTag
{
    public int Id { get; set; }

    // ForeinKey
    public int TagId { get; set; }
    public int ProductId { get; set; }

    // Navigation Properties 
    public virtual Product Product { get; set; }
    public virtual Tag Tag { get; set; }
}
