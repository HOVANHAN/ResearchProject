using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResearchProject.Areas.Identity.Data;
using ResearchProject.Models;

namespace ResearchProject.Areas.Identity.Data;

public class ResearchProjectContext : IdentityDbContext<ResearchProjectUser>
{
    public ResearchProjectContext(DbContextOptions<ResearchProjectContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
        builder.ApplyConfiguration(new ResearchProjectUserEntityConfiguration());
    }

    public DbSet<ResearchProject.Models.Document>? Document { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Tasks> Tasks { get; set; }
    public DbSet<ResearchProjectUser> researchProjectUsers { get; set; }

    public DbSet<Invitation> invitations { get; set; }

}

public class ResearchProjectUserEntityConfiguration : IEntityTypeConfiguration<ResearchProjectUser>
{
    public void Configure(EntityTypeBuilder<ResearchProjectUser> builder)
    {
        builder.Property(u => u.Name).HasMaxLength(255);
    }
}