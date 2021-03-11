namespace RvmSharp.Primitives
{
    using System.Numerics;

    public class RvmRectangularTorus : RvmPrimitive
    {
        public readonly float RadiusInner;
        public readonly float RadiusOuter;
        public readonly float Height;
        public readonly float Angle;

        public RvmRectangularTorus(uint version, Matrix4x4 matrix, RvmBoundingBox bBoxLocal, float radiusInner, float radiusOuter, float height, float angle)
            : base(version, RvmPrimitiveKind.RectangularTorus, matrix, bBoxLocal)
        {
            RadiusInner = radiusInner;
            RadiusOuter = radiusOuter;
            Height = height;
            Angle = angle;
        }
    }
}