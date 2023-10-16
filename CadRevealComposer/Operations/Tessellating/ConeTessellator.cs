﻿namespace CadRevealComposer.Operations.Tessellating;

using CadRevealComposer.Primitives;
using CadRevealComposer.Tessellation;
using CadRevealComposer.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

public static class ConeTessellator
{
    public static IEnumerable<APrimitive> Tessellate(Cone cone)
    {
        if (Vector3.Distance(cone.CenterB, cone.CenterA) == 0)
        {
            yield return cone;
            yield break;
        }

        var vertices = new List<Vector3>();
        var indices = new List<uint>();

        var centerA = cone.CenterA;
        var radiusA = cone.RadiusA;
        var centerB = cone.CenterB;
        var radiusB = cone.RadiusB;
        var arcAngle = cone.ArcAngle;

        var segments = TessellationUtils.SagittaBasedSegmentCount(arcAngle, float.Max(radiusA, radiusB), 1f, 0.05f);
        var error = TessellationUtils.SagittaBasedError(arcAngle, float.Max(radiusA, radiusB), 1f, segments);

        var normal = Vector3.Normalize(centerB - centerA);

        bool isComplete = arcAngle.ApproximatelyEquals(2 * MathF.PI);

        var angleIncrement = arcAngle / segments;

        var startVector = cone.LocalXAxis;

        for (uint i = 0; i < segments + 1; i++)
        {
            if (isComplete && i == segments)
                continue;

            var q = Quaternion.CreateFromAxisAngle(normal, -angleIncrement * i);

            var v = Vector3.Transform(startVector, q);

            var vNorm = Vector3.Normalize(v);

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
        yield return new TriangleMesh(mesh, cone.TreeIndex, cone.Color, cone.AxisAlignedBoundingBox);
    }
}
