using System.Text;
using AuthBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<AuthBackend.Services.SmtpSettings>(builder.Configuration.GetSection(AuthBackend.Services.SmtpSettings.SectionName));
builder.Services.Configure<AuthBackend.Services.JwtSettings>(builder.Configuration.GetSection(AuthBackend.Services.JwtSettings.SectionName));

builder.Services.AddSingleton<IOtpService, OtpService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddSingleton<IAppleAuthService, AppleAuthService>();

var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyForJwtTokensMustBeAtLeast32Characters";
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
