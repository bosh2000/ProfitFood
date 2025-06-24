using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.UI.ViewModels.Base
{
    internal abstract class ViewModel :INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        ~ViewModel()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
        }
        private bool _disposed;
        protected virtual void Dispose(bool disposing) 
        {
            if (!disposing || _disposed) return;
            _disposed = true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string? PropertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        }

    }
}
