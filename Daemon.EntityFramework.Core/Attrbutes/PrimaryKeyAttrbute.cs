using System;
using System.Collections.Generic;
using System.Text;

namespace Daemon.EntityFramework.Core.Attrbutes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
    }
}
