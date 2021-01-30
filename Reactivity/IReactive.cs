using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace blazor_client.Reactivity
{
    public interface IReactive
    {
        public virtual void InitializeReactivity(IReactive parent)
        {
            var type = this.GetType();
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var value = (IReactive)field.GetValue(this);
                if (value != null)
                {
                    value.InitializeReactivity(this);
                }
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (property.GetMethod == null) continue;

                var value = (IReactive)property.GetValue(this);
                if (value != null)
                {
                    value.InitializeReactivity(this);
                }
            }
        }

        public void NotifyUpdate<T>(object sender, T argument);
    }
}