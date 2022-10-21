using LeaveManagement.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Web.Configurations.Entities
{
    public class UserSeedConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            var hasher = new PasswordHasher<Employee>();
            builder.HasData(
                 new Employee
                 {
                     Id = "770ea924-7cfd-4a28-a188-817f108b21b4", //408aa945-3d84-4421-8342-7269ec64d949
                     Email = "admin@localhost.com",
                     NormalizedEmail = "ADMIN@LOCALHOST.COM",
                     NormalizedUserName = "ADMIN@LOCALHOST.COM",
                     UserName = "admin@localhost.com",
                     Firstname = "System",
                     Lastname = "Admin",
                     PasswordHash = hasher.HashPassword(null, "P@ssword1"),
                     EmailConfirmed = true
                 },
                 new Employee
                 {
                     Id = "f7a0f56e-ca55-444c-a28b-d56a4e23ca2b",
                     Email = "user@localhost.com",
                     NormalizedEmail = "USER@LOCALHOST.COM",
                     NormalizedUserName = "USER@LOCALHOST.COM",
                     UserName = "user@localhost.com",
                     Firstname = "System",
                     Lastname = "User",
                     PasswordHash = hasher.HashPassword(null, "P@ssword1"),
                     EmailConfirmed = true
                 }
            );
        }
    }
}