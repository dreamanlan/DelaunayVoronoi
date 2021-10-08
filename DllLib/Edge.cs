namespace DelaunayVoronoi
{
    public class Edge
    {
        public Point Point1 { get; private set; }
        public Point Point2 { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            var edge = obj as Edge;

            var samePoints = Point1 == edge.Point1 && Point2 == edge.Point2;
            var samePointsReversed = Point1 == edge.Point2 && Point2 == edge.Point1;
            return samePoints || samePointsReversed;
        }
        public override int GetHashCode()
        {
            if (null != Point1 && null != Point2) {
                int hCode = (int)Point1.X ^ (int)Point1.Y ^ (int)Point2.X ^ (int)Point2.Y;
                return hCode.GetHashCode();
            }
            else {
                return base.GetHashCode();
            }
        }
        public void Recycle()
        {
            Point1 = null;
            Point2 = null;
            s_Pool.Recycle(this);
        }
        private void Init(Point point1, Point point2)
        {
            Point1 = point1;
            Point2 = point2;
        }

        public static Edge New(Point point1, Point point2)
        {
            var obj = s_Pool.Alloc();
            obj.Init(point1, point2);
            return obj;
        }
        private static SimpleObjectPool<Edge> s_Pool = new SimpleObjectPool<Edge>();
    }
}