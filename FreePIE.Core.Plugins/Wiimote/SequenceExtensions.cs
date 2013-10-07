using System;
using System.Collections.Generic;
using System.Linq;

namespace FreePIE.Core.Plugins.Wiimote
{
    public static class SequenceExtensions
    {
        public static IEnumerable<double> Deltas(this IEnumerable<double> values)
        {
            return values.AdjacentElements().Select(x => x.Item2 - x.Item1);
        }

        public static IEnumerable<Tuple<T, T>> AdjacentElements<T>(this IEnumerable<T> values)
        {
            var previous = values.FirstOrDefault();

            if (default(double).Equals(previous))
                yield break;

            foreach (var value in values.Skip(1))
            {
                yield return new Tuple<T, T>(previous, value);
                previous = value;
            }
        }
    }
}