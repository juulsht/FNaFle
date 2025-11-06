using Microsoft.AspNetCore.Identity;

namespace FNaFle.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Ensure roles
            foreach (var r in new[] { "Admin", "User" })
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));

            // Ensure an admin user
            var email = "admin@fnafle.local";
            var admin = await userMgr.FindByEmailAsync(email);
            if (admin == null)
            {
                admin = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                await userMgr.CreateAsync(admin, "Admin123!"); // change later
            }
            if (!await userMgr.IsInRoleAsync(admin, "Admin"))
                await userMgr.AddToRoleAsync(admin, "Admin");
        }
    }
}
