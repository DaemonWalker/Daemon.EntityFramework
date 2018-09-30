using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Daemon.EntityFramework.Core.Utils
{
    public static class ExtensionMethod
    {
        public static T DeepCopy<T>(this T t)
        {
            //var bin = new BinaryFormatter();
            //var ms = new MemoryStream();
            //bin.Serialize(ms, t);
            //ms.Position = 0;
            //return (T)bin.Deserialize(ms);
            var newT = Activator.CreateInstance<T>();
            foreach (var prop in typeof(T).GetProperties().Where(p => p.CanWrite))
            {
                prop.SetValue(newT, prop.GetValue(t));
            }
            return newT;
        }
    }
}
