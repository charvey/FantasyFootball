using FantasyFootball.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Core
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TResult> CrossJoin<T1, T2, TResult>(this IEnumerable<T1> source, IEnumerable<T2> second, Func<T1, T2, TResult> elementSelector)
        {
            foreach (var i1 in source)
            {
                foreach (var i2 in second)
                {
                    yield return elementSelector(i1, i2);
                }
            }
        }

        public static IEnumerable<TResult> CrossJoin<T1, T2, T3, TResult>(
            this IEnumerable<T1> source, IEnumerable<T2> second, IEnumerable<T3> third,
            Func<T1, T2, T3, TResult> elementSelector)
        {
            return source.SelectMany(i1 => second.SelectMany(i2 => third.Select(i3 => elementSelector(i1, i2, i3))));
        }

        public static IEnumerable<T> cons<T>(this T item, IEnumerable<T> list)
        {
            return new[] { item }.Concat(list);
        }

        public static IEnumerable<TSource> WhereMax<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (!source.Any()) return Enumerable.Empty<TSource>();
            var max = source.Max(selector);
            return source.Where(x => Equals(selector(x), max));
        }

        public static IEnumerable<Player> Except(this IEnumerable<Player> source, Player item)
        {
            return source.Where(x => x != item);
        }
    }
}
