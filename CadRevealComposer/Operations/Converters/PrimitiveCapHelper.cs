﻿namespace CadRevealComposer.Operations.Converters;

using RvmSharp.Primitives;
using System;
using System.Numerics;
using Utils;

public static class PrimitiveCapHelper
{
    public static (bool showCapA, bool showCapB) CalculateCapVisibility(RvmPrimitive primitive, Vector3 centerCapA, Vector3 centerCapB)
    {
        const float factor = 0.000_05f;

        bool showCapA = true, showCapB = true;

        foreach (var connection in primitive.Connections.WhereNotNull())
        {
            // sort Primitive1/Primitive2 to avoid creating double amount of switch statements
            var isSorted = StringComparer.Ordinal.Compare(
                connection.Primitive1.GetType().Name,
                connection.Primitive2.GetType().Name) < 0;

            var prim1 = isSorted
                ? connection.Primitive1
                : connection.Primitive2;

            var prim2 = isSorted
                ? connection.Primitive2
                : connection.Primitive1;

            var offset1 = isSorted
                ? connection.ConnectionIndex1
                : connection.ConnectionIndex2;

            var offset2 = isSorted
                ? connection.ConnectionIndex2
                : connection.ConnectionIndex1;

            var isCenterCapA = connection.Position.EqualsWithinTolerance(centerCapA, factor);
            var isCenterCapB = connection.Position.EqualsWithinTolerance(centerCapB, factor);

            var otherPrimitive = ReferenceEquals(primitive, prim1)
                ? prim2
                : prim1;

            var showCap = (prim1, prim2) switch
            {
                (RvmBox a, RvmCylinder b) => true,
                (RvmBox a, RvmSnout b) => true,
                (RvmCylinder a, RvmCylinder b) => !OtherPrimitiveHasLargerOrEqualCap(primitive, a, b),
                (RvmCircularTorus a, RvmCircularTorus b) => true, // TODO: very common case
                (RvmCircularTorus a, RvmCylinder b) => !OtherPrimitiveHasLargerOrEqualCap(otherPrimitive, a, b),
                (RvmCircularTorus a, RvmSnout b) => true,
                (RvmCylinder a, RvmSphericalDish b) => true,
                (RvmCylinder a, RvmEllipticalDish b) => true,
                (RvmCylinder a, RvmSnout b) => !OtherPrimitiveHasLargerOrEqualCap(otherPrimitive, a, b, offset2),
                (RvmCylinder a, RvmPyramid b) => true,
                (RvmEllipticalDish a, RvmSnout b) => true,
                (RvmSnout a, RvmSnout b) => !OtherPrimitiveHasLargerOrEqualCap(primitive, a, b, offset1, offset2),
                (RvmSnout a, RvmSphericalDish b) => true,
                _ => true
            };

            if (showCap is false && isCenterCapA)
            {
                showCapA = false;
            }
            if (showCap is false && isCenterCapB)
            {
                showCapB = false;
            }
        }

        return (showCapA, showCapB);
    }

    private static bool OtherPrimitiveHasLargerOrEqualCap(
        RvmPrimitive currentPrimitive,
        RvmCylinder rvmCylinder1,
        RvmCylinder rvmCylinder2)
    {
        rvmCylinder1.Matrix.DecomposeAndNormalize(out var scale1, out _, out _);
        rvmCylinder2.Matrix.DecomposeAndNormalize(out var scale2, out _, out _);

        var radius1 = rvmCylinder1.Radius * scale1.X;
        var radius2 = rvmCylinder2.Radius * scale2.X;

        if (ReferenceEquals(currentPrimitive, rvmCylinder1) &&
            radius2 >= radius1)
        {
            return true;
        }

        if (ReferenceEquals(currentPrimitive, rvmCylinder2) &&
            radius1 >= radius2)
        {
            return true;
        }

        return false;
    }

    private static bool OtherPrimitiveHasLargerOrEqualCap(
        RvmPrimitive otherPrimitive,
        RvmCircularTorus rvmCircularTorus,
        RvmCylinder rvmCylinder)
    {
        rvmCircularTorus.Matrix.DecomposeAndNormalize(out var scaleCircularTorus, out _, out _);
        rvmCylinder.Matrix.DecomposeAndNormalize(out var scaleCylinder, out _, out _);

        var radiusCircularTorus = rvmCircularTorus.Radius * scaleCircularTorus.X;
        var radiusCylinder = rvmCylinder.Radius * scaleCylinder.X;

        if (otherPrimitive.GetType() == typeof(RvmCircularTorus))
        {
            if (radiusCircularTorus >= radiusCylinder)
            {
                return true;
            }
        }

        if (otherPrimitive.GetType() == typeof(RvmCylinder))
        {
            if (radiusCylinder >= radiusCircularTorus)
            {
                return true;
            }
        }

        return false;
    }

    private static bool OtherPrimitiveHasLargerOrEqualCap(
        RvmPrimitive otherPrimitive,
        RvmCylinder rvmCylinder,
        RvmSnout rvmSnout,
        uint rvmSnoutOffset)
    {
        rvmCylinder.Matrix.DecomposeAndNormalize(out var scaleCylinder, out _, out _);
        rvmSnout.Matrix.DecomposeAndNormalize(out var scaleSnout, out _, out _);

        var radiusCylinder = rvmCylinder.Radius * scaleCylinder.X;
        var radiusSnoutTop = rvmSnout.RadiusTop * scaleSnout.X;
        var radiusSnoutBottom = rvmSnout.RadiusBottom * scaleSnout.X;

        var isSnoutCapTop = rvmSnoutOffset == 0;
        var isSnoutCapBottom = rvmSnoutOffset == 1;

        if (otherPrimitive.GetType() == typeof(RvmCylinder))
        {
            if (isSnoutCapTop && radiusCylinder >= radiusSnoutTop ||
                isSnoutCapBottom && radiusCylinder >= radiusSnoutBottom)
            {
                return true;
            }
        }

        if (otherPrimitive.GetType() == typeof(RvmSnout))
        {
            if (isSnoutCapTop && radiusSnoutTop >= radiusCylinder ||
                isSnoutCapBottom && radiusSnoutBottom >= radiusCylinder)
            {
                return true;
            }
        }

        return false;
    }

    private static bool OtherPrimitiveHasLargerOrEqualCap(
        RvmPrimitive currentPrimitive,
        RvmSnout rvmSnout1,
        RvmSnout rvmSnout2,
        uint rvmSnoutOffset1,
        uint rvmSnoutOffset2)
    {
        rvmSnout1.Matrix.DecomposeAndNormalize(out var scaleSnout1, out _, out _);
        rvmSnout2.Matrix.DecomposeAndNormalize(out var scaleSnout2, out _, out _);

        var radiusSnoutTop1 = rvmSnout1.RadiusTop * scaleSnout1.X;
        var radiusSnoutBottom1 = rvmSnout1.RadiusBottom * scaleSnout1.X;
        var radiusSnoutTop2 = rvmSnout2.RadiusTop * scaleSnout2.X;
        var radiusSnoutBottom2 = rvmSnout2.RadiusBottom * scaleSnout2.X;

        var isSnoutCapTop1 = rvmSnoutOffset1 == 0;
        var isSnoutCapBottom1 = rvmSnoutOffset1 == 1;
        var isSnoutCapTop2 = rvmSnoutOffset2 == 0;
        var isSnoutCapBottom2 = rvmSnoutOffset2 == 1;

        if (ReferenceEquals(currentPrimitive, rvmSnout1))
        {
            if (isSnoutCapTop1 && isSnoutCapTop2 && radiusSnoutTop2 >= radiusSnoutTop1 ||
                isSnoutCapTop1 && isSnoutCapBottom2 && radiusSnoutTop2 >= radiusSnoutBottom1 ||
                isSnoutCapBottom1 && isSnoutCapTop2 && radiusSnoutBottom2 >= radiusSnoutTop1 ||
                isSnoutCapBottom1 && isSnoutCapBottom2 && radiusSnoutBottom2 >= radiusSnoutBottom1)
            {
                return true;
            }
        }

        if (ReferenceEquals(currentPrimitive, rvmSnout2))
        {
            if (isSnoutCapTop2 && isSnoutCapTop1 && radiusSnoutTop1 >= radiusSnoutTop2 ||
                isSnoutCapTop2 && isSnoutCapBottom1 && radiusSnoutTop1 >= radiusSnoutBottom2 ||
                isSnoutCapBottom2 && isSnoutCapTop1 && radiusSnoutBottom1 >= radiusSnoutTop2 ||
                isSnoutCapBottom2 && isSnoutCapBottom1 && radiusSnoutBottom1 >= radiusSnoutBottom2)
            {
                return true;
            }
        }

        return false;
    }
}