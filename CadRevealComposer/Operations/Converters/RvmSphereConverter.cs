﻿namespace CadRevealComposer.Operations.Converters;

using Primitives;
using RvmSharp.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Utils;

public static class RvmSphereConverter
{
    public static IEnumerable<APrimitive> ConvertToRevealPrimitive(
        this RvmSphere rvmSphere,
        ulong treeIndex,
        Color color)
    {
        if (!rvmSphere.Matrix.DecomposeAndNormalize(out var scale, out var rotation, out var position))
        {
            throw new Exception("Failed to decompose matrix to transform. Input Matrix: " + rvmSphere.Matrix);
        }
        Trace.Assert(scale.IsUniform(), $"Expected Uniform Scale. Was: {scale}");

        var (normal, _) = rotation.DecomposeQuaternion();

        var radius = rvmSphere.Radius * scale.X;
        var diameter = radius * 2f;
        yield return new EllipsoidSegment(
            radius,
            radius,
            diameter,
            position,
            normal,
            treeIndex,
            color,
            rvmSphere.CalculateAxisAlignedBoundingBox());
    }
}