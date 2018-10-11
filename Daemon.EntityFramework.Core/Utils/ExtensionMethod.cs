using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        public static bool IsAnonymousType(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            // HACK: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }
    }
}
