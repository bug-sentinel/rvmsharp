﻿namespace CadRevealComposer.Operations.Converters;

using Primitives;
using RvmSharp.Primitives;
using System;
using System.Diagnostics;

public static class RvmCircularTorusConverter
{
    public static APrimitive ConvertToRevealPrimitive(this RvmCircularTorus rvmCircularTorus, RvmNode container, CadRevealNode cadNode)
    {
        var commonPrimitiveProperties = rvmCircularTorus.GetCommonProps(container, cadNode);
        var scale = commonPrimitiveProperties.Scale;
        var normal = commonPrimitiveProperties.RotationDecomposed.Normal;
        var rotationAngle = commonPrimitiveProperties.RotationDecomposed.RotationAngle;

        Trace.Assert(scale.IsUniform(), $"Expected Uniform Scale. Was: {commonPrimitiveProperties}");
        var tubeRadius = rvmCircularTorus.Radius * scale.X;
        var radius = rvmCircularTorus.Offset * scale.X;
        if (rvmCircularTorus.Angle >= Math.PI * 2)
        {
            return new Torus
            (
                commonPrimitiveProperties,
                Normal: normal,
                Radius: radius,
                TubeRadius: tubeRadius
            );
        }

        return new ClosedTorusSegment
        (
            commonPrimitiveProperties,
            Normal: normal,
            Radius: radius,
            TubeRadius: tubeRadius,
            RotationAngle: rotationAngle,
            ArcAngle: rvmCircularTorus.Angle
        );
    }
}