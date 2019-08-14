using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.GeorgianCard
{
    public enum ExtendedResultCode
    {
        PAYMENT_REJECTED = -2,
        PAYMENT_FAILED = -3,
        PAYMENT_REVERSED = -4,
        OK = 0,
        ONLINE_RP_FAILED = 1,
        CPA_REJECTED = 2,
        PREAUTHORIZE_OK = 3,
        CPA_FAILED = 4,
        CS_NOTSUPPORTED = 11,
        CS_LIMITEXCEEDED = 12,
        CS_CARDNOTFOUND = 13,
        CLIENT_LOST = 53,
        USER_CANCEL = 54
    }
}
