using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ObituaryApp.Data;
using ObituaryApp.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    // Allow any origin/header/method to avoid CORS issues during development and simple deployments.
    // NOTE: In production you should constrain this to the specific origins you expect.
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
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

// Configure EF Core + Identity with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=obituaries.db"));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// register application services
builder.Services.AddScoped<IObituaryService, ObituaryService>();

// read jwt options
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured");
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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

// Register authorization services so UseAuthorization() has required services
builder.Services.AddAuthorization();

// No per-client CORS policy required after removing frontend apps; keep default policy configured above

var app = builder.Build();

// Ensure DB is created and seed users/roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    DbInitializer.InitializeAsync(services).GetAwaiter().GetResult();
}

// Configure the HTTP request pipeline.

// Enable CORS using the default policy (AllowAnyOrigin/AnyHeader/AnyMethod)
// This must be called before Swagger and other middleware that might generate responses
app.UseCors();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Obituary API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
// Serve static files (for uploaded images)
app.UseStaticFiles();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Client apps removed: no fallback file mapping required

app.Run();
