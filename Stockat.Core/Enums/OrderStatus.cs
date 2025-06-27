using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.Enums
{
public enum OrderStatus
{
    PendingSeller,
    PendingBuyer,
    Processing,
    Ready,
    Pending,
    Shipped,
    Completed,
    Cancelled,
    PaymentFailed,
    Delivered
    }
}
