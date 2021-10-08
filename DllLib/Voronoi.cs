using System.Collections.Generic;

namespace DelaunayVoronoi
{
    public class Voronoi
    {
        public bool GenerateEdgesFromDelaunay(HashSet<Triangle> triangulation, HashSet<Edge> voronoiEdges)
        {
            foreach (var triangle in triangulation)
            {
                foreach (var neighbor in triangle.TrianglesWithSharedEdge)
                {
                    var edge = Edge.New(triangle.Circumcenter, neighbor.Circumcenter);
                    voronoiEdges.Add(edge);
                }
            }

            return voronoiEdges.Count > 0;
        }
    }
}