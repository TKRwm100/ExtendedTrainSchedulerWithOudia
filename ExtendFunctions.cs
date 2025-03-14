using BveEx.Extensions.MapStatements;
using BveTypes.ClassWrappers;
using BveTypes.ClassWrappers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BveEx.Toukaitetudou.ExtendedTrainSchedulerWithOudia
{
    internal static class ExtendFunctions
    {
        public static SortedList<TKey,TValue> ToSortedList<TEmuretable,TKey,TValue>(this IEnumerable<TEmuretable> values,Func<TEmuretable,TKey>keySelector,Func<TEmuretable,TValue>valueSelector)
        {
            return new SortedList<TKey, TValue> (values.ToDictionary (keySelector,valueSelector));
        }
        public static void AddRange<TValue>(this WrappedList<TValue> values,IEnumerable<TValue> collection)
        {
            foreach (var item in collection)
            {
                values.Add(item);
            }
        }
        public static void Foreach<TEmuretable>(this IEnumerable<TEmuretable> values, Action<TEmuretable> action)
        {
            foreach (TEmuretable item in values)
                action.Invoke(item);
        }

        public static string ToTrainKey(this string trainnumber)
        {
            return $"{nameof(ExtendedTrainSchedulerWithOudia)}_{trainnumber}";
        }
    }
}
