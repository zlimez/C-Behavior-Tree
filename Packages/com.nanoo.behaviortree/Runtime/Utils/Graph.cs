using System.Collections.Generic;

namespace Utils
{
    public class Graph
    {
        public static bool Kahn(List<(int, int)>[] adj, ref int[] sortedVertices)
        {
            var inDegs = new int[adj.Length];
            foreach (var children in adj)
                foreach (var (v, _) in children)
                    inDegs[v]++;

            Queue<int> frontier = new();
            for (var i = 0; i < inDegs.Length; i++)
                if (inDegs[i] == 0)
                    frontier.Enqueue(i);
            var j = 0;
            while (frontier.Count > 0)
            {
                var curr = frontier.Dequeue();
                sortedVertices[j++] = curr;
                foreach (var (child, _) in adj[curr])
                {
                    if (--inDegs[child] == 0)
                        frontier.Enqueue(child);
                }
            }
            return j == adj.Length;
        }
    }
}
