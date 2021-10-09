using System;
using System.Collections.Generic;
using System.Linq;

namespace DelaunayVoronoi
{
    public class DelaunayTriangulator
    {
        public bool BowyerWatson(List<Point> points, HashSet<Triangle> triangles)
        {
            return BowyerWatson(points, 1.0, triangles);
        }
        public bool BowyerWatson(List<Point> points, double margin, HashSet<Triangle> triangles)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;
            if (points.Count > 0) {
                var pt = points[0];
                minX = maxX = pt.X;
                minY = maxY = pt.Y;
            }
            for (int ix = 1; ix < points.Count; ++ix) {
                var pt = points[ix];
                double x = pt.X;
                double y = pt.Y;
                if (x < minX)
                    minX = x;
                else if (x > maxX)
                    maxX = x;
                if (y < minY)
                    minY = y;
                else if (y > maxY)
                    maxY = y;
            }
            minX = minX - margin;
            maxX = maxX + margin;
            minY = minY - margin;
            maxY = maxY + margin;

            var supraTri = GenerateSupraTriangle(minX, minY, maxX, maxY);

            triangles.Clear();
            triangles.Add(supraTri);

            foreach (var point in points)
            {
                HandlePoint(point, triangles);
            }

            var supraTris = m_TriangleHashSetPool.Alloc();
            foreach(var tri in triangles) {
                foreach(var v in tri.Vertices) {
                    if (supraTri.Vertices.Contains(v))
                        supraTris.Add(tri);
                }
            }
            foreach(var tri in supraTris) {
                triangles.Remove(tri);
                tri.Recycle();
            }
            supraTris.Clear();
            m_TriangleHashSetPool.Recycle(supraTris);
            return triangles.Count > 1;
        }

        private Triangle GenerateSupraTriangle(double minX, double minY, double maxX, double maxY)
        {
            var dx = maxX - minX;
            var dy = maxY - minY;
            var deltaMax = Math.Max(dx, dy);
            var midx = (minX + maxX) / 2.0;
            var midy = (minY + maxY) / 2.0;

            var p1 = Point.New(midx - 20.0 * deltaMax, midy - deltaMax);
            var p2 = Point.New(midx, midy + 20.0 * deltaMax);
            var p3 = Point.New(midx + 20.0 * deltaMax, midy - deltaMax);

            return Triangle.New(p1, p2, p3);
        }

        private void HandlePoint(Point point, HashSet<Triangle> triangles)
        {
            var badTriangles = FindBadTriangles(point, triangles);
            var polygon = FindHoleBoundaries(badTriangles);

            foreach (var triangle in badTriangles) {
                foreach (var vertex in triangle.Vertices) {
                    vertex.AdjacentTriangles.Remove(triangle);
                }
            }
            triangles.RemoveWhere(o => badTriangles.Contains(o));

            foreach (var pair in polygon) {
                if (pair.Value == 1) {
                    var edge = pair.Key;
                    if (edge.Point1 != point && edge.Point2 != point) {
                        var triangle = Triangle.New(point, edge.Point1, edge.Point2);
                        triangles.Add(triangle);
                    }
                }
            }

            foreach (var tri in badTriangles) {
                tri.Recycle();
            }
            badTriangles.Clear();
            m_TriangleHashSetPool.Recycle(badTriangles);

            foreach (var pair in polygon) {
                pair.Key.Recycle();
            }
            polygon.Clear();
            m_EdgeIntDictPool.Recycle(polygon);
        }

        private Dictionary<Edge, int> FindHoleBoundaries(HashSet<Triangle> badTriangles)
        {
            var edges = m_EdgeIntDictPool.Alloc();
            foreach (var triangle in badTriangles)
            {
                var a = Edge.New(triangle.Vertices[0], triangle.Vertices[1]);
                var b = Edge.New(triangle.Vertices[1], triangle.Vertices[2]);
                var c = Edge.New(triangle.Vertices[2], triangle.Vertices[0]);
                GroupAddEdge(edges, a);
                GroupAddEdge(edges, b);
                GroupAddEdge(edges, c);
            }
            return edges;
        }

        private HashSet<Triangle> FindBadTriangles(Point point, HashSet<Triangle> triangles)
        {
            var hash = m_TriangleHashSetPool.Alloc();
            foreach(var o in triangles) {
                if (o.IsPointInsideCircumcircle(point)) {
                    hash.Add(o);
                }
            }
            return hash;
        }

        private SimpleObjectPool<Dictionary<Edge, int>> m_EdgeIntDictPool = new SimpleObjectPool<Dictionary<Edge, int>>();
        private SimpleObjectPool<HashSet<Triangle>> m_TriangleHashSetPool = new SimpleObjectPool<HashSet<Triangle>>();

        private static void GroupAddEdge(Dictionary<Edge, int> edges, Edge e)
        {
            int ct;
            if (edges.TryGetValue(e, out ct)) {
                edges[e] = ct + 1;
            }
            else {
                edges.Add(e, 1);
            }
        }
    }
}