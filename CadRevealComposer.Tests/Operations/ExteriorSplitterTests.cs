﻿namespace CadRevealComposer.Tests.Operations;

using CadRevealComposer.Operations;
using CadRevealComposer.Primitives;
using NUnit.Framework;
using RvmSharp.Primitives;
using System.Drawing;
using System.Linq;
using System.Numerics;

[TestFixture]
public class ExteriorSplitterTests
{
    [Test]
    public void ExteriorTest()
    {
        var exterior = CreateBoxCenteredInOrigin(1, 10);
        var interior = CreateBoxCenteredInOrigin(2, 5);
        var (exteriorList, interiorList) = ExteriorSplitter.Split(new APrimitive[] { interior, exterior });

        Assert.AreEqual(1, exteriorList.Length);
        Assert.AreEqual(1, interiorList.Length);
        Assert.AreEqual(exterior, exteriorList.Single());
        Assert.AreEqual(interior, interiorList.Single());
    }

    /// <summary>
    /// The exterior splitter uses axis aligned bounding box for Box primitive. All other data is irrelevant.
    /// </summary>
    private static Box CreateBoxCenteredInOrigin(ulong nodeId, float boxSize)
    {
        return new Box(
            Matrix4x4.Identity,
            Color.Red,
            int.MaxValue,
            new RvmBoundingBox(
                new Vector3(-boxSize / 2f),
                new Vector3(boxSize / 2f)));
    }
}