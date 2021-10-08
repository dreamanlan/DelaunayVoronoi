using System.Collections.Generic;

namespace DelaunayVoronoi
{
    public class Point
    {
        /// <summary>
        /// Used only for generating a unique ID for each instance of this class that gets generated
        /// </summary>
        private static int _counter;

        /// <summary>
        /// Used for identifying an instance of a class; can be useful in troubleshooting when geometry goes weird
        /// (e.g. when trying to identify when Triangle objects are being created with the same Point object twice)
        /// </summary>
        private readonly int _instanceId = _counter++;

        public double X { get; private set; }
        public double Y { get; private set; }
        public HashSet<Triangle> AdjacentTriangles { get; } = new HashSet<Triangle>();

        public override string ToString()
        {
            // Simple way of seeing what's going on in the debugger when investigating weirdness
            return $"{nameof(Point)} {_instanceId} {X:0.##}@{Y:0.##}";
        }
        public void Recycle()
        {
            AdjacentTriangles.Clear();
            s_Pool.Recycle(this);
        }
        private void Init(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Point New(double x, double y)
        {
            var obj = s_Pool.Alloc();
            obj.Init(x, y);
            return obj;
        }
        private static SimpleObjectPool<Point> s_Pool = new SimpleObjectPool<Point>();
    }
}