﻿namespace CadRevealComposer.Tests.Primitives.Converters;

using CadRevealComposer.Operations.Converters;
using CadRevealComposer.Primitives;
using NUnit.Framework;
using RvmSharp.Primitives;
using System.Linq;
using System.Numerics;

public class RvmSphericalDishConverterTests
{
    const int _treeIndex = 1337;
    private RvmSphericalDish _rvmSphericalDish;

    [SetUp]
    public void Setup()
    {
        _rvmSphericalDish = new RvmSphericalDish(
            Version: 2,
            Matrix: Matrix4x4.Identity,
            BoundingBoxLocal: new RvmBoundingBox(-Vector3.One, Vector3.One),
            BaseRadius: 1,
            Height: 1
        );
    }

    [Test]
    public void RvmSphericalDishConverter_ReturnsEllipsoidSegmentWithCap()
    {

        var geometries = _rvmSphericalDish.ConvertToRevealPrimitive(_treeIndex, System.Drawing.Color.Red).ToArray();

        Assert.That(geometries.Length, Is.EqualTo(2));
        Assert.That(geometries[0], Is.TypeOf<EllipsoidSegment>());
        Assert.That(geometries[1], Is.TypeOf<Circle>());
    }
}
