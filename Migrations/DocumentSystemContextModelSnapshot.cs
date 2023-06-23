﻿// <auto-generated />
using System;
using DocumentSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DocumentSystem.Migrations
{
    [DbContext(typeof(DocumentSystemContext))]
    partial class DocumentSystemContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.16")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("DocumentSystem.Models.Metadata", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("Updated")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.ToTable("Metadata");
                });

            modelBuilder.Entity("DocumentSystem.Models.Node", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<Guid?>("OwnerId")
                        .HasColumnType("char(36)");

                    b.Property<Guid?>("ParentId")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.HasIndex("ParentId");

                    b.ToTable("Nodes");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Node");
                });

            modelBuilder.Entity("DocumentSystem.Models.Permission", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid?>("FolderId")
                        .HasColumnType("char(36)");

                    b.Property<int>("Mode")
                        .HasColumnType("int");

                    b.Property<Guid?>("RevisionId")
                        .HasColumnType("char(36)");

                    b.Property<Guid?>("RoleId")
                        .HasColumnType("char(36)");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.HasIndex("FolderId");

                    b.HasIndex("RevisionId");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("DocumentSystem.Models.Revision", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid>("DocumentId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("FileId")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.HasIndex("DocumentId");

                    b.ToTable("Revisions");
                });

            modelBuilder.Entity("DocumentSystem.Models.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("DocumentSystem.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("RoleUser", b =>
                {
                    b.Property<Guid>("RolesId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("UsersId")
                        .HasColumnType("char(36)");

                    b.HasKey("RolesId", "UsersId");

                    b.HasIndex("UsersId");

                    b.ToTable("RoleUser");
                });

            modelBuilder.Entity("DocumentSystem.Models.Document", b =>
                {
                    b.HasBaseType("DocumentSystem.Models.Node");

                    b.Property<Guid>("MetadataId")
                        .HasColumnType("char(36)");

                    b.HasIndex("MetadataId");

                    b.HasDiscriminator().HasValue("Document");
                });

            modelBuilder.Entity("DocumentSystem.Models.Folder", b =>
                {
                    b.HasBaseType("DocumentSystem.Models.Node");

                    b.HasDiscriminator().HasValue("Folder");
                });

            modelBuilder.Entity("DocumentSystem.Models.Node", b =>
                {
                    b.HasOne("DocumentSystem.Models.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");

                    b.HasOne("DocumentSystem.Models.Folder", "Parent")
                        .WithMany("Contents")
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Owner");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("DocumentSystem.Models.Permission", b =>
                {
                    b.HasOne("DocumentSystem.Models.Folder", null)
                        .WithMany("Permissions")
                        .HasForeignKey("FolderId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DocumentSystem.Models.Revision", null)
                        .WithMany("Permissions")
                        .HasForeignKey("RevisionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DocumentSystem.Models.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId");

                    b.HasOne("DocumentSystem.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DocumentSystem.Models.Revision", b =>
                {
                    b.HasOne("DocumentSystem.Models.Document", "Document")
                        .WithMany("Revisions")
                        .HasForeignKey("DocumentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Document");
                });

            modelBuilder.Entity("RoleUser", b =>
                {
                    b.HasOne("DocumentSystem.Models.Role", null)
                        .WithMany()
                        .HasForeignKey("RolesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DocumentSystem.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DocumentSystem.Models.Document", b =>
                {
                    b.HasOne("DocumentSystem.Models.Metadata", "Metadata")
                        .WithMany()
                        .HasForeignKey("MetadataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Metadata");
                });

            modelBuilder.Entity("DocumentSystem.Models.Revision", b =>
                {
                    b.Navigation("Permissions");
                });

            modelBuilder.Entity("DocumentSystem.Models.Document", b =>
                {
                    b.Navigation("Revisions");
                });

            modelBuilder.Entity("DocumentSystem.Models.Folder", b =>
                {
                    b.Navigation("Contents");

                    b.Navigation("Permissions");
                });
#pragma warning restore 612, 618
        }
    }
}
