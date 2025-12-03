using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    using DistanceFunction = Func<int, int, int, int, int>;
    using SRand = System.Random;

    public static class Random
    {
        public static readonly SRand Ran = new();

        public static void FisherYates<T>(IList<T> arr)
        {
            for (var i = arr.Count - 1; i > 0; i--)
            {
                var j = Ran.Next(i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
        }

        private struct WeightedItem : IComparable<WeightedItem>
        {
            public int Idx;
            public float Key;

            // DESCENDING sort (Highest Key First)
            public int CompareTo(WeightedItem other) => other.Key.CompareTo(Key);
        }

        // Reference: https://en.wikipedia.org/wiki/Reservoir_sampling
        public static void WeightedShuffle(List<float> weights, ref int[] order)
        {
            var count = weights.Count;
            var items = new WeightedItem[count];

            for (var i = 0; i < count; i++)
            {
                var w = weights[i];

                var r = Ran.NextDouble();
                if (r == 0) r = 0.0000001;

                var key = (float)(Math.Log(r) / w);

                items[i] = new WeightedItem
                {
                    Idx = i,
                    Key = key
                };
            }

            Array.Sort(items);
            for (var i = 0; i < count; i++)
                order[i] = items[i].Idx;
        }
    }

    public static class Map
    {
        public static int Manhattan(int x1, int y1, int x2, int y2)
            => Math.Abs(x1 - x2) + Math.Abs(y1 - y2);

        public static int EuclideanSquared(int x1, int y1, int x2, int y2)
        {
            var dx = x1 - x2;
            var dy = y1 - y2;
            return dx * dx + dy * dy;
        }

        public static int Chebyshev(int x1, int y1, int x2, int y2)
            => Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));

        public static int[,] Voronoi(
            (int, int) dim,
            IEnumerable<(int type, int count)>
                freeTileSet,
            DistanceFunction distFunc,
            (int row, int col, int type)[] fixedTiles = null)
        {
            var (n, m) = dim;
            var map = new int[n, m];

            for (var i = 0; i < n; i++)
                for (var j = 0; j < m; j++)
                    map[i, j] = -1;

            var sources = new List<(int row, int col, int type)>();

            // Process fixed tiles if provided
            if (fixedTiles != null)
            {
                foreach (var (y, x, t) in fixedTiles)
                {
                    map[y, x] = t;
                    sources.Add((y, x, t));
                }
            }

            var allCells = new List<(int row, int col)>();
            for (var i = 0; i < n; i++)
            for (var j = 0; j < m; j++)
                if (map[i, j] == -1)
                    allCells.Add((i, j));

            allCells = allCells.OrderBy(_ => Random.Ran.Next()).ToList();

            // Place random seeds
            var chosenInd = 0;
            foreach (var (t, cnt) in freeTileSet)
            {
                for (var i = 0; i < cnt; i++)
                {
                    if (chosenInd >= allCells.Count) break;

                    var (y, x) = allCells[chosenInd++];
                    map[y, x] = t;
                    sources.Add((y, x, t));
                }
            }

            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < m; j++)
                {
                    if (map[i, j] != -1) continue;

                    var minDist = int.MaxValue;
                    foreach (var (y, x, t) in sources)
                    {
                        var currDist = distFunc(i, j, y, x);
                        if (currDist >= minDist) continue;
                        minDist = currDist;
                        map[i, j] = t;
                    }
                }
            }

            return map;
        }
    }
}
