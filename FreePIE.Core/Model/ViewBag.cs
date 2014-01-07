using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FreePIE.Core.Model
{
    [DataContract]
    public class ViewBag
    {
        [DataMember]
        private Dictionary<string, object> values = new Dictionary<string, object>();

        public T Get<T>(string key)
        {
            if (!values.ContainsKey(key))
                return default(T);

            return (T) values[key];
        }

        public void Set<T>(string key, T value)
        {
            values[key] = value;
        }
    }
}
