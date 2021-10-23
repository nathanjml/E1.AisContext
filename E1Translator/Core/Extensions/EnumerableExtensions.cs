using System;
using System.Collections.Generic;
using System.Linq;

namespace E1Translator.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        public static TReturn[] ForEach<T, TReturn>(this IEnumerable<T> source, Func<T, TReturn> func)
        {
            return source.Select(func).ToArray();
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            var index = 0;

            foreach (var item in source)
                action(item, index++);
        }

        public static TReturn[] ForEach<T, TReturn>(this IEnumerable<T> source, Func<T, int, TReturn> func)
        {
            return source.Select(func).ToArray();
        }

        public static int IndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            var index = 0;

            foreach (var value in enumerable)
            {
                if (predicate(value))
                    return index;

                index++;
            }

            return -1;
        }

        public static IEnumerable<TFirst> Except<TFirst, TSecond>(
            this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second,
            Func<TFirst, TSecond, bool> comparer)
        {
            return first.Where(x => second.Count(y => comparer(x, y)) == 0);
        }

        public static IEnumerable<TSource> Intersect<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            Func<TSource, TSource, bool> comparer)
        {
            return first.Where(x => second.Count(y => comparer(x, y)) == 1);
        }

        public static (List<TResult>, List<TResult>) Partition<T, TResult>(this IEnumerable<T> source, Func<T, bool> func, Func<T, TResult> selector)
        {
            var left = new List<TResult>();
            var right = new List<TResult>();

            source.ForEach(x =>
            {
                if (func(x))
                    left.Add(selector(x));
                else
                    right.Add(selector(x));
            });

            return (left, right);
        }

        public static (List<T>, List<T>) Partition<T>(this IEnumerable<T> source, Func<T, bool> func)
            => source.Partition(func, x => x);

        public static IEnumerable<T> SortTopologically<T>(this IEnumerable<T> nodes,
            Func<T, IEnumerable<T>> dependencies)
        {
            var elems = nodes.ToDictionary(node => node,
                node => new HashSet<T>(dependencies(node)));

            while (elems.Count > 0)
            {
                var elem = elems.FirstOrDefault(x => x.Value.Count == 0);

                if (elem.Key == null)
                {
                    throw new ArgumentException("Cyclic dependency found.");
                }

                elems.Remove(elem.Key);

                foreach (var nodeSet in elems)
                {
                    nodeSet.Value.Remove(elem.Key);
                }

                yield return elem.Key;
            }
        }

        public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
        {
            TSource[] bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                    bucket = new TSource[size];

                bucket[count++] = item;
                if (count != size)
                    continue;

                yield return bucket;

                bucket = null;
                count = 0;
            }

            if (bucket != null && count > 0)
                yield return bucket.Take(count).ToArray();
        }
    }
}
