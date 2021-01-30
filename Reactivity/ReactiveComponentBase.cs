using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace blazor_client.Reactivity
{
    public class ReactiveComponentBase : ComponentBase, IDisposable
    {
        protected void RegisterReactiveObject(INotifyPropertyChanged value)
        {
            // -= here in case there are multiple
            value.PropertyChanged -= OnPropertyNotify;
            value.PropertyChanged += OnPropertyNotify;
        }

        protected void DeregisterReactiveObject(INotifyPropertyChanged value)
        {
            value.PropertyChanged -= OnPropertyNotify;
        }

        private void InternalSwapINotifyPropertyChangedUntyped(object old, object value)
        {
            InternalSwapINotifyPropertyChanged(old as INotifyPropertyChanged, value as INotifyPropertyChanged);
        }

        private void InternalSwapINotifyPropertyChanged(INotifyPropertyChanged old, INotifyPropertyChanged value)
        {
            if (old != null)
            {
                DeregisterReactiveObject(old);
            }

            if (value != null)
            {
                RegisterReactiveObject(value);
            }
        }

        public void Set<T>(ref T location, T value) where T : INotifyPropertyChanged
        {
            location = value;
            InternalSwapINotifyPropertyChanged(location, value);
            SafeEnqueueStateHasChanged();
        }

        public void Set<T>(string name, T value)
        {
            var type = GetType();
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
            {
                var current = field.GetValue(this);
                InternalSwapINotifyPropertyChangedUntyped(current, value);
                field.SetValue(this, value);
            }
            else if (property != null)
            {
                var current = property.GetValue(this);
                InternalSwapINotifyPropertyChangedUntyped(current, value);
                property.SetValue(this, value);
            }
            else
            {
                throw new ArgumentException($"Field or property {name} does not exist");
            }
            SafeEnqueueStateHasChanged();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            var type = GetType();
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!typeof(INotifyPropertyChanged).IsAssignableFrom(field.FieldType)) continue;

                var value = (INotifyPropertyChanged)field.GetValue(this);
                if (value != null)
                {
                    RegisterReactiveObject(value);
                }
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!typeof(INotifyPropertyChanged).IsAssignableFrom(property.PropertyType)) continue;
                if (property.GetMethod == null) continue;

                var value = (INotifyPropertyChanged)property.GetValue(this);
                if (value != null)
                {
                    RegisterReactiveObject(value);
                }
            }
        }

        private bool _isStateChanged = true;
        private int _propertyNotifyCounter = 0;
        protected void SafeEnqueueStateHasChanged()
        {
            var res = Interlocked.Increment(ref _propertyNotifyCounter);
            if (res == 1)
            {
                InvokeAsync(() =>
                {
                    int initialValue;
                    do
                    {
                        initialValue = _propertyNotifyCounter;
                        _isStateChanged = true;
                        StateHasChanged();
                        _isStateChanged = false;
                    } while (initialValue != Interlocked.CompareExchange(ref _propertyNotifyCounter, 0, initialValue));
                });
            }
        }

        private void OnPropertyNotify(object sender, PropertyChangedEventArgs e)
        {
            SafeEnqueueStateHasChanged();
        }

        protected override bool ShouldRender()
        {
            var res = base.ShouldRender() && _isStateChanged;
            Console.WriteLine($"shouldRender?  {res}");
            return res;
        }

        protected virtual void OnShutdown()
        {
            var type = GetType();
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!typeof(INotifyPropertyChanged).IsAssignableFrom(field.FieldType)) continue;

                var value = (INotifyPropertyChanged)field.GetValue(this);
                if (value != null)
                {
                    DeregisterReactiveObject(value);
                }
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!typeof(INotifyPropertyChanged).IsAssignableFrom(property.PropertyType)) continue;
                if (property.GetMethod == null) continue;

                var value = (INotifyPropertyChanged)property.GetValue(this);
                if (value != null)
                {
                    DeregisterReactiveObject(value);
                }
            }
        }

        public void Dispose()
        {
            OnShutdown();
        }
    }
}