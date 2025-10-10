using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ObituaryApp.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // apply pending migrations
        await context.Database.MigrateAsync();

        // seed roles
        if (!await roleManager.RoleExistsAsync("admin"))
            await roleManager.CreateAsync(new IdentityRole("admin"));
        if (!await roleManager.RoleExistsAsync("user"))
            await roleManager.CreateAsync(new IdentityRole("user"));

        // seed users
        var adminEmail = "aa@aa.aa";
        var userEmail = "uu@uu.uu";

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(admin, "P@$$w0rd");
            await userManager.AddToRoleAsync(admin, "admin");
        }

        if (await userManager.FindByEmailAsync(userEmail) == null)
        {
            var user = new IdentityUser { UserName = userEmail, Email = userEmail, EmailConfirmed = true };
            await userManager.CreateAsync(user, "P@$$w0rd");
            await userManager.AddToRoleAsync(user, "user");
        }
    }
}
