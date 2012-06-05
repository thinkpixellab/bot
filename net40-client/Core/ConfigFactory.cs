#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.ComponentModel;

namespace PixelLab.Common
{
    public class ConfigFactory<T> : IEqualityComparer<T>
    {
        private static ConfigFactory<T> _instance;

        // the names of the properties of the class, and their types
        private readonly IDictionary<string, Tuple<PropertyInfo, object>> _targetTypes;

        // these are the names of the properties of this class
        // but in the order of the paramaters in the ctor
        private static ReadOnlyCollection<string> _ctorParamMap;

        private ConfigFactory()
        {
            // things that must be true
            // only one ctor
            // for every param in the ctor, there must be one public property that
            // 1) matches the name of the param name (invariant culture, ignore-case)
            // 2) matches the type of the param (exactly)
            // 3) has a default value defined (via [DefaultValue] attribute)
            // 4) if the default value is not a valid constant value for an attribute, it must be convertable from the provided value

            // will fail if we can't convert the DefaultValueAttribute value to the target type
            _targetTypes = (from prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            where prop.CanRead // must be readable
                            where prop.GetIndexParameters().Length == 0 // no index properties
                            let attr = prop.GetCustomAttributes<DefaultValueAttribute>(false).SingleOrDefault()
                            where attr != null // must have a default value
                            let value = convert(attr.Value, prop.PropertyType)
                            select new { prop, value }).ToDictionary(a => a.prop.Name, a => Tuple.Create(a.prop, a.value));

            // wil fail if more than one ctor
            var ctor = typeof(T).GetConstructors().Single();
            var ctorMap = new List<string>();
            foreach (var param in ctor.GetParameters())
            {
                // will fail unless onle one matching property exists
                var prop = (from kvp in _targetTypes
                            where kvp.Key.Equals(param.Name, StringComparison.InvariantCultureIgnoreCase)
                            select kvp.Value.Item1).Single();

                Util.ThrowUnless<InvalidOperationException>(prop.PropertyType == param.ParameterType);
                ctorMap.Add(prop.Name);
            }
            _ctorParamMap = ctorMap.ToReadOnlyCollection();
        }

        /// <remarks>Not thread safe. Well, at least I don't make any promises.</remarks>
        public static ConfigFactory<T> Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConfigFactory<T>();
                }
                return _instance;
            }
        }

        public T CreateDefaultInstance()
        {
            var sourceDefaultValues = _targetTypes.ToDictionary(t => t.Key, t => t.Value.Item2);
            return CreateInstance(sourceDefaultValues);
        }

#if !SILVERLIGHT
        public T CreateInstance(System.Collections.Specialized.NameValueCollection nameValueCollection)
        {
            Contract.Requires(nameValueCollection != null);
            var values = (from name in nameValueCollection.AllKeys
                          let value = nameValueCollection.Get(name)
                          select Tuple.Create(name, (object)value)).ToDictionary(a => a.Item1, a => a.Item2);
            return CreateInstance(values);
        }
#endif

        public T CreateInstance(IDictionary<string, object> values)
        {
            var ctorParams = new object[_ctorParamMap.Count];
            var index = 0;
            foreach (var paramName in _ctorParamMap)
            {
                object val;
                if (values.TryGetValue(paramName, out val))
                {
                    ctorParams[index++] = convert(val, _targetTypes[paramName].Item1.PropertyType);
                }
                else
                {
                    throw new ArgumentException("missing required key {0}".DoFormat(paramName), "values");
                }
            }

            return (T)typeof(T).CreateInstance(ctorParams);
        }

        // TODO: a clever way to see if the object already supports this? can cache this?
        public IDictionary<string, object> GetValues(T instance)
        {
            Contract.Requires(instance != null);
            var _values = new Dictionary<string, object>(_targetTypes.Count);
            foreach (var entry in _targetTypes)
            {
                _values[entry.Key] = entry.Value.Item1.GetValue(instance, null);
            }
            return _values;
        }

        public bool Equals(T x, T y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return true;
                }
                return false;
            }
            else if (y == null)
            {
                return false;
            }
            var xValues = getKeyOrderedValues(GetValues(x));
            var yValues = getKeyOrderedValues(GetValues(y));
            return Enumerable.SequenceEqual(xValues, yValues);
        }

        public int GetHashCode(T obj)
        {
            if (obj == null)
            {
                return 0;
            }
            var values = getKeyOrderedValues(GetValues(obj));
            return Util.GetHashCode(values.ToArray());
        }

        private static IEnumerable<object> getKeyOrderedValues(IDictionary<string, object> values)
        {
            return values.OrderBy(kvp => kvp.Key, CultureInfo.InvariantCulture.CompareInfo.ToComparer<string>()).Select(kvp => kvp.Value);
        }

        private static IDictionary<string, object> convert(IDictionary<string, object> source, Func<string, Type> map)
        {
            var newPairs = from pair in source
                           let targetType = map(pair.Key)
                           select new { name = pair.Key, converterValue = convert(pair.Value, targetType) };

            return newPairs.ToDictionary(p => p.name, p => p.converterValue);
        }

        private static object convert(object value, Type targetType)
        {
            Util.ThrowUnless(value != null);
            Util.ThrowUnless(targetType != null);

            if (!targetType.IsAssignableFrom(value.GetType()))
            {
                if (value is string && targetType == typeof(Uri))
                {
                    return new Uri((string)value);
                }
                else
                {
                    return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                }
            }
            return value;
        }
    }
}
