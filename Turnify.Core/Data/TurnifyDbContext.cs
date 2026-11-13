using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using Turnify.Core.Entities;
using Turnify.Core.Services;

namespace Turnify.Core.Data;

public class TurnifyDbContext : DbContext
{
    public TurnifyDbContext(DbContextOptions<TurnifyDbContext> options)
        : base(options)
    {
    }

    public DbSet<Service> Servicios => Set<Service>();
    public DbSet<Barbero> Barberos => Set<Barbero>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Service>(entity =>
        {
            entity.Property(s => s.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(s => s.Precio)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<Barbero>(entity =>
        {
            entity.Property(b => b.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(b => b.Apellido)
                .HasMaxLength(100)
                .IsRequired();
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.Property(c => c.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(c => c.Apellido)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(c => c.Telefono)
                .HasMaxLength(30)
                .IsRequired();
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.Property(u => u.NombreUsuario)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(u => u.NombreUsuario)
                .IsUnique();

            entity.Property(u => u.NombreCompleto)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(u => u.PasswordHash)
                .IsRequired();
        });

        modelBuilder.Entity<Turno>(entity =>
        {
            entity.Property(t => t.PrecioPactado)
                .HasPrecision(18, 2);

            entity.HasOne(t => t.Cliente)
                .WithMany()
                .HasForeignKey(t => t.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Barbero)
                .WithMany()
                .HasForeignKey(t => t.BarberoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Servicio)
                .WithMany()
                .HasForeignKey(t => t.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}