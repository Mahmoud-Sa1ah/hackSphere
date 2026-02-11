using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PentestHub.API.Data;
using PentestHub.API.Hubs;
using PentestHub.API.Data.Models;
using PentestHub.API.Services;
using PentestHub.API.Repositories;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = 1073741824; // 1GB
    x.MemoryBufferThreshold = int.MaxValue;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PentestHub API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            Array.Empty<string>()
        }
    });
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=localhost;Database=PentestHub;User=root;Password=;Port=3306;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyForJWTTokenGenerationThatIsAtLeast32Characters";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "PentestHub";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "PentestHubUsers";

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

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs/launcher") || path.StartsWithSegments("/hubs/notifications")))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// SignalR
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IToolRepository, ToolRepository>();
builder.Services.AddScoped<ILabRepository, LabRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IToolService, ToolService>();

builder.Services.AddScoped<ILabService, LabService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ILauncherService, LauncherService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IDomainService, DomainService>();

// AutoMapper (optional - can be added later if needed)
// builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

// Serve static files from wwwroot (for uploaded images)
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<LauncherHub>("/hubs/launcher");
app.MapHub<NotificationHub>("/hubs/notifications");

// Ensure database is created and seed data
var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

// Always ensure database and all tables are created (this will create all tables from the model)
// Note: EnsureCreated() only creates if database doesn't exist, but won't add new tables to existing database
// For production, use migrations instead
try
{
    // First, ensure the database can connect
    if (!db.Database.CanConnect())
    {
        db.Database.EnsureCreated();
    }
    else
    {
        // Database exists, but we need to ensure all tables exist
        // Try to query a table to see if schema is up to date
        try
        {
            db.Database.ExecuteSqlRaw("SELECT 1 FROM Roles LIMIT 1");
        }
        catch
        {
            // Tables don't exist, recreate them
            Console.WriteLine("Database exists but tables are missing. Recreating database schema...");
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }

    // Sync database schema manually (silent updates)
    Console.WriteLine("Syncing database schema...");
    var migrations = new[] {
        "ALTER TABLE Users ADD COLUMN TwoFactorSecret LONGTEXT NULL;",
        "ALTER TABLE Users ADD COLUMN IsTwoFactorEnabled BOOLEAN NOT NULL DEFAULT FALSE;",
        "ALTER TABLE Users ADD COLUMN ResetToken LONGTEXT NULL;",
        "ALTER TABLE Users ADD COLUMN ResetTokenExpires DATETIME(6) NULL;",
        "ALTER TABLE Users ADD COLUMN ProfilePhoto LONGTEXT NULL;",
        "ALTER TABLE Users ADD COLUMN Bio LONGTEXT NULL;",
        "ALTER TABLE Users MODIFY COLUMN ScanId int NULL;",
        "ALTER TABLE Tools ADD COLUMN PackageData LONGBLOB NULL;",
        "ALTER TABLE Users ADD COLUMN Points INT NOT NULL DEFAULT 0;",
        "ALTER TABLE Users ADD COLUMN CompletedRooms INT NOT NULL DEFAULT 0;",
        "ALTER TABLE Users ADD COLUMN Streak INT NOT NULL DEFAULT 0;",
        "ALTER TABLE Users ADD COLUMN LastLabSolvedDate DATETIME(6) NULL;",
        "ALTER TABLE Labs ADD COLUMN Points INT NOT NULL DEFAULT 0;",
        "ALTER TABLE Tools ADD COLUMN PackageExtension VARCHAR(10) NULL;",
        "ALTER TABLE ScanHistory ADD COLUMN VulnerabilityCount INT NOT NULL DEFAULT 0;"
    };

    foreach (var sql in migrations)
    {
        try { db.Database.ExecuteSqlRaw(sql); } catch { /* Ignore if exists or constraint fails */ }
    }

    // Update Lab points if they are still 0 (silent update for existing data)
    try {
        db.Database.ExecuteSqlRaw("UPDATE Labs SET Points = 10 WHERE LabId IN (1, 3, 4);");
        db.Database.ExecuteSqlRaw("UPDATE Labs SET Points = 25 WHERE LabId IN (2, 7, 8, 9);");
        db.Database.ExecuteSqlRaw("UPDATE Labs SET Points = 50 WHERE LabId IN (5, 6, 10);");
    } catch { /* Silent */ }

    try
    {
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS KnowledgeBases (
                KnowledgeBaseId INT AUTO_INCREMENT PRIMARY KEY,
                Title VARCHAR(200) NOT NULL,
                Category VARCHAR(100) NOT NULL,
                Content TEXT NOT NULL,
                Tags VARCHAR(500),
                IsPublished BOOLEAN NOT NULL DEFAULT TRUE,
                CreatedAt DATETIME(6) NOT NULL,
                UpdatedAt DATETIME(6) NULL,
                CreatedBy INT NULL
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ");
        
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS Badges (
                BadgeId INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(100) NOT NULL,
                Description VARCHAR(500),
                PointsRequired INT NOT NULL,
                IconUrl VARCHAR(500)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ");

        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS UserBadges (
                UserBadgeId INT AUTO_INCREMENT PRIMARY KEY,
                UserId INT NOT NULL,
                BadgeId INT NOT NULL,
                DateEarned DATETIME(6) NOT NULL,
                CONSTRAINT FK_UserBadges_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
                CONSTRAINT FK_UserBadges_Badges_BadgeId FOREIGN KEY (BadgeId) REFERENCES Badges(BadgeId) ON DELETE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ");

        // Seed Roles if empty
        if (!db.Roles.Any())
        {
            db.Roles.AddRange(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Learner" },
                new Role { RoleId = 3, RoleName = "Professional" }
            );
            db.SaveChanges();
        }

        // Seed Badges if empty
        if (!db.Badges.Any())
        {
            db.Badges.AddRange(
                new Badge { Name = "Bronze Badge", Description = "Earned 100 points", PointsRequired = 100, IconUrl = "/badges/bronze.png" },
                new Badge { Name = "Silver Badge", Description = "Earned 500 points", PointsRequired = 500, IconUrl = "/badges/silver.png" },
                new Badge { Name = "Gold Badge", Description = "Earned 1000 points", PointsRequired = 1000, IconUrl = "/badges/gold.png" }
            );
            db.SaveChanges();
        }
    } catch (Exception ex) { 
        Console.WriteLine($"Warning: Table creation or seeding failed: {ex.Message}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Database sync issue: {ex.Message}");
}

app.Run();

