using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace blazor_client
{
    public class BaseNotifyPropertyChanged : INotifyPropertyChanged
    {
        protected void NotifyStateChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void NotifyStateChanged<T>(ref T target, T value, [CallerMemberName] string propertyName = "")
        {
            target = value;
            NotifyStateChanged(propertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
