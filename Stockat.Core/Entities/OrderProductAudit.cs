using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.Entities;

public class OrderProductAudit
{
    public int Id { get; set; }
    public int OrderProductId { get; set; }
    public string UserId { get; set; }
    public DateTime ChangedAt { get; set; }

    public string OldRecordJson { get; set; }
    public string NewRecordJson { get; set; }

}

