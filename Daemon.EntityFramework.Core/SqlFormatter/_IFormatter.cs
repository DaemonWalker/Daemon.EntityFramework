using System;
using System.Collections.Generic;
using System.Text;

namespace Daemon.EntityFramework.Core.SqlFormatter
{
    public interface IFormatter
    {
        string Format(string source);
    }
}
