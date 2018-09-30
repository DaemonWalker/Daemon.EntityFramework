using System;
using System.Collections.Generic;
using System.Text;

namespace Daemon.EntityFramework.Core.Utils
{
    public class GlobalMethod
    {
        public static Type GetClassGenericType(Type t)
        {
            if (t.IsGenericType)
            {
                return t.GenericTypeArguments[0];
            }
            else
            {
                return GetClassGenericType(t.BaseType);
            }
        }
    }
}
