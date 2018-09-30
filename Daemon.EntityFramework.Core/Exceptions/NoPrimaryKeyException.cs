using System;
using System.Collections.Generic;
using System.Text;

namespace Daemon.EntityFramework.Core.Exceptions
{
    public class NoPrimaryKeyException:Exception
    {
        public NoPrimaryKeyException(Type t)
               : base($"Type \"{t.FullName}\" Has No Primary Key.Please Use On PK Property") { }
    }
}
