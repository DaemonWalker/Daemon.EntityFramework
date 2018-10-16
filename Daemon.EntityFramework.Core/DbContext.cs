using System;
using System.Collections.Generic;
using System.Text;

namespace Daemon.EntityFramework.Core
{
    public abstract class DBContext : IDisposable
    {
        public DefSettings DefSettings { get; set; }
        private List<IDisposable> tables = new List<IDisposable>();
        public DBContext(DefSettings defSettings)
        {
            var type = this.GetType();
            foreach (var prop in type.GetProperties())
            {
                if (prop.PropertyType.ToString().Contains("DBTable"))
                {
                    if (prop.GetValue(this) == null)
                    {
                        prop.SetValue(this, Activator.CreateInstance(prop.PropertyType));
                    }
                    var obj = prop.GetValue(this);
                    tables.Add(obj as IDisposable);
                    obj.GetType().GetProperty("DefSettings").SetValue(obj, defSettings);
                }
            }
        }

        public void Dispose()
        {
            tables.ForEach(p => p.Dispose());
        }
    }
}
