using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ObituaryApp.Data;
using ObituaryApp.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

try
{
    Console.WriteLine("=== STARTING OBITUARY APP ===");
    
    var builder = WebApplication.CreateBuilder(args);

    Console.WriteLine("=== CONFIGURING CORS ===");
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
            .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    Console.WriteLine("=== ADDING CONTROLLERS ===");
    builder.Services.AddControllersWithViews();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    Console.WriteLine("=== CONFIGURING SWAGGER ===");
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Obituary API",
            Version = "v1",
            Description = "API for managing obituary records"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    });

  Console.WriteLine("=== CONFIGURING DATABASE ===");
var connectionString = builder.Configuration.GetConnectionString("sqldata");
Console.WriteLine($"Connection String: {(string.IsNullOrEmpty(connectionString) ? "MISSING" : "Found")}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(60); // Increase timeout
    }));

    Console.WriteLine("=== CONFIGURING IDENTITY ===");
    builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        // Configure password requirements
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // Configure application cookie for MVC
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    });

    Console.WriteLine("=== REGISTERING SERVICES ===");
    builder.Services.AddScoped<IObituaryService, ObituaryService>();

    Console.WriteLine("=== CONFIGURING JWT ===");
    var jwtSection = builder.Configuration.GetSection("Jwt");
    var jwtKey = jwtSection["Key"];
    var jwtIssuer = jwtSection["Issuer"];
    var jwtAudience = jwtSection["Audience"];
    
    Console.WriteLine($"JWT Key: {(string.IsNullOrEmpty(jwtKey) ? "MISSING" : "Found")}");
    Console.WriteLine($"JWT Issuer: {jwtIssuer ?? "MISSING"}");
    Console.WriteLine($"JWT Audience: {jwtAudience ?? "MISSING"}");
    
    if (string.IsNullOrEmpty(jwtKey))
    {
        throw new InvalidOperationException("Jwt:Key is not configured in appsettings.json");
    }

    // Configure authentication with BOTH Cookie (for MVC) and JWT (for API)
    builder.Services.AddAuthentication(options =>
    {
        // Use cookies as default for MVC controllers
        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

    Console.WriteLine("=== ADDING HTTP CLIENT ===");
    builder.Services.AddHttpClient();
    builder.Services.AddAuthorization();

Console.WriteLine("=== BUILDING APP ===");
var app = builder.Build();

Console.WriteLine("=== ENSURING DATABASE CREATED ===");
var maxAttempts = 10;
var attempt = 0;
var connected = false;

while (attempt < maxAttempts && !connected)
{
    attempt++;
    try
    {
        Console.WriteLine($"Database connection attempt {attempt}/{maxAttempts}...");
        
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var db = services.GetRequiredService<ApplicationDbContext>();
            
            var connString = db.Database.GetConnectionString();
            Console.WriteLine($"Using connection string: {connString}");
            
            // Test connection first
            await db.Database.CanConnectAsync();
            Console.WriteLine("✓ Connection successful!");
            
            // Apply migrations instead of EnsureCreated
            Console.WriteLine("Applying migrations...");
            await db.Database.MigrateAsync();
            Console.WriteLine("✓ Migrations applied!");
            
            // Seed data
            Console.WriteLine("Running database seeder...");
            await DbInitializer.InitializeAsync(services);
            Console.WriteLine("✓ Seeding complete!");
            
            connected = true;
        }
    }
    catch (Exception dbEx)
    {
        Console.WriteLine($"❌ Attempt {attempt} failed: {dbEx.Message}");
        
        if (attempt < maxAttempts)
        {
            var delay = attempt * 2;
            Console.WriteLine($"Waiting {delay} seconds before retry...");
            await Task.Delay(TimeSpan.FromSeconds(delay));
        }
        else
        {
            Console.WriteLine("⚠️ Could not connect to database after all attempts");
            Console.WriteLine("The app will start, but database operations won't work");
        }
    }
}
Console.WriteLine("=== CONFIGURING MIDDLEWARE ===");
    app.UseCors();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseAuthentication();
    app.UseAuthorization();

    // Map MVC routes first
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Obituary}/{action=Index}/{id?}");
    
    // Then map API controllers
    app.MapControllers();

    Console.WriteLine("=== STARTING APP ===");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("=== FATAL STARTUP ERROR ===");
    Console.WriteLine($"Exception Type: {ex.GetType().Name}");
    Console.WriteLine($"Message: {ex.Message}");
    Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    throw;
}