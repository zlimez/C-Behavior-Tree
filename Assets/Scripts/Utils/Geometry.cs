using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public readonly struct Poly
    {
        public readonly Vector2[] Vertices;
        public readonly int VtxCnt;

        public Poly(Vector2[] vertices, int vtxCnt)
        {
            Vertices = vertices;
            VtxCnt = vtxCnt;
        }

        public Vector2 Center()
        {
            var center = Vector2.zero;
            for (var i = 0; i < VtxCnt; i++) center += Vertices[i];
            return center / VtxCnt;
        }

        public void Project(Vector2 axis, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;
            for (var i = 0; i < VtxCnt; i++)
            {
                var projection = Vector2.Dot(Vertices[i], axis);
                min = Mathf.Min(min, projection);
                max = Mathf.Max(max, projection);
            }
        }

        public static void Transform(Vector2 translation, Vector2 scale, float rotation,
            Vector2[] poly, int vertexCount)
        {
            var rotMat = Matrix4x4.TRS(
                new Vector3(translation.x, translation.y),
                Quaternion.Euler(0, 0, rotation),
                new Vector3(scale.x, scale.y, 1));

            for (var i = 0; i < vertexCount; i++)
            {
                var v = poly[i];
                poly[i] = rotMat.MultiplyPoint(v);
            }
        }

        public static void BoxVertices(float width, float height, Vector2[] vertices)
        {
            vertices[0] = new Vector2(width / 2, height / 2);
            vertices[1] = new Vector2(-width / 2, height / 2);
            vertices[2] = new Vector2(-width / 2, -height / 2);
            vertices[3] = new Vector2(width / 2, -height / 2);
        }
    }

    public struct Circle
    {
        public readonly Vector2 Center;
        public readonly float Radius;

        public Circle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public static Circle Transform(Vector2 center, float radius, Vector2 translation, Vector2 scale) =>
            new(center * scale + translation, radius * Mathf.Max(scale.x, scale.y));
    }

    public static class CollisionDetection
    {
        // Moving circleA along normal resolves the collision
        public static bool Circles(Circle circA, Circle circB,
            out Vector2 normal, out float depth, float epsilon = 0.001f)
        {
            normal = circA.Center - circB.Center;
            float sqd = normal.sqrMagnitude, radSum = circA.Radius + circB.Radius;

            if (sqd >= radSum * radSum - epsilon * epsilon)
            {
                depth = 0;
                return false;
            }

            var distance = Mathf.Sqrt(sqd);
            depth = radSum - distance;
            if (distance > 0) normal /= distance; // Normalize the normal
            return true;
        }

        // Moving circle A along normal resolves the collision
        public static bool CirclePoly(Circle circ, Poly poly,
            out Vector2 normal, out float depth, float epsilon = 0.001f)
        {
            normal = Vector2.zero;
            depth = float.MaxValue;

            for (var i = 0; i < poly.VtxCnt; i++)
            {
                Vector2 a = poly.Vertices[i], b = poly.Vertices[(i + 1) % poly.VtxCnt];
                Vector2 edge = b - a, axis = new Vector2(-edge.y, edge.x).normalized;

                poly.Project(axis, out var minB, out var maxB);
                var circleProjection = Vector2.Dot(circ.Center, axis);
                float circleMin = circleProjection - circ.Radius, circleMax = circleProjection + circ.Radius;

                if (maxB - epsilon < circleMin || circleMax - epsilon < minB) return false;

                var overlap = Mathf.Min(maxB - circleMin, circleMax - minB);
                if (!(overlap < depth)) continue;
                depth = overlap;
                normal = axis * (Vector2.Dot(circ.Center - a, axis) < 0 ? -1 : 1);
            }

            Vector2 polyCenter = poly.Center(), circleAxis = (circ.Center - polyCenter).normalized;

            poly.Project(circleAxis, out var polyMin, out var polyMax);
            var circleProj = Vector2.Dot(circ.Center, circleAxis);
            float cMin = circleProj - circ.Radius, cMax = circleProj + circ.Radius;

            if (polyMax - epsilon < cMin || cMax - epsilon < polyMin) return false;

            var circleOverlap = Mathf.Min(polyMax - cMin, cMax - polyMin);
            if (!(circleOverlap < depth)) return true;
            depth = circleOverlap;
            normal = circleAxis;

            return true;
        }

        // Moving polyA along normal resolves the collision
        public static bool Polys(Poly polyA, Poly polyB,
            out Vector2 normal, out float depth, float epsilon = 0.001f)
        {
            normal = Vector2.zero;
            depth = float.MaxValue;
            Vector2 centerA = polyA.Center(), centerB = polyB.Center();
            var collides = FindOverlapOnAxes(polyA, polyB, ref normal, ref depth, epsilon) &&
                FindOverlapOnAxes(polyB, polyA, ref normal, ref depth, epsilon, true);
            if (collides && Vector2.Dot(normal, centerA - centerB) < 0) normal *= -1;
            return collides;
        }

        private static bool FindOverlapOnAxes(Poly polyA, Poly polyB,
            ref Vector2 normal, ref float depth, float epsilon, bool rev = false)
        {
            for (var i = 0; i < polyA.VtxCnt; i++)
            {
                Vector2 a = polyA.Vertices[i], b = polyA.Vertices[(i + 1) % polyA.VtxCnt];
                Vector2 edge = b - a, axis = new Vector2(-edge.y, edge.x).normalized;

                polyA.Project(axis, out var minA, out var maxA);
                polyB.Project(axis, out var minB, out var maxB);

                if (maxA - epsilon < minB || maxB - epsilon < minA) return false;

                var overlap = Mathf.Min(maxA - minB, maxB - minA);
                if (!(overlap < depth)) continue;
                depth = overlap;
                normal = axis * (rev ? -1: 1);
            }
            return true;
        }
    }

    public struct Hit
    {
        public readonly Vector2 Point;
        public readonly Vector2 Normal;
        public readonly float Distance;

        public Hit(Vector2 point, Vector2 normal, float distance)
        {
            Point = point;
            Normal = normal;
            Distance = distance;
        }
    }

    public struct LineSeg
    {
        public readonly Vector2 Start, End;

        public LineSeg(Vector2 start, Vector2 end)
        {
            End = end;
            Start = start;
        }
    }

    public readonly struct Grid
    {
        public readonly float CellSize;
        public readonly (int x, int y) Dim;
        private readonly Vector2 _leftBottom;

        public Grid(Vector2 leftBottom, float cellSize, (int x, int y) dim)
        {
            _leftBottom = leftBottom;
            CellSize = cellSize;
            Dim = dim;
        }

        public readonly (int, int) GetCellFor(Vector2 pos)
        {
            pos -= _leftBottom;
            var x = Mathf.FloorToInt(pos.x / CellSize);
            var y = Mathf.FloorToInt(pos.y / CellSize);
            return (x, y);
        }

        public readonly Vector2 GetRawCellFor(Vector2 pos) => (pos - _leftBottom) / CellSize;

        public readonly bool IsIn(Vector2 pos)
        {
            var (x, y) = GetCellFor(pos);
            return x >= 0 && x < Dim.x && y >= 0 && y < Dim.y;
        }

        public readonly bool IsIn((int x, int y) cell) => cell.x >= 0 && cell.x < Dim.x && cell.y >= 0 && cell.y < Dim.y;

        public readonly Vector2 GetPosFor((int x, int y) cell) =>
            _leftBottom + new Vector2(cell.x * CellSize, cell.y * CellSize) + new Vector2(CellSize / 2, CellSize / 2);
    }

    public static class Raycaster
    {
        // Amanatides & Woo
        public static void GetRayCells(Grid grid, Vector2 origin, Vector2 dir,
            List<((int, int), float)> cells, float dist = float.MaxValue)
        {
            cells.Clear();
            if (!grid.IsIn(origin)) return;
            dir = dir.normalized;
            var rayGridOrigin = grid.GetRawCellFor(origin);
            (int x, int y) cCell = grid.GetCellFor(origin);
            (int x, int y) step = (dir.x > 0 ? 1 : -1, dir.y > 0 ? 1 : -1);

            var tMax = new Vector2(
                TMax(rayGridOrigin.x, dir.x, step.x),
                TMax(rayGridOrigin.y, dir.y, step.y)
            );

            var tDelta = new Vector2(
                grid.CellSize / Mathf.Abs(dir.x),
                grid.CellSize / Mathf.Abs(dir.y)
            );

            while (grid.IsIn(cCell))
            {
                cells.Add((cCell, Mathf.Min(tMax.x, tMax.y)));
                if (Mathf.Min(tMax.x, tMax.y) > dist) break;
                if (tMax.x < tMax.y)
                {
                    tMax.x += tDelta.x;
                    cCell.x += step.x;
                }
                else
                {
                    tMax.y += tDelta.y;
                    cCell.y += step.y;
                }
            }
        }

        private static float TMax(float gridCoord, float dir, int step, float epsilon = (float)1e-6)
        {
            if (Mathf.Abs(dir) < epsilon)
                return float.MaxValue;

            float nextBoundary;
            if (step > 0) nextBoundary = Mathf.Floor(gridCoord) + 1;
            else nextBoundary = Mathf.Ceil(gridCoord) - 1;
            return (nextBoundary - gridCoord) / dir;
        }

        public static Hit? Cast(Vector2 origin, Vector2 dir, Circle circle, float dist = float.MaxValue)
        {
            dir = dir.normalized;
            var toCirc = circle.Center - origin;

            float a = dir.sqrMagnitude, b = 2 * Vector2.Dot(dir, toCirc),
                c = toCirc.sqrMagnitude - circle.Radius * circle.Radius;
            var d = b * b - 4 * a * c;
            if (d < 0) return null;

            var sqrtD = Mathf.Sqrt(d);
            var t1 = (-b - sqrtD) / (2 * a);
            var t2 = (-b + sqrtD) / (2 * a);
            var t = Mathf.Min(t1, t2);
            if (t < 0 || t > dist) return null;
            var hitPoint = origin + dir * t;
            var hitNormal = (hitPoint - circle.Center).normalized;
            return new Hit(hitPoint, hitNormal, t);
        }

        public static Hit? Cast(Vector2 origin, Vector2 dir, Poly poly, float dist = float.MaxValue)
        {
            var minDist = float.MaxValue;
            var minNormal = Vector2.zero;
            var minPoint = Vector2.zero;
            var hasHit = false;

            for (var i = 0; i < poly.VtxCnt; i++)
            {
                Vector2 a = poly.Vertices[i], b = poly.Vertices[(i + 1) % poly.VtxCnt];
                var line = new LineSeg(a, b);
                var hit = Cast(origin, dir, line, dist);
                if (hit == null) continue;
                hasHit = true;
                if (!(hit.Value.Distance < minDist)) continue;
                minDist = hit.Value.Distance;
                minNormal = hit.Value.Normal;
                minPoint = hit.Value.Point;
            }

            if (!hasHit) return null;
            return new Hit(minPoint, minNormal, minDist);
        }

        private static Hit? Cast(Vector2 origin, Vector2 dir, LineSeg lineSeg, float dist = float.MaxValue)
        {
            var lineDir = lineSeg.End - lineSeg.Start;
            var lineNormal = new Vector2(-lineDir.y, lineDir.x).normalized;

            var div = Vector2.Dot(lineNormal, dir);
            if (Mathf.Abs(div) < float.Epsilon) return null;
            var t = Vector2.Dot(lineNormal, lineSeg.Start - origin) / div;
            if (t < 0 || t > dist) return null;
            var hitPoint = origin + dir * t;
            var d = Vector2.Dot(hitPoint - lineSeg.Start, lineDir);
            if (d < 0 || d > lineDir.sqrMagnitude) return null;
            return new Hit(hitPoint, lineNormal, t);
        }
    }
}
