namespace CadRevealComposer.AlgebraExtensions
{
    using System;
    using System.Numerics;

    // ReSharper disable once InconsistentNaming
    public struct Vector3i
    {
        public int X;
        public int Y;
        public int Z;

        public Vector3i(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 operator *(Vector3i a, float b)
        {
            return new Vector3(a.X * b, a.Y * b, a.Z * b);
        }

        public static Vector3i operator +(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public int this[int i]
        {
            get
            {
                return i switch
                {
                    0 => X,
                    1 => Y,
                    2 => Z,
                    _ => throw new IndexOutOfRangeException()
                };
            }
            set
            {
                switch (i)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }
}