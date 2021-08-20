﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mop.Hierarchy;

namespace Mop.Hierarchy.Migrations
{
    [DbContext(typeof(HierarchyContext))]
    partial class HierarchyContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.1");

            modelBuilder.Entity("Mop.Hierarchy.Model.AABB", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("AABBs");
                });

            modelBuilder.Entity("Mop.Hierarchy.Model.Node", b =>
                {
                    b.Property<uint>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AABBId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("HasMesh")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<uint?>("ParentId")
                        .HasColumnType("INTEGER");

                    b.Property<uint?>("TopNodeId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AABBId");

                    b.HasIndex("ParentId");

                    b.HasIndex("TopNodeId");

                    b.ToTable("Nodes");
                });

            modelBuilder.Entity("Mop.Hierarchy.Model.NodePDMSEntry", b =>
                {
                    b.Property<uint>("NodeId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("PDMSEntryId")
                        .HasColumnType("INTEGER");

                    b.HasKey("NodeId", "PDMSEntryId");

                    b.HasIndex("PDMSEntryId");

                    b.ToTable("NodeToPDMSEntries");
                });

            modelBuilder.Entity("Mop.Hierarchy.Model.PDMSEntry", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("PDMSEntries");
                });

            modelBuilder.Entity("Mop.Hierarchy.Model.AABB", b =>
                {
                    b.OwnsOne("Mop.Hierarchy.Model.Vector3f", "max", b1 =>
                        {
                            b1.Property<int>("AABBId")
                                .HasColumnType("INTEGER");

                            b1.Property<float>("x")
                                .HasColumnType("REAL");

                            b1.Property<float>("y")
                                .HasColumnType("REAL");

                            b1.Property<float>("z")
                                .HasColumnType("REAL");

                            b1.HasKey("AABBId");

                            b1.ToTable("AABBs");

                            b1.WithOwner()
                                .HasForeignKey("AABBId");
                        });

                    b.OwnsOne("Mop.Hierarchy.Model.Vector3f", "min", b1 =>
                        {
                            b1.Property<int>("AABBId")
                                .HasColumnType("INTEGER");

                            b1.Property<float>("x")
                                .HasColumnType("REAL");

                            b1.Property<float>("y")
                                .HasColumnType("REAL");

                            b1.Property<float>("z")
                                .HasColumnType("REAL");

                            b1.HasKey("AABBId");

                            b1.ToTable("AABBs");

                            b1.WithOwner()
                                .HasForeignKey("AABBId");
                        });
                });

            modelBuilder.Entity("Mop.Hierarchy.Model.Node", b =>
                {
                    b.HasOne("Mop.Hierarchy.Model.AABB", "AABB")
                        .WithMany()
                        .HasForeignKey("AABBId");

                    b.HasOne("Mop.Hierarchy.Model.Node", "Parent")
                        .WithMany()
                        .HasForeignKey("ParentId");

                    b.HasOne("Mop.Hierarchy.Model.Node", "TopNode")
                        .WithMany()
                        .HasForeignKey("TopNodeId");
                });

            modelBuilder.Entity("Mop.Hierarchy.Model.NodePDMSEntry", b =>
                {
                    b.HasOne("Mop.Hierarchy.Model.Node", "Node")
                        .WithMany("NodePDMSEntry")
                        .HasForeignKey("NodeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Mop.Hierarchy.Model.PDMSEntry", "PDMSEntry")
                        .WithMany("NodePDMSEntry")
                        .HasForeignKey("PDMSEntryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
