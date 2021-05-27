﻿using System;

namespace CadRevealComposer
{
    using System.Collections.Generic;
    using System.IO;
    using RvmSharp.BatchUtils;
    using RvmSharp.Primitives;
    using System.Linq;
    using System.Numerics;
    using Newtonsoft.Json;
    using Primitives;
    using Utils;

    public static class Program
    {
        static readonly TreeIndexGenerator TreeIndexGenerator = new();
        static readonly NodeIdProvider NodeIdGenerator = new();

        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            var workload = Workload.CollectWorkload(new[] { @"/Users/GUSH/data/hda" });
            var progressReport = new Progress<(string fileName, int progress, int total)>((x) =>
            {
                Console.WriteLine(x.fileName);
            });

            var rvmStore = Workload.ReadRvmData(workload, progressReport);
            // Project name og project parameters tull from Cad Control Center
            var rootNode =
                new CadRevealNode
                {
                    NodeId = NodeIdGenerator.GetNodeId(null),
                    TreeIndex = TreeIndexGenerator.GetNextId(),
                    Parent = null,
                    Group = null,
                    Children = null
                };

            rootNode.Children = rvmStore.RvmFiles.SelectMany(f => f.Model.Children)
                .Select(root => CollectGeometryNodesRecursive(root, rootNode)).ToArray();

            var allNodes = GetAllNodesFlat(rootNode).ToArray();

            var geometries = allNodes.SelectMany(x => x.Geometries).ToArray();

            var distinctDiagonals = geometries.CollectProperties<float, APrimitive>("Diagonal").Distinct();
            var distinctCenterX = geometries.CollectProperties<float, APrimitive>("CenterX").Distinct();
            var distinctCenterY = geometries.CollectProperties<float, APrimitive>("CenterY").Distinct();
            var distinctCenterZ = geometries.CollectProperties<float, APrimitive>("CenterZ").Distinct();
            
            
            var distinctNormals = geometries.CollectProperties<float[], APrimitive>("Normal", "CenterAxis")
                .Select(x => new Vector3(x[0], x[1], x[2])).Distinct().Select(y => new[] { y.X, y.Y, y.Z });
            
            var distinctDelta = geometries.CollectProperties<float, APrimitive>("DeltaX", "DeltaY", "DeltaZ").Distinct();
            
            var height = geometries.CollectProperties<float, APrimitive>("Height").Distinct();
            var radius = geometries.CollectProperties<float, APrimitive>("Radius", "TubeRadius").Distinct();
            var angle = geometries.CollectProperties<float, APrimitive>("RotationAngle", "ArcAngle").Distinct();
            
            var color = geometries.CollectProperties<int[], APrimitive>("Color")
                .Select(x => new Vector4(x[0], x[1], x[2], x[3])).Distinct().Select(x => new int[] { (byte) x.X, (byte)x.Y, (byte)x.Z, (byte)x.W });


            var file = new FileI3D()
            {
                FileSector = new FileSector
                {
                    Header = new Header()
                    {
                        // Constants
                        MagicBytes = 1178874697,
                        FormatVersion = 8,
                        OptimizerVersion = 1,

                        // Arbitrary selected numbers 
                        SectorId = 0,
                        ParentSectorId = null,
                        BboxMax = new[] {1000, 1000, 1000.0},
                        BboxMin = new[] {0, 0, 0.0},
                        Attributes = new Attributes()
                        {
                            Angle = angle.ToArray(),
                            CenterX = distinctCenterX.ToArray(),
                            CenterY = distinctCenterY.ToArray(),
                            CenterZ = distinctCenterZ.ToArray(),
                            Color = color.ToArray(),
                            Normal = distinctNormals.ToArray(),
                            Delta = distinctDelta.ToArray(),
                            Diagonal = distinctDiagonals.ToArray(),
                            ScaleX = new object[0],
                            ScaleY = new object[0],
                            ScaleZ = new object[0],
                            TranslationX = new object[0],
                            TranslationY = new object[0],
                            TranslationZ = new object[0],
                            Radius = radius.ToArray(),
                            FileId = new object[0],
                            Height = height.ToArray(),
                            Texture = new object[0]
                        }
                    },
                    PrimitiveCollections = new Dictionary<string, APrimitive[]>()
                    {
                        {"box_collection", geometries.OfType<Box>().ToArray()},
                        {"circle_collection", new APrimitive[0]},
                        {"closed_cone_collection", new APrimitive[0]},
                        {"closed_cylinder_collection", geometries.OfType<ClosedCylinder>().ToArray()},
                        {"closed_eccentric_cone_collection", new APrimitive[0]},
                        {"closed_ellipsoid_segment_collection", new APrimitive[0]},
                        {"closed_extruded_ring_segment_collection", new APrimitive[0]},
                        {"closed_spherical_segment_collection", new APrimitive[0]},
                        {"closed_torus_segment_collection", geometries.OfType<ClosedTorusSegment>().ToArray()},
                        {"ellipsoid_collection", new APrimitive[0]},
                        {"extruded_ring_collection", new APrimitive[0]},
                        {"nut_collection", new APrimitive[0]},
                        {"open_cone_collection", new APrimitive[0]},
                        {"open_cylinder_collection", geometries.OfType<OpenCylinder>().ToArray()},
                        {"open_eccentric_cone_collection", new APrimitive[0]},
                        {"open_ellipsoid_segment_collection", new APrimitive[0]},
                        {"open_extruded_ring_segment_collection", new APrimitive[0]},
                        {"open_spherical_segment_collection", new APrimitive[0]},
                        {"open_torus_segment_collection", geometries.OfType<OpenTorusSegment>().ToArray()},
                        {"ring_collection", new APrimitive[0]},
                        {"sphere_collection", new APrimitive[0]},
                        {"torus_collection", geometries.OfType<Torus>().ToArray()},
                        {"open_general_cylinder_collection", new APrimitive[0]},
                        {"closed_general_cylinder_collection", new APrimitive[0]},
                        {"solid_open_general_cylinder_collection", new APrimitive[0]},
                        {"solid_closed_general_cylinder_collection", new APrimitive[0]},
                        {"open_general_cone_collection", new APrimitive[0]},
                        {"closed_general_cone_collection", new APrimitive[0]},
                        {"solid_open_general_cone_collection", new APrimitive[0]},
                        {"solid_closed_general_cone_collection", new APrimitive[0]},
                        {"triangle_mesh_collection", new APrimitive[0]},
                        {"instanced_mesh_collection", new APrimitive[0]}
                    }
                }
            };


            File.WriteAllText("output.json", JsonConvert.SerializeObject(file));


            // TODO: Nodes must be generated for implicit geometry like implicit pipes
            // BOX treeIndex, transform -> cadreveal, 

            // TODO: For each CadRevealNode -> Collect CadRevealGeometries -> 
            // TODO: Translate Rvm
        }

        public static IEnumerable<CadRevealNode> GetAllNodesFlat(CadRevealNode root)
        {
            yield return root;

            if (root.Children != null)
            {
                foreach (CadRevealNode cadRevealNode in root.Children)
                {
                    foreach (CadRevealNode revealNode in GetAllNodesFlat(cadRevealNode))
                    {
                        yield return revealNode;
                    }
                }
            }
        }

        public static CadRevealNode CollectGeometryNodesRecursive(RvmNode root, CadRevealNode parent)
        {
            var node = new CadRevealNode
            {
                NodeId = NodeIdGenerator.GetNodeId(null),
                TreeIndex = TreeIndexGenerator.GetNextId(),
                Group = root,
                Parent = parent,
                Children = null
            };

            var childrenCadNodes = root.Children.OfType<RvmNode>().Select(n => CollectGeometryNodesRecursive(n, node)).ToArray();
            if (root.Children.OfType<RvmPrimitive>().Any() && root.Children.OfType<RvmNode>().Any())
            {
                // TODO: Implicit Pipes
                // TODO: Keep Child order when implicit pipes.
            }

            var geometries = root.Children.OfType<RvmPrimitive>()
                .Select(x => APrimitive.FromRvmPrimitive(node, root, x)).Where( g => g != null);

            node.Geometries = geometries.ToArray()!;
            node.Children = childrenCadNodes;
            return node;
        }
    }
}