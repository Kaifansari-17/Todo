using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Todo.Models;

public partial class TodoDbContext : DbContext
{
    public TodoDbContext()
    {
    }

    public TodoDbContext(DbContextOptions<TodoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Todolist> Todos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=dbsc");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Todolist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Todolist__3213E83F1447893D");

            entity.ToTable("Todolist");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Des)
                .HasMaxLength(255)
                .HasColumnName("des");
            entity.Property(e => e.Logo)
                .HasMaxLength(100)
                .HasColumnName("logo");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
