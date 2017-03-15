using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Eto.Forms;

// ReSharper disable AssignNullToNotNullAttribute

namespace LiteDB.Explorer.Core.Helpers
{
    /// <summary>
    /// The base model for all bindable models.
    /// </summary>
    public abstract class BaseModel : INotifyPropertyChanged
    {
        /// <summary>
        /// All the model values.
        /// </summary>
        protected readonly ConcurrentDictionary<string, object> Values = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Gets the value for a property.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="memberName">The name of the property.</param>
        protected T Get<T>([CallerMemberName]string memberName = null)
        {
            object returnValue;
            return Values.TryGetValue(memberName, out returnValue) ? (T)returnValue : default(T);
        }

        /// <summary>
        /// Gets the value for a property.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="value">The property value.</param>
        /// <param name="memberName">The name of the property.</param>
        protected void Set<T>(T value, [CallerMemberName]string memberName = null)
        {
            Values[memberName] = value;
            Application.Instance.Invoke(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName)));
        }

        /// <summary>
        /// The event handler for property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
