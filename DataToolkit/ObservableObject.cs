using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace DataToolkit
{
    public class ObservableObject: INotifyPropertyChanged
    {
        private bool _hasChanged;

        public bool HasChanged
        {
            get { return _hasChanged; }
            set
            {
                Set(ref _hasChanged, value);
            }
        }

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propName);
                HasChanged = true;
                return true;
            }
            return false;
        }

        protected void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
