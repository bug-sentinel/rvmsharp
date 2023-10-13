﻿namespace CadRevealComposer.Operations.Tessellating;

using CadRevealComposer.Primitives;
using CadRevealComposer.Tessellation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

public static class EccentricConeTessellator
{
    public static IEnumerable<APrimitive> Tessellate(EccentricCone cone)
    {
        var vertices = new List<Vector3>();
        var indices = new List<uint>();

        var normal = cone.Normal;
        var centerA = cone.CenterA;
        var radiusA = cone.RadiusA;
        var centerB = cone.CenterB;
        var radiusB = cone.RadiusB;

        var segments = TessellationUtils.SagittaBasedSegmentCount(2 * MathF.PI, float.Max(radiusA, radiusB), 1f, 0.05f);
        var error = TessellationUtils.SagittaBasedError(2 * MathF.PI, float.Max(radiusA, radiusB), 1f, segments);

        var angleIncrement = (2 * MathF.PI) / segments;

        var startVector = TessellationUtils.CreateOrthogonalUnitVector(normal);

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
        yield return new TriangleMesh(mesh, cone.TreeIndex, Color.DarkCyan, cone.AxisAlignedBoundingBox);
    }
}
