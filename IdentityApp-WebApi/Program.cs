using IdentityApp_WebApi.Data;
using IdentityApp_WebApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//configure the dbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

    // definging our IdentityCore Service
builder.Services.AddIdentityCore<User>(options =>
{
    //password Configuration
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    //for emailConfiguration
    options.SignIn.RequireConfirmedEmail = true;
}) .AddRoles<IdentityRole>()    // be able to add roles 
   .AddRoles<RoleManager<IdentityRole>>()   // be able to make use of roleMangaer
   .AddEntityFrameworkStores<AuthDbContext>()   // providing our dbContext 
   .AddSignInManager<SignInManager<User>>()     // make use Of Signin Manager
   .AddUserManager<UserManager<User>>()     //make use of User MAnager To create users
   .AddDefaultTokenProviders(); // be able to create token for email confirmation 

// be able to authenticate users using JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Validate the token bases on the key  me have provided inside appsetings.developpement.json JET:key
                ValidateIssuerSigningKey = true,
                // the issuer singing key based on JWT:Key
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:key"])),
                // the issuer which in here is the api project url we are using
                ValidIssuer = builder.Configuration["JWT:Issuer"],
                //validate the Issuer (who ever is issuing the JWT)
                ValidateIssuer = true,
                //don't validate audiance (angular side)
                ValidateAudience = false

            };
        });



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// adding UseAuthentication into our pipeline and this should come before UseAuthorisation 
// Authentication verifies the identity of a user or services , and authorisation determines their access rights.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
