using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace KinmuReport.Models;

public partial class AttendanceContext : DbContext
{
    public AttendanceContext()
    {
    }

    public AttendanceContext(DbContextOptions<AttendanceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<アップロード履歴> アップロード履歴s { get; set; }

    public virtual DbSet<グループ> グループs { get; set; }

    public virtual DbSet<ロック> ロックs { get; set; }

    public virtual DbSet<勤怠> 勤怠s { get; set; }

    public virtual DbSet<社員> 社員s { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<アップロード履歴>(entity =>
        {
            entity.HasKey(e => new { e.社員番号, e.対象年月 }).HasName("アップロード履歴_pkey");

            entity.Property(e => e.アップロード日時).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.社員番号Navigation).WithMany(p => p.アップロード履歴s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("アップロード履歴_社員番号_fkey");
        });

        modelBuilder.Entity<グループ>(entity =>
        {
            entity.HasKey(e => e.グループコード).HasName("グループ_pkey");
        });

        modelBuilder.Entity<ロック>(entity =>
        {
            entity.HasKey(e => new { e.社員番号, e.対象年月 }).HasName("ロック_pkey");

            entity.Property(e => e.ロック日時).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.ロック者番号Navigation).WithMany(p => p.ロックロック者番号Navigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ロック_ロック者番号_fkey");

            entity.HasOne(d => d.社員番号Navigation).WithMany(p => p.ロック社員番号Navigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ロック_社員番号_fkey");
        });

        modelBuilder.Entity<勤怠>(entity =>
        {
            entity.HasKey(e => new { e.社員番号, e.勤務日 }).HasName("勤怠_pkey");

            entity.HasOne(d => d.社員番号Navigation).WithMany(p => p.勤怠s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("勤怠_社員番号_fkey");
        });

        modelBuilder.Entity<社員>(entity =>
        {
            entity.HasKey(e => e.社員番号).HasName("社員_pkey");

            entity.HasOne(d => d.グループコードNavigation).WithMany(p => p.社員s).HasConstraintName("社員_グループコード_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
