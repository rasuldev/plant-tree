using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Memory;
using PlantTree.Data.Entities;
using PlantTree.Infrastructure.Common;

namespace PlantTree.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<News> News { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<Project>().HasMany(x => x.ProjectUsers)
                .WithOne(x => x.Project).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ApplicationUser>().HasMany(x => x.ProjectUsers)
                .WithOne(x => x.User).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ProjectUser>()
                .HasAlternateKey(x => new { x.ProjectId, x.UserId });
            builder.Entity<ProjectUser>().HasIndex(x => x.ProjectId);
            builder.Entity<ProjectUser>().HasIndex(x => x.UserId);
            builder.Entity<Project>().HasOne(x => x.MainImage).WithMany().OnDelete(DeleteBehavior.SetNull);
            builder.Entity<Project>().HasMany(x => x.OtherImages).WithOne(x => x.Project).OnDelete(DeleteBehavior.SetNull);

            // Transaction
            // TODO: it may be useful to create table TransactionArchive
            // and to move all rows for finished projects from Transaction to TransactionArchive
            builder.Entity<Transaction>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Transactions)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany(p => p.Transactions)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transaction>()
                .HasIndex(t => t.ProjectId);

            builder.Entity<Transaction>()
                .HasIndex(t => t.UserId);

            builder.Entity<Transaction>()
                .HasIndex(t => t.Status);

            builder.Entity<Transaction>()
                .HasIndex(t => t.CreationDate);

            builder.Entity<Transaction>()
                .HasIndex(t => t.FinishedDate);


            // News
            builder.Entity<News>().HasOne(n => n.Project).WithMany(p => p.News).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<News>().HasIndex(n => n.Date);

        }


        #region Cache
        
        public event EventHandler SavingChanges;
        protected void OnSavingChanges()
        {
            SavingChanges?.Invoke(this, EventArgs.Empty);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnSavingChanges();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            OnSavingChanges();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        #endregion



    }
}
