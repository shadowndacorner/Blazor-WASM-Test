using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using blazor_client.Reactivity;
using BlazorStrap;

namespace blazor_client.Services
{
    public class ToastManager
    {
        public class DisplayedToast
        {
            public string Header = "";
            public string Text = "";
            public DateTime EndTime;
        }

        private ReactiveList<DisplayedToast> _ActiveToasts = new ReactiveList<DisplayedToast>();
        public void ShowToast(string heading, string body, TimeSpan duration)
        {
            var toast = new DisplayedToast
            {
                Header = heading,
                Text = body,
                EndTime = DateTime.Now + duration
            };

            _ActiveToasts.Add(toast);

            Task.Run(()=>RemoveToast(toast, duration));
        }

        private async Task RemoveToast(DisplayedToast toast, TimeSpan delay)
        {
            await Task.Delay(delay);
            _ActiveToasts.Remove(toast);
        }

        public IEnumerable<DisplayedToast> Toasts => _ActiveToasts;
        public event PropertyChangedEventHandler ToastListener
        {
            add
            {
                _ActiveToasts.PropertyChanged += value;
            }
            remove
            {
                _ActiveToasts.PropertyChanged -= value;
            }
        }
    }
}
