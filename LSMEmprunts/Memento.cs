using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LSMEmprunts
{
    /// <summary>
    /// A implementation of the memento design pattern using introspection to get/restore all properties of the saved object
    /// </summary>
    class Memento<T>
    {
        private readonly Dictionary<PropertyInfo, object> _StoredProperties = new Dictionary<PropertyInfo, object>();

        public Memento(T originator)
        {
            var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite);

            foreach(var propertyInfo in propertyInfos)
            {
                _StoredProperties[propertyInfo] = propertyInfo.GetValue(originator);
            }
        }

        public void Restore(T originator)
        {
            foreach(var kvp in _StoredProperties)
            {
                kvp.Key.SetValue(originator, kvp.Value);
            }
        }

        public IEnumerable<string> SavedProperties => _StoredProperties.Keys.Select(e=>e.Name);
    }
}
