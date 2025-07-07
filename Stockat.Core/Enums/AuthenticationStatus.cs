using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.Enums;

public enum AuthenticationStatus
{
    Success,
    InvalidCredentials,
    EmailNotConfirmed,
    AccountDeleted, // soft-deleted
    Blocked
}
