using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Projet.Data;
using Projet.Hubs;
using Projet.Models;
using Projet.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<AppUser, AppRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
    }
)
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI();

builder.Services.AddSignalR();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        string path = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Auth.db");

        options.UseSqlite($"Filename={path}");
    }
);


builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 104857600;
});

builder.Services.AddScoped<TokenService, TokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(
        options =>
            {
                #pragma warning disable CS8604 // Possible null reference argument.
                options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<String>(),
                        ValidAudience = builder.Configuration.GetSection("Jwt:Audience").Get<String>(),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:Key").Get<String>()))
                    };
                #pragma warning restore CS8604 // Possible null reference argument.

            }
        
    );

builder.Services.AddAuthorization(config =>
    {
        config.AddPolicy("Can_Add_Role", policy => policy.RequireClaim("Can Add Role", "Can Add Role"));
        config.AddPolicy("Can_Add_Admin", policy => policy.RequireClaim("Can Add Admin", "Can Add Admin"));
        config.AddPolicy("Can_Add_Claim", policy => policy.RequireClaim("Can Add Claim", "Can Add Claim"));
        config.AddPolicy("Can_Add_User", policy => policy.RequireClaim("Can Add User", "Can Add User"));
        config.AddPolicy("Can_Add_Product", policy => policy.RequireClaim("Can Add Product", "Can Add Product"));
        config.AddPolicy("Can_View_Role", policy => policy.RequireClaim("Can View Role", "Can View Role"));
        config.AddPolicy("Can_View_Admin", policy => policy.RequireClaim("Can View Admin", "Can View Admin"));
        config.AddPolicy("Can_View_Claim", policy => policy.RequireClaim("Can View Claim", "Can View Claim"));
        config.AddPolicy("Can_View_User", policy => policy.RequireClaim("Can View User", "Can View User"));
        config.AddPolicy("Can_View_Product", policy => policy.RequireClaim("Can View Product", "Can View Product"));
        config.AddPolicy("Can_DeleteOrLock_User", policy => policy.RequireClaim("Can DeleteOrLock User", "Can DeleteOrLock User"));
        config.AddPolicy("Can_Delete_Admin", policy => policy.RequireClaim("Can Delete Admin", "Can Delete Admin"));
    }
);

builder.Services.AddScoped<ISenderEmail, EmailSender>();

var claims = new List<ClaimModel>
    {
        new ClaimModel{Type = "Can Add Role",Value = "Can Add Role"},
        new ClaimModel{Type = "Can Add Admin",Value = "Can Add Admin"},
        new ClaimModel{Type = "Can Add Claim",Value = "Can Add Claim"},
        new ClaimModel{Type = "Can Add User",Value = "Can Add User"},
        new ClaimModel{Type = "Can Add Product",Value = "Can Add Product"},
        new ClaimModel{Type = "Can View Role",Value = "Can View Role"},
        new ClaimModel{Type = "Can View Admin",Value = "Can View Admin"},
        new ClaimModel{Type = "Can View Claim",Value = "Can View Claim"},
        new ClaimModel{Type = "Can View User",Value = "Can View User"},
        new ClaimModel{Type = "Can View Product",Value = "Can View Product"},
        new ClaimModel{Type = "Can DeleteOrLock User",Value = "Can DeleteOrLock User"},
        new ClaimModel{Type = "Can Delete Admin",Value = "Can Delete Admin"}
    };

var json = JsonSerializer.Serialize(claims, new JsonSerializerOptions{WriteIndented = true});

File.WriteAllText("Claims.json", json);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();
builder.Services.AddControllers();

var app = builder.Build();

app.Use(async (context, next) =>
    {
        context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = 104857600;
        await next.Invoke();
    });

app.UseCors(cors => cors
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials()
    );

app.UseRouting();

app.MapHub<ChatHub>("/chathub");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapIdentityApi<AppUser>();
app.MapControllers();

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
