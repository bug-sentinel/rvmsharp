namespace CadRevealComposer.Shadow;

using Primitives;
using System;
using System.Numerics;
using Utils;

public static class CylinderShadowCreator
{
    public static APrimitive CreateShadow(this GeneralCylinder cylinder)
    {
        if (!cylinder.InstanceMatrix.DecomposeAndNormalize(out _, out var cylinderRotation, out var cylinderPosition))
        {
            throw new Exception("Failed to decompose matrix to transform. Input Matrix: " + cylinder.InstanceMatrix);
        }

        var cylinderHeight = Vector3.Distance(cylinder.CenterA, cylinder.CenterB);
        var newScale = new Vector3(cylinder.Radius * 2, cylinder.Radius * 2, cylinderHeight);

        var shadowBoxMatrix =
            Matrix4x4.CreateScale(newScale)
            * Matrix4x4.CreateFromQuaternion(cylinderRotation)
            * Matrix4x4.CreateTranslation(cylinderPosition);

        return new Box(shadowBoxMatrix, cylinder.TreeIndex, cylinder.Color, cylinder.AxisAlignedBoundingBox);
    }
}
