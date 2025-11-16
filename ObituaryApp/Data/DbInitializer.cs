using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ObituaryApp.Models;

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

        IdentityUser? admin;
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(admin, "P@$$w0rd");
            await userManager.AddToRoleAsync(admin, "admin");
        }
        else
        {
            admin = await userManager.FindByEmailAsync(adminEmail);
        }

        IdentityUser? user;
        if (await userManager.FindByEmailAsync(userEmail) == null)
        {
            user = new IdentityUser { UserName = userEmail, Email = userEmail, EmailConfirmed = true };
            await userManager.CreateAsync(user, "P@$$w0rd");
            await userManager.AddToRoleAsync(user, "user");
        }
        else
        {
            user = await userManager.FindByEmailAsync(userEmail);
        }

        // Seed obituaries
        if (!await context.Obituaries.AnyAsync())
        {
            var obituaries = new List<Obituary>
            {
                new Obituary
                {
                    FullName = "Abraham Lincoln",
                    DateOfBirth = new DateOnly(1809, 2, 12),
                    DateOfDeath = new DateOnly(1865, 4, 15),
                    Biography = "Abraham Lincoln was an American lawyer, politician, and statesman who served as the 16th president of the United States from 1861 until his assassination in 1865. Lincoln led the nation through the American Civil War and succeeded in preserving the Union, abolishing slavery, bolstering the federal government, and modernizing the U.S. economy.",
                    CreatorId = admin?.Id ?? string.Empty,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Obituary
                {
                    FullName = "Marie Curie",
                    DateOfBirth = new DateOnly(1867, 11, 7),
                    DateOfDeath = new DateOnly(1934, 7, 4),
                    Biography = "Marie Curie was a Polish and naturalized-French physicist and chemist who conducted pioneering research on radioactivity. She was the first woman to win a Nobel Prize, the first person to win a Nobel Prize twice, and the only person to win a Nobel Prize in two scientific fields.",
                    CreatorId = admin?.Id ?? string.Empty,
                    CreatedAt = DateTime.UtcNow.AddDays(-25)
                },
                new Obituary
                {
                    FullName = "Martin Luther King Jr.",
                    DateOfBirth = new DateOnly(1929, 1, 15),
                    DateOfDeath = new DateOnly(1968, 4, 4),
                    Biography = "Martin Luther King Jr. was an American Baptist minister and activist who became the most visible spokesperson and leader in the civil rights movement from 1955 until his assassination in 1968. He is best known for advancing civil rights through nonviolence and civil disobedience.",
                    CreatorId = user?.Id ?? string.Empty,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new Obituary
                {
                    FullName = "Albert Einstein",
                    DateOfBirth = new DateOnly(1879, 3, 14),
                    DateOfDeath = new DateOnly(1955, 4, 18),
                    Biography = "Albert Einstein was a German-born theoretical physicist, widely acknowledged to be one of the greatest and most influential physicists of all time. He is best known for developing the theory of relativity, but he also made important contributions to the development of the theory of quantum mechanics.",
                    CreatorId = admin?.Id ?? string.Empty,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Obituary
                {
                    FullName = "Nelson Mandela",
                    DateOfBirth = new DateOnly(1918, 7, 18),
                    DateOfDeath = new DateOnly(2013, 12, 5),
                    Biography = "Nelson Mandela was a South African anti-apartheid activist who served as the first president of South Africa from 1994 to 1999. He was the country's first black head of state and the first elected in a fully representative democratic election.",
                    CreatorId = user?.Id ?? string.Empty,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };

            await context.Obituaries.AddRangeAsync(obituaries);
            await context.SaveChangesAsync();
            Console.WriteLine($"✓ Seeded {obituaries.Count} obituaries");
        }
    }
}
