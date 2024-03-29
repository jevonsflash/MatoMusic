﻿// <auto-generated />
using System;
using MatoMusic.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MatoMusic.Migrations
{
    [DbContext(typeof(MatoMusicDbContext))]
    partial class MatoMusicDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.0");

            modelBuilder.Entity("MatoMusic.Core.Playlist", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("TEXT");

                    b.Property<long?>("CreatorUserId")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("DeleterUserId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("DeletionTime")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsRemovable")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastModificationTime")
                        .HasColumnType("TEXT");

                    b.Property<long?>("LastModifierUserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Playlist");
                });

            modelBuilder.Entity("MatoMusic.Core.PlaylistItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("TEXT");

                    b.Property<long?>("CreatorUserId")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("DeleterUserId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("DeletionTime")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastModificationTime")
                        .HasColumnType("TEXT");

                    b.Property<long?>("LastModifierUserId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MusicInfoId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MusicTitle")
                        .HasColumnType("TEXT");

                    b.Property<long>("PlaylistId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Rank")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("PlaylistId");

                    b.ToTable("PlaylistItem");
                });

            modelBuilder.Entity("MatoMusic.Core.Queue", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("TEXT");

                    b.Property<long?>("CreatorUserId")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("DeleterUserId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("DeletionTime")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastModificationTime")
                        .HasColumnType("TEXT");

                    b.Property<long?>("LastModifierUserId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MusicInfoId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MusicTitle")
                        .HasColumnType("TEXT");

                    b.Property<int>("Rank")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Queue");
                });

            modelBuilder.Entity("MatoMusic.Core.Theme.Theme", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ColorA")
                        .HasColumnType("TEXT");

                    b.Property<string>("ColorB")
                        .HasColumnType("TEXT");

                    b.Property<string>("ColorC")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("TEXT");

                    b.Property<long?>("CreatorUserId")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("DeleterUserId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("DeletionTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Img")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsSel")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastModificationTime")
                        .HasColumnType("TEXT");

                    b.Property<long?>("LastModifierUserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Theme");
                });

            modelBuilder.Entity("MatoMusic.Core.PlaylistItem", b =>
                {
                    b.HasOne("MatoMusic.Core.Playlist", "Playlist")
                        .WithMany("PlaylistItems")
                        .HasForeignKey("PlaylistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Playlist");
                });

            modelBuilder.Entity("MatoMusic.Core.Playlist", b =>
                {
                    b.Navigation("PlaylistItems");
                });
#pragma warning restore 612, 618
        }
    }
}
