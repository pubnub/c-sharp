using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public enum PNStatusCategory
    {
        PNUnknownCategory,
        PNAcknowledgmentCategory,
        PNAccessDeniedCategory,
        PNTimeoutCategory,
        PNNetworkIssuesCategory,
        PNConnectedCategory,
        PNReconnectedCategory,
        PNDisconnectedCategory,
        PNUnexpectedDisconnectCategory,
        PNCancelledCategory,
        PNBadRequestCategory,
        PNMalformedFilterExpressionCategory,
        PNMalformedResponseCategory,
        PNDecryptionErrorCategory,
        PNTLSConnectionFailedCategory,
        PNTLSUntrustedCertificateCategory,
        PNRequestMessageCountExceededCategory
    }
}
