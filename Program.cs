using Gest_Immo_API.Data;
using Gest_Immo_API.Models;
using Gest_Immo_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Context>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// be able to inject JWTService class inside our Controllers
builder.Services.AddScoped<JWTService>();
builder.Services.AddScoped<EmailService>();

//Defining our identityCore service
builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    options.SignIn.RequireConfirmedEmail = true;
})
    .AddRoles<IdentityRole>()   // be able to add roles
    .AddRoleManager<RoleManager<IdentityRole>>() // be able to make use of RoleManager
    .AddEntityFrameworkStores<Context>() // providing our context
    .AddSignInManager<SignInManager<User>>() // make use of Signing manager
    .AddUserManager<UserManager<User>>() // make use of UserManager to create users
    .AddDefaultTokenProviders(); // be able to create tokens for email confirmation 

// be able to authenticate users using JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // validate the token based on the key provided in appsettings.development.json JWT:Key
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidateIssuer = true,
            ValidateAudience = false
        };
    });

builder.Services.AddCors();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = ActionContext =>
    {
        var errors = ActionContext.ModelState
        .Where(x => x.Value.Errors.Count>0)
        .SelectMany(x=>x.Value.Errors)
        .Select(x=>x.ErrorMessage) .ToArray();

        var toReturn = new
        {
            Errors = errors
        };

        return new BadRequestObjectResult(toReturn);
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors(opt =>
{
    //opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(builder.Configuration["JWT:ClientUrl"]);
    opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:4200");

});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
