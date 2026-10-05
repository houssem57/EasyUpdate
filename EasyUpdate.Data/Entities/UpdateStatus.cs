using System;
using System.Collections.Generic;
using System.Text;

namespace EasyUpdate.Data.Entities
{
    public enum UpdateStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        RolledBack,
        Cancelled
    }
}
