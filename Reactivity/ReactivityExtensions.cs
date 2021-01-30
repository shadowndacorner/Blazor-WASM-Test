using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blazor_client.Reactivity
{
    public static class ReactivityExtensions
    {
        public static T Notify<T>(this IReactive reactive, T value)
        {
            reactive.NotifyUpdate(reactive, value);
            return value;
        }
    }
}
