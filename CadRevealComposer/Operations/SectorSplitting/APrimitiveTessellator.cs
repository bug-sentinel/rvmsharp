namespace CadRevealComposer.Operations.SectorSplitting;

using Primitives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.ExceptionServices;
using Tessellation;
using Utils;

public static class APrimitiveTessellator
{
    public static IEnumerable<APrimitive> TryToTessellate(APrimitive primitive)
    {
        var result = new List<APrimitive>();

        switch (primitive)
        {
            case Box box:
                result.AddRange(Tessellate(box));
                break;
            case EccentricCone cone:
                result.AddRange(Tessellate(cone));
                break;
            case TorusSegment torus:
                result.AddRange(Tessellate(torus));
                break;
            case Cone cone:
                result.AddRange(Tessellate(cone));
                break;
            case GeneralCylinder cylinder:
                result.AddRange(Tessellate(cylinder));
                break;
            default:
                result.Add(primitive with { Color = Color.WhiteSmoke });
                break;
        }

        return result;
    }

    private static IEnumerable<APrimitive> Tessellate(Box box, float error = 0f)
    {
        var vertices = new Vector3[]
        {
            new(-0.5f, -0.5f, -0.5f),
            new(0.5f, -0.5f, -0.5f),
            new(0.5f, 0.5f, -0.5f),
            new(-0.5f, 0.5f, -0.5f),
            new(-0.5f, -0.5f, 0.5f),
            new(0.5f, -0.5f, 0.5f),
            new(0.5f, 0.5f, 0.5f),
            new(-0.5f, 0.5f, 0.5f)
        };
        // csharpier-ignore
        var indices = new uint[]
        {
            0,1,2,
            0,2,3,
            0,1,5,
            0,5,4,
            1,2,6,
            1,6,5,
            2,3,7,
            2,7,6,
            3,0,4,
            3,4,7,
            4,5,6,
            4,6,7
        };

        var matrix = box.InstanceMatrix;

        var transformedVertices = vertices.Select(x => Vector3.Transform(x, matrix)).ToArray();

        var mesh = new Mesh(transformedVertices, indices, error);
        yield return new TriangleMesh(mesh, box.TreeIndex, Color.Aqua, box.AxisAlignedBoundingBox);
    }

    private static IEnumerable<APrimitive> Tessellate(EccentricCone cone, float error = 0)
    {
        int segments = 12;

        var vertices = new List<Vector3>();
        var indices = new List<uint>();

        var normal = cone.Normal;
        var centerA = cone.CenterA;
        var radiusA = cone.RadiusA;
        var centerB = cone.CenterB;
        var radiusB = cone.RadiusB;

        var angleIncrement = (2 * MathF.PI) / segments;

        var startVector = CreateOrthogonalUnitVector(normal);

        for (uint i = 0; i < segments; i++)
        {
            var q = Quaternion.CreateFromAxisAngle(normal, angleIncrement * i);

            var v = Vector3.Transform(startVector, q);

            var vNorm = Vector3.Normalize(v);

            vertices.Add(centerA + vNorm * radiusA);
            vertices.Add(centerB + vNorm * radiusB);

            if (i < segments - 1)
            {
                indices.Add(i * 2);
                indices.Add(i * 2 + 1);
                indices.Add(i * 2 + 2);

                indices.Add(i * 2 + 1);
                indices.Add(i * 2 + 2);
                indices.Add(i * 2 + 3);
            }
            else
            {
                indices.Add(i * 2);
                indices.Add(i * 2 + 1);
                indices.Add(0);

                indices.Add(i * 2 + 1);
                indices.Add(0);
                indices.Add(1);
            }
        }

        var mesh = new Mesh(vertices.ToArray(), indices.ToArray(), error);
        yield return new TriangleMesh(mesh, cone.TreeIndex, Color.Magenta, cone.AxisAlignedBoundingBox);
    }

    private static IEnumerable<APrimitive> Tessellate(TorusSegment torus, float error = 0)
    {
        var vertices = new List<Vector3>();
        var indices = new List<uint>();

        var arcAngle = torus.ArcAngle;
        var offset = torus.Radius;
        var tubeRadius = torus.TubeRadius;
        var matrix = torus.InstanceMatrix;

        uint segments = 12;
        uint turnSegments = 4; // var turnSegments = (int)(torus.ArcAngle / (MathF.PI / 8));

        var turnIncrement = arcAngle / turnSegments;

        var angleIncrement = (2 * MathF.PI) / segments;

        var startVectors = new List<Vector3>(); // start vectors at the circles at each turn segment
        var startCenters = new List<Vector3>(); // the center of the turn segment circles
        var startNormals = new List<Vector3>();

        for (int i = 0; i < turnSegments + 1; i++)
        {
            var turnAngle = i * turnIncrement;
            var normal = Vector3.UnitZ;
            var q = Quaternion.CreateFromAxisAngle(normal, turnAngle);

            var v = Vector3.Transform(Vector3.UnitX, q);

            startVectors.Add(v);
            startCenters.Add(Vector3.Zero + v * (offset));
            startNormals.Add(Vector3.Normalize(Vector3.Cross(normal, v)));
        }

        for (int j = 0; j < turnSegments + 1; j++)
        {
            var startVector = startVectors[j];
            var center = startCenters[j];
            var turnNormal = startNormals[j];

            for (int i = 0; i < segments; i++)
            {
                var q = Quaternion.CreateFromAxisAngle(turnNormal, angleIncrement * i);

                var v = Vector3.Transform(startVector, q);

                var vNorm = Vector3.Normalize(v);

                vertices.Add(center + vNorm * tubeRadius);
            }
        }

        for (uint j = 0; j < turnSegments; j++)
        {
            for (uint i = 0; i < segments; i++)
            {
                if (i < segments - 1)
                {
                    indices.Add(j * segments + i);
                    indices.Add(j * segments + i + 1);
                    indices.Add((j + 1) * segments + i);

                    indices.Add((j + 1) * segments + i);
                    indices.Add(j * segments + i + 1);
                    indices.Add((j + 1) * segments + i + 1);
                }
                else
                {
                    indices.Add(j * segments + i);
                    indices.Add(j * segments);
                    indices.Add((j + 1) * segments + i);

                    indices.Add(j * segments);
                    indices.Add((j + 1) * segments + i);
                    indices.Add((j + 1) * segments);
                }
            }
        }

        var transformedVertices = vertices.Select(x => Vector3.Transform(x, matrix)).ToArray();

        var mesh = new Mesh(transformedVertices, indices.ToArray(), error);
        yield return new TriangleMesh(mesh, torus.TreeIndex, Color.Gold, torus.AxisAlignedBoundingBox);
    }

    private static IEnumerable<APrimitive> Tessellate(Cone cone, float error = 0)
    {
        if (Vector3.Distance(cone.CenterB, cone.CenterA) == 0)
        {
            yield return cone;
            yield break;
        }

        uint totalSegments = 12; // Number of segments if the cone is complete
        var vertices = new List<Vector3>();
        var indices = new List<uint>();

        var centerA = cone.CenterA;
        var radiusA = cone.RadiusA;
        var centerB = cone.CenterB;
        var radiusB = cone.RadiusB;
        var arcAngel = cone.ArcAngle;

        var normal = Vector3.Normalize(centerB - centerA);

        int segments = (int)(totalSegments * (arcAngel / (2 * MathF.PI)));
        if (segments == 0)
            segments = 1;

        bool isComplete = segments == totalSegments;

        var angleIncrement = arcAngel / segments;

        var startVector = CreateOrthogonalUnitVector(normal);
        var localXAxis = cone.LocalXAxis;

        if (
            !startVector.EqualsWithinTolerance(localXAxis, 0.1f)
            && !((startVector * -1).EqualsWithinTolerance(localXAxis, 0.1f))
        )
        {
            var angle = MathF.Acos(Vector3.Dot(startVector, localXAxis) / (startVector.Length() * localXAxis.Length()));
            var test = Quaternion.CreateFromAxisAngle(normal, angle);

            startVector = Vector3.Transform(startVector, test);
        }

        if ((startVector * -1).EqualsWithinTolerance(localXAxis, 0.1f))
        {
            var halfRotation = Quaternion.CreateFromAxisAngle(normal, MathF.PI);
            startVector = Vector3.Transform(startVector, halfRotation);
        }

        var qTest = Quaternion.CreateFromAxisAngle(normal, 3 * MathF.PI / 2.0f);
        startVector = Vector3.Transform(startVector, qTest);

        if (!float.IsFinite(startVector.X) || !float.IsFinite(startVector.X) || !float.IsFinite(startVector.X))
        {
            Console.WriteLine("asmdalks");
        }

        for (uint i = 0; i < segments + 1; i++)
        {
            if (isComplete && i == segments)
                continue;

            var q = Quaternion.CreateFromAxisAngle(normal, angleIncrement * i);

            var v = Vector3.Transform(startVector, q);

            var vNorm = Vector3.Normalize(v);

            var vertexA = centerA + vNorm * radiusA;
            var vertexB = centerB + vNorm * radiusB;

            if (!float.IsFinite(vertexA.X) || !float.IsFinite(vertexB.X))
            {
                Console.WriteLine("nkajsnd");
            }

            vertices.Add(centerA + vNorm * radiusA);
            vertices.Add(centerB + vNorm * radiusB);
        }

        for (uint i = 0; i < segments; i++)
        {
            if (i < segments - 1 || !isComplete)
            {
                indices.Add(i * 2);
                indices.Add(i * 2 + 1);
                indices.Add(i * 2 + 2);

                indices.Add(i * 2 + 1);
                indices.Add(i * 2 + 2);
                indices.Add(i * 2 + 3);
            }
            else
            {
                indices.Add(i * 2);
                indices.Add(i * 2 + 1);
                indices.Add(0);

                indices.Add(i * 2 + 1);
                indices.Add(0);
                indices.Add(1);
            }
        }

        var mesh = new Mesh(vertices.ToArray(), indices.ToArray(), error);
        yield return new TriangleMesh(mesh, cone.TreeIndex, Color.Red, cone.AxisAlignedBoundingBox);
    }

    private static IEnumerable<APrimitive> Tessellate(GeneralCylinder cylinder, float error = 0)
    {
        if (cylinder.TreeIndex == 21864)
        {
            Console.WriteLine("mkl");
        }

        int segments = 12;

        var vertices = new List<Vector3>();
        var indices = new List<uint>();

        var planeA = cylinder.PlaneA;
        var planeB = cylinder.PlaneB;

        var localPlaneANormal = Vector3.Normalize(new Vector3(planeA.X, planeA.Y, planeA.Z));
        var localPlaneBNormal = Vector3.Normalize(new Vector3(planeB.X, planeB.Y, planeB.Z));

        var localXAxis = Vector3.Normalize(cylinder.LocalXAxis);
        var qqq = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.1f);

        var testV1 = Vector3.Transform(Vector3.UnitX, qqq);
        var testV2 = Vector3.Transform(localXAxis, qqq);

        Quaternion rotation;
        if (Vector3.Dot(Vector3.UnitX, localXAxis) > 0.99999f)
        {
            rotation = Quaternion.Identity;
        }
        else if (Vector3.Dot(Vector3.UnitX, localXAxis) < -0.99999f)
        {
            rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI);
        }
        else
        {
            // var cross = Vector3.Normalize(Vector3.Cross(localXAxis, Vector3.UnitX));
            //
            // var angle = MathF.Acos(Vector3.Dot(localXAxis, Vector3.UnitX));
            //
            // var testQ = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(cross, angle));
            //
            // // var rotation = cylinder.Rotation;
            // rotation = testQ;

            // var cross = Vector3.Normalize(Vector3.Cross(Vector3.UnitX, localXAxis));
            //
            // Quaternion q;
            // q.X = cross.X;
            // q.Y = cross.Y;
            // q.Z = cross.Z;
            //
            // q.W =
            //     MathF.Sqrt((Vector3.UnitX.LengthSquared()) * (localXAxis.LengthSquared()))
            //     + Vector3.Dot(Vector3.UnitX, localXAxis);
            //
            // rotation = Quaternion.Inverse(q);

            // float k_cos_theta = Vector3.Dot(Vector3.UnitX, localXAxis);
            // float k = MathF.Sqrt(Vector3.UnitX.LengthSquared() * localXAxis.LengthSquared());
            //
            // if ((k_cos_theta / k).ApproximatelyEquals(-1f, 0.01f))
            // {
            //     rotation = Quaternion.Normalize(new Quaternion(0, 1, 1, 0));
            // }
            //
            // rotation = Quaternion.Normalize(
            //     Quaternion.CreateFromAxisAngle(Vector3.Cross(Vector3.UnitX, localXAxis), k_cos_theta + k)
            // );

            var angle = MathF.Acos(Vector3.Dot(localXAxis, Vector3.UnitX));
            var cross = Vector3.Normalize(Vector3.Cross(Vector3.UnitX, localXAxis));

            rotation = new Quaternion(
                MathF.Cos(angle),
                MathF.Sin(angle / 2f) * cross.X,
                MathF.Sin(angle / 2f) * cross.Y,
                MathF.Sin(angle / 2f) * cross.Z
            );
        }

        Console.WriteLine("-------------------------------------------------------");
        // Console.WriteLine($"Test: {testQ.ToString()}");
        Console.WriteLine($"Rotation: {cylinder.Rotation.ToString()}");
        Console.WriteLine($"Our rotation: {rotation.ToString()}");
        // Console.WriteLine($"Processed unit x: {Vector3.Transform(Vector3.UnitX, testQ)}");
        Console.WriteLine("-------------------------------------------------------");

        //var angleBetweenXs = AngleBetween(Vector3.UnitX, localXAxis);
        //var cross = Vector3.Normalize(Vector3.Cross(Vector3.UnitX, localXAxis));
        //q.X = cross.X;
        //q.Y = cross.Y;
        //q.Z = cross.Z;

        //q.W = MathF.Sqrt(1 * 1 * 1 * 1) + Vector3.Dot(Vector3.UnitX, localXAxis);

        //q = Quaternion.Normalize(q);

        //var q = Quaternion.CreateFromAxisAngle(cross, angleBetweenXs);

        var planeANormal = Vector3.Normalize(Vector3.Transform(localPlaneANormal, rotation));
        var planeBNormal = Vector3.Normalize(-Vector3.Transform(localPlaneBNormal, rotation));

        //var planeANormal = localPlaneANormal;
        //var planeBNormal = localPlaneBNormal;

        var extendedCenterA = cylinder.CenterA;
        var extendedCenterB = cylinder.CenterB;
        var radius = cylinder.Radius;
        var normal = Vector3.Normalize(extendedCenterB - extendedCenterA);

        var anglePlaneA = AngleBetween(normal, planeANormal);
        var anglePlaneB = AngleBetween(normal, planeBNormal);

        //anglePlaneA -= MathF.PI / 2;
        //anglePlaneB -= MathF.PI / 2;

        var extendedHeightA = MathF.Sin(anglePlaneA) * radius;
        var extendedHeightB = MathF.Sin(anglePlaneB) * radius;

        float hypoA = radius;
        float hypoB = radius;

        if (anglePlaneA != 0)
        {
            hypoA = extendedHeightA * (1f / MathF.Sin(anglePlaneA));
        }

        if (anglePlaneB != 0)
        {
            hypoB = extendedHeightB * (1f / MathF.Sin(anglePlaneB));
        }

        var centerA = extendedCenterA + extendedHeightA * normal;
        var centerB = extendedCenterB - extendedHeightB * normal;

        if (!float.IsFinite(centerA.X) || !float.IsFinite(centerB.X))
        {
            Console.WriteLine("jn");
        }

        var angleIncrement = (2 * MathF.PI) / segments;

        //yield return DebugDrawVector(actualPlaneNormalA, centerA);
        //yield return DebugDrawVector(actualPlaneNormalB, centerB);

        //yield return DebugDrawVector(planeANormal, centerA);
        //yield return DebugDrawVector(planeBNormal, centerB);

        //yield return DebugDrawPlane(planeA, centerA);
        //yield return DebugDrawPlane(planeB, centerB);

        var startVectorA = Vector3.Normalize(CreateOrthogonalUnitVector(planeANormal));
        var startVectorB = Vector3.Normalize(CreateOrthogonalUnitVector(planeBNormal));

        for (uint i = 0; i < segments; i++)
        {
            var qA = Quaternion.CreateFromAxisAngle(planeANormal, angleIncrement * i);
            var qB = Quaternion.CreateFromAxisAngle(planeBNormal, angleIncrement * i);

            var vA = Vector3.Transform(startVectorA, qA);
            var vB = Vector3.Transform(startVectorB, qB);

            var vANormalized = Vector3.Normalize(vA);
            var vBNormalized = Vector3.Normalize(vB);

            // TODO
            var distanceFromCenterA = radius + MathF.Abs((hypoA - radius) * MathF.Cos(i * angleIncrement));
            var distanceFromCenterB = radius + MathF.Abs((hypoB - radius) * MathF.Cos(i * angleIncrement));

            vertices.Add(centerA + vANormalized * distanceFromCenterA);
            vertices.Add(centerB + vBNormalized * distanceFromCenterB);

            if (i < segments - 1)
            {
                indices.Add(i * 2);
                indices.Add(i * 2 + 1);
                indices.Add(i * 2 + 2);

                indices.Add(i * 2 + 1);
                indices.Add(i * 2 + 2);
                indices.Add(i * 2 + 3);
            }
            else
            {
                indices.Add(i * 2);
                indices.Add(i * 2 + 1);
                indices.Add(0);

                indices.Add(i * 2 + 1);
                indices.Add(0);
                indices.Add(1);
            }
        }

        var mesh = new Mesh(vertices.ToArray(), indices.ToArray(), error);
        yield return new TriangleMesh(mesh, cylinder.TreeIndex, Color.LimeGreen, cylinder.AxisAlignedBoundingBox);
    }

    private static float ComputeDistance(Vector3 point, Plane plane)
    {
        float dot = Vector3.Dot(plane.Normal, point);
        return dot - plane.D;
    }

    private static float AngleBetween(Vector3 v1, Vector3 v2)
    {
        if (v1.EqualsWithinFactor(v2, 0.1f))
            return 0;

        if ((v1 * -1).EqualsWithinFactor(v2, 0.1f))
            return MathF.PI;

        var result = MathF.Acos(Vector3.Dot(v1, v2) / (v1.Length() * v2.Length()));
        return float.IsFinite(result) ? result : MathF.PI;
    }

    private static Vector3 CreateOrthogonalUnitVector(Vector3 vector)
    {
        var v = Vector3.Normalize(vector);

        if (v.X != 0 && v.Y != 0)
            return Vector3.Normalize(new Vector3(-v.Y, v.X, 0));
        if (v.X != 0 && v.Z != 0)
            return Vector3.Normalize(new Vector3(-v.Z, 0, v.X));
        if (v.Y != 0 && v.Z != 0)
            return Vector3.Normalize(new Vector3(0, -v.Z, v.Y));
        if (v.Equals(Vector3.UnitX) || v.Equals(-Vector3.UnitX))
            return Vector3.UnitY;
        if (v.Equals(Vector3.UnitY) || v.Equals(-Vector3.UnitY))
            return Vector3.UnitZ;
        if (v.Equals(Vector3.UnitZ) || v.Equals(-Vector3.UnitZ))
            return Vector3.UnitX;

        throw new Exception($"Could not find orthogonal vector of {v.ToString()}");
    }

    private static APrimitive DebugDrawVector(Vector3 direction, Vector3 startPoint, float length = 1.0f)
    {
        var baseDiameter = length / 10f;
        var baseLength = length * (4.0f / 5.0f);
        var arrowLength = length / 5.0f;
        var arrowDiameter = length / 5f;

        var unitDirection = Vector3.Normalize(direction);

        var baseCenterA = startPoint;
        var baseCenterB = startPoint + unitDirection * baseLength;
        var arrowCenterA = baseCenterB;
        var arrowCenterB = arrowCenterA + unitDirection * arrowLength;

        uint segments = 6;

        var vertices = new List<Vector3>();
        var indices = new List<uint>();

        var angleIncrement = (2 * MathF.PI) / segments;

        var startVector = CreateOrthogonalUnitVector(unitDirection);

        for (uint i = 0; i < segments; i++)
        {
            var q = Quaternion.CreateFromAxisAngle(unitDirection, angleIncrement * i);
            var v = Vector3.Transform(startVector, q);
            var vNormalized = Vector3.Normalize(v);

            vertices.Add(baseCenterA + vNormalized * baseDiameter);
        }

        for (uint i = 0; i < segments; i++)
        {
            var q = Quaternion.CreateFromAxisAngle(unitDirection, angleIncrement * i);
            var v = Vector3.Transform(startVector, q);
            var vNormalized = Vector3.Normalize(v);

            vertices.Add(baseCenterB + vNormalized * baseDiameter);
        }

        for (uint i = 0; i < segments; i++)
        {
            var q = Quaternion.CreateFromAxisAngle(unitDirection, angleIncrement * i);
            var v = Vector3.Transform(startVector, q);
            var vNormalized = Vector3.Normalize(v);

            vertices.Add(arrowCenterA + vNormalized * arrowDiameter);
        }

        vertices.Add(arrowCenterB);

        for (uint j = 0; j < 2; j++)
        {
            for (uint i = 0; i < segments; i++)
            {
                if (i < segments - 1)
                {
                    indices.Add(j * segments + i);
                    indices.Add(j * segments + i + 1);
                    indices.Add((j + 1) * segments + i);

                    indices.Add((j + 1) * segments + i);
                    indices.Add(j * segments + i + 1);
                    indices.Add((j + 1) * segments + i + 1);
                }
                else
                {
                    indices.Add(j * segments + i);
                    indices.Add(j * segments);
                    indices.Add((j + 1) * segments + i);

                    indices.Add(j * segments);
                    indices.Add((j + 1) * segments + i);
                    indices.Add((j + 1) * segments);
                }
            }
        }

        uint firstBaseVertex = (uint)vertices.Count - 1 - segments;
        uint arrowPoint = (uint)vertices.Count - 1;
        for (uint i = 0; i < segments; i++)
        {
            if (i < segments - 1)
            {
                indices.Add(firstBaseVertex + i);
                indices.Add(((firstBaseVertex + i + 1)));
                indices.Add(arrowPoint);
            }
            else
            {
                indices.Add(firstBaseVertex + i);
                indices.Add(firstBaseVertex);
                indices.Add(arrowPoint);
            }
        }

        var boundingBox = new BoundingBox(baseCenterB - Vector3.One, baseCenterB + Vector3.One);

        var mesh = new Mesh(vertices.ToArray(), indices.ToArray(), 0);
        return new TriangleMesh(mesh, 0, Color.Magenta, boundingBox);
    }

    private static TriangleMesh DebugDrawPlane(Vector4 plane, Vector3 startPoint)
    {
        var planeNormal = new Vector3(plane.X, plane.Y, plane.Z);

        var startVector = CreateOrthogonalUnitVector(planeNormal);

        var vertices = new List<Vector3>();

        for (int i = 0; i < 4; i++)
        {
            var q = Quaternion.CreateFromAxisAngle(planeNormal, i * MathF.PI / 2.0f);

            vertices.Add(Vector3.Transform(startVector, q) + startPoint);
        }

        var indices = new uint[] { 0, 1, 2, 0, 2, 3 };

        var boundingBox = new BoundingBox(startPoint - Vector3.One, startPoint + Vector3.One);

        if (!float.IsFinite(boundingBox.Center.X))
            Console.WriteLine("mksdlf");

        var mesh = new Mesh(vertices.ToArray(), indices.ToArray(), 0);
        return new TriangleMesh(mesh, 0, Color.Aquamarine, boundingBox);
    }
}
