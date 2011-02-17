using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PixelLab.Common
{
    public class PathWatcher<TSource, TResult> : IDynamicValue<TResult>
    {
        public PathWatcher(TSource source, Expression<Func<TSource, TResult>> expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            var memberExpressions = new List<MemberExpression>();

            var memberExpression = expression.Body;

            while (memberExpression is MemberExpression)
            {
                var me = (MemberExpression)memberExpression;
                memberExpressions.Add(me);
                memberExpression = me.Expression;
            }

            var paramExpression = (ParameterExpression)memberExpression;
            memberExpressions.Reverse();

            m_getMethods = memberExpressions
              .Select(me => me.Member)
              .Cast<PropertyInfo>()
              .Select(pi => pi.GetGetMethod())
              .ToReadOnlyCollection();

            m_source = source;
            m_path = memberExpressions.Select(me => me.Member.Name).ToReadOnlyCollection();
            if (m_getMethods.Last().ReturnType != typeof(TResult))
            {
                throw new ArgumentException("The provided property path does not yield the desired result type.", "propertyPath");
            }

            m_values = new object[m_getMethods.Count];
            populateValues(false);
        }

        public TResult Value { get { return m_value; } }

        public event EventHandler ValueChanged;

        protected virtual void OnValueChanged(EventArgs e = null)
        {
            var handler = ValueChanged;
            if (handler != null)
            {
                handler(this, e ?? EventArgs.Empty);
            }
        }

        private void object_propertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var value = m_watching.Where(p => p.Item1 == sender && p.Item2 == e.PropertyName);
            if (!value.IsEmpty())
            {
                Contract.Assume(value.Count() == 1);
                populateValues(true);
            }
        }

        private void populateValues(bool notifyChange)
        {
            object value = m_source;
            m_getMethods.CountForEach((mi, i) =>
            {
                ensureWatching(value, m_path[i]);
                value = m_getMethods[i].Invoke(value, null);
                if (m_values[i] != value)
                {
                    for (int j = i; j < (m_getMethods.Count - 1); j++)
                    {
                        stopWatching(m_values[j], m_path[j + 1]);
                        m_values[j] = null;
                    }
                    m_values[i] = value;

                    if (i == (m_path.Count - 1))
                    {
                        m_value = (TResult)value;
                        if (notifyChange)
                        {
                            OnValueChanged();
                        }
                    }
                }
            });
        }

        private void ensureWatching(object source, string propertyName)
        {
            if (!m_watching.Any(p => p.Item1 == source && p.Item2 == propertyName))
            {
                if (source is INotifyPropertyChanged)
                {
                    var inpc = (INotifyPropertyChanged)source;
                    inpc.PropertyChanged += object_propertyChanged;
                    m_watching.Add(new Tuple<object, string>(source, propertyName));
                }
            }
        }

        private void stopWatching(object source, string propertyName)
        {
            if (source != null)
            {
                var match = m_watching.Where(p => p.Item1 == source && p.Item2 == propertyName).First();
                if (match.Item1 is INotifyPropertyChanged)
                {
                    var inpc = (INotifyPropertyChanged)match.Item1;
                    inpc.PropertyChanged -= object_propertyChanged;
                    m_watching.Remove(match);
                }
            }
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(m_source != null);
            Contract.Invariant(m_path != null);
            Contract.Invariant(m_getMethods != null);
            Contract.Invariant(Contract.ForAll(m_getMethods, gm => gm != null));
            Contract.Invariant(m_values != null);
            Contract.Invariant(m_watching != null);
            Contract.Invariant(Contract.ForAll(m_watching, w => w != null));
        }

        private readonly TSource m_source;
        private TResult m_value;
        private readonly ReadOnlyCollection<string> m_path;
        private readonly ReadOnlyCollection<MethodInfo> m_getMethods;
        private readonly object[] m_values;
        private readonly List<Tuple<object, string>> m_watching = new List<Tuple<object, string>>();
    }
}
