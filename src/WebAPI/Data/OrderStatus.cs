using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Data
{
    public enum OrderStatus
    {
        Pending,
        PendingPayment,
        Processing,
        Completed,
        Failed
    }
}
