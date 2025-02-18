﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ApiBlancosLogan.Models;

public partial class BlancosLoganContext : DbContext
{
    public BlancosLoganContext()
    {
    }

    public BlancosLoganContext(DbContextOptions<BlancosLoganContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Concepto> Conceptos { get; set; }

    public virtual DbSet<Direccion> Direccions { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Termino> Terminos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Ventum> Venta { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=GWNC31514;Database=BlancosLogan;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente);

            entity.ToTable("Cliente");

            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Concepto>(entity =>
        {
            entity.HasKey(e => e.IdConcepto);

            entity.ToTable("Concepto");

            entity.Property(e => e.IdConcepto).HasColumnName("idConcepto");
            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            entity.Property(e => e.IdVenta).HasColumnName("idVenta");
            entity.Property(e => e.Precio).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<Direccion>(entity =>
        {
            entity.HasKey(e => e.IdDireccion);

            entity.ToTable("Direccion");

            entity.Property(e => e.IdDireccion).HasColumnName("idDireccion");
            entity.Property(e => e.Calle)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Colonia)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Municipio)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Numero)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto);

            entity.ToTable("Producto");

            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Precio).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Ubicacion).HasColumnType("text");
        });

        modelBuilder.Entity<Termino>(entity =>
        {
            entity.HasKey(e => e.IdTerminos);

            entity.Property(e => e.IdTerminos).HasColumnName("idTerminos");
            entity.Property(e => e.ArchivoVersion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario);

            entity.ToTable("Usuario");

            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(256)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Ventum>(entity =>
        {
            entity.HasKey(e => e.IdVenta);

            entity.Property(e => e.IdVenta).HasColumnName("idVenta");
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.IdDirecion).HasColumnName("idDirecion");
            entity.Property(e => e.Pago)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
