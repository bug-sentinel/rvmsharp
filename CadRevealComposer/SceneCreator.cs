namespace CadRevealComposer;

using Configuration;
using HierarchyComposer.Functions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Operations;
using Primitives;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using Utils;
using Writers;

public static class SceneCreator
{
    public record SectorInfo(
        uint SectorId,
        uint? ParentSectorId,
        long Depth,
        string Path,
        string Filename,
        long EstimatedTriangleCount,
        long EstimatedDrawCallCount,
        IReadOnlyList<APrimitive> Geometries,
        Vector3 BoundingBoxMin,
        Vector3 BoundingBoxMax
    )
    {
        public long DownloadSize { get; init; }
    };

    public static void ExportHierarchyDatabase(string databasePath, CadRevealNode[] allNodes)
    {
        var nodes = HierarchyComposerConverter.ConvertToHierarchyNodes(allNodes);

        ILogger<DatabaseComposer> databaseLogger = NullLogger<DatabaseComposer>.Instance;
        var exporter = new DatabaseComposer(databaseLogger);
        exporter.ComposeDatabase(nodes.ToList(), Path.GetFullPath(databasePath));
    }

    public static void WriteSceneFile(
        ImmutableArray<SectorInfo> sectors,
        ModelParameters parameters,
        DirectoryInfo outputDirectory,
        ulong maxTreeIndex,
        CameraPositioning.CameraPosition cameraPosition)
    {
        static Sector FromSector(SectorInfo sector)
        {

            return new Sector
            {
                Id = sector.SectorId,
                ParentId = sector.ParentSectorId.HasValue // FIXME: not needed anymore?
                    ? sector.ParentSectorId.Value
                    : -1,
                BoundingBox =
                    new BoundingBox(
                        Min: new BbVector3(sector.BoundingBoxMin.X, sector.BoundingBoxMin.Y, sector.BoundingBoxMin.Z),
                        Max: new BbVector3(sector.BoundingBoxMax.X, sector.BoundingBoxMax.Y, sector.BoundingBoxMax.Z)
                    ),
                Depth = sector.Depth,
                Path = sector.Path,
                SectorFileName = sector.Filename,
                EstimatedTriangleCount = sector.EstimatedTriangleCount,
                EstimatedDrawCallCount = sector.EstimatedDrawCallCount,
                // FIXME diagonal data
                DownloadSize = 1000, //FIXME: right size
            };
        }

        var scene = new Scene
        {
            Version = 9,
            ProjectId = parameters.ProjectId,
            ModelId = parameters.ModelId,
            RevisionId = parameters.RevisionId,
            SubRevisionId = -1,
            MaxTreeIndex = maxTreeIndex,
            Unit = "Meters",
            Sectors = sectors.Select(FromSector).ToArray()
        };

        var cameraPath = Path.Join(outputDirectory.FullName, "initialCamera.json");
        var scenePath = Path.Join(outputDirectory.FullName, "scene.json");
        JsonUtils.JsonSerializeToFile(cameraPosition, cameraPath);
        JsonUtils.JsonSerializeToFile(scene, scenePath, Formatting.Indented);
    }

    public static void ExportSector(SectorInfo sector, string outputDirectory)
    {
        var filePath = Path.Join(outputDirectory, sector.Filename);
        using var gltfSectorFile = File.Create(filePath);
        GltfWriter.WriteSector(sector.Geometries.ToArray(), gltfSectorFile);
        gltfSectorFile.Flush(true);
    }
}