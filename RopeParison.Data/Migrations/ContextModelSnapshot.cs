﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RopeParison.Data;

#nullable disable

namespace RopeParison.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class ContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("RopeParison.Data.Model.Brand", b =>
                {
                    b.Property<int>("BrandId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("BrandId"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("BrandId");

                    b.ToTable("Brands");
                });

            modelBuilder.Entity("RopeParison.Data.Model.Rope", b =>
                {
                    b.Property<int>("RopeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RopeId"));

                    b.Property<int>("BrandId")
                        .HasColumnType("int");

                    b.Property<int>("Category")
                        .HasColumnType("int");

                    b.Property<double>("Diameter")
                        .HasColumnType("float");

                    b.Property<int?>("DropsBeforeBreak55kgOneStrand")
                        .HasColumnType("int");

                    b.Property<int?>("DropsBeforeBreak80kgOneStrand")
                        .HasColumnType("int");

                    b.Property<int?>("DropsBeforeBreak80kgTwoStrand")
                        .HasColumnType("int");

                    b.Property<double?>("DynamicElongation55kgOneStrand")
                        .HasColumnType("float");

                    b.Property<double?>("DynamicElongation80kgOneStrand")
                        .HasColumnType("float");

                    b.Property<double?>("DynamicElongation80kgTwoStrand")
                        .HasColumnType("float");

                    b.Property<double?>("ImpactForce55kgOneStrand")
                        .HasColumnType("float");

                    b.Property<double?>("ImpactForce80kgOneStrand")
                        .HasColumnType("float");

                    b.Property<double?>("ImpactForce80kgTwoStrand")
                        .HasColumnType("float");

                    b.Property<double>("MassPerUnitLength")
                        .HasColumnType("float");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("SheathPercentage")
                        .HasColumnType("float");

                    b.Property<double?>("SheathSlippage")
                        .HasColumnType("float");

                    b.Property<double?>("StaticElongation80kgOneStrand")
                        .HasColumnType("float");

                    b.Property<double?>("StaticElongation80kgTwoStrand")
                        .HasColumnType("float");

                    b.Property<bool>("Verified")
                        .HasColumnType("bit");

                    b.HasKey("RopeId");

                    b.HasIndex("BrandId");

                    b.ToTable("Ropes");
                });

            modelBuilder.Entity("RopeParison.Data.Model.RopeCalculatedParameterSet", b =>
                {
                    b.Property<int>("RopeCalculatedParameterSetId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RopeCalculatedParameterSetId"));

                    b.Property<double?>("Area")
                        .HasColumnType("float");

                    b.Property<double?>("CoreArea")
                        .HasColumnType("float");

                    b.Property<double?>("CoreDiameter")
                        .HasColumnType("float");

                    b.Property<double?>("Density")
                        .HasColumnType("float");

                    b.Property<int>("RopeId")
                        .HasColumnType("int");

                    b.Property<double?>("SheathArea")
                        .HasColumnType("float");

                    b.Property<double?>("SheathThickness")
                        .HasColumnType("float");

                    b.HasKey("RopeCalculatedParameterSetId");

                    b.HasIndex("RopeId")
                        .IsUnique();

                    b.ToTable("RopeCalculatedParameterSets");
                });

            modelBuilder.Entity("RopeParison.Data.Model.RopeEditSuggestion", b =>
                {
                    b.Property<int>("RopeEditSuggestionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RopeEditSuggestionId"));

                    b.Property<int?>("BrandId")
                        .HasColumnType("int");

                    b.Property<int?>("Category")
                        .HasColumnType("int");

                    b.Property<double?>("Diameter")
                        .HasColumnType("float");

                    b.Property<double>("DownVoteCount")
                        .HasColumnType("float");

                    b.Property<int?>("DropsBeforeBreak55kgOneStrand")
                        .HasColumnType("int");

                    b.Property<int?>("DropsBeforeBreak80kgOneStrand")
                        .HasColumnType("int");

                    b.Property<int?>("DropsBeforeBreak80kgTwoStrand")
                        .HasColumnType("int");

                    b.Property<double?>("DynamicElongation55kgOneStrand")
                        .HasColumnType("float");

                    b.Property<double?>("DynamicElongation80kgOneStrand")
                        .HasColumnType("float");

                    b.Property<double?>("DynamicElongation80kgTwoStrand")
                        .HasColumnType("float");

                    b.Property<double?>("ImpactForce55kgOneStrand")
                        .HasColumnType("float");

                    b.Property<double?>("ImpactForce80kgOneStrand")
                        .HasColumnType("float");

                    b.Property<double?>("ImpactForce80kgTwoStrand")
                        .HasColumnType("float");

                    b.Property<double?>("MassPerUnitLength")
                        .HasColumnType("float");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RopeId")
                        .HasColumnType("int");

                    b.Property<double?>("SheathPercentage")
                        .HasColumnType("float");

                    b.Property<double?>("SheathSlippage")
                        .HasColumnType("float");

                    b.Property<double?>("StaticElongation80kgOneStrand")
                        .HasColumnType("float");

                    b.Property<double?>("StaticElongation80kgTwoStrand")
                        .HasColumnType("float");

                    b.Property<double>("UpVoteCount")
                        .HasColumnType("float");

                    b.HasKey("RopeEditSuggestionId");

                    b.HasIndex("BrandId");

                    b.HasIndex("RopeId");

                    b.ToTable("RopeEditSuggestions");
                });

            modelBuilder.Entity("RopeParison.Data.Model.Rope", b =>
                {
                    b.HasOne("RopeParison.Data.Model.Brand", "Brand")
                        .WithMany()
                        .HasForeignKey("BrandId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Brand");
                });

            modelBuilder.Entity("RopeParison.Data.Model.RopeCalculatedParameterSet", b =>
                {
                    b.HasOne("RopeParison.Data.Model.Rope", "Rope")
                        .WithOne("RopeCalculatedParameterSet")
                        .HasForeignKey("RopeParison.Data.Model.RopeCalculatedParameterSet", "RopeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Rope");
                });

            modelBuilder.Entity("RopeParison.Data.Model.RopeEditSuggestion", b =>
                {
                    b.HasOne("RopeParison.Data.Model.Brand", "Brand")
                        .WithMany()
                        .HasForeignKey("BrandId");

                    b.HasOne("RopeParison.Data.Model.Rope", null)
                        .WithMany("RopeEditSuggestions")
                        .HasForeignKey("RopeId");

                    b.Navigation("Brand");
                });

            modelBuilder.Entity("RopeParison.Data.Model.Rope", b =>
                {
                    b.Navigation("RopeCalculatedParameterSet")
                        .IsRequired();

                    b.Navigation("RopeEditSuggestions");
                });
#pragma warning restore 612, 618
        }
    }
}
