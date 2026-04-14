using ChatBot.Data;
using ChatBot.Services;
using ChatBot.Repository;
using Microsoft.EntityFrameworkCore;
using ChatBot.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Connection string not found");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

builder.Services.AddAuthentication("ManualAuth")
    .AddCookie("ManualAuth", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

builder.Services.AddAuthorization();

// Dependency Injection
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IChatKnowledgeRepository, ChatKnowledgeRepository>();
builder.Services.AddHttpClient<IAiService, OpenAiService>();
builder.Services.AddScoped<IAiService, OpenAiService>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

// ✅ FIX 1: Register OtpService and EmailService (were missing)
builder.Services.AddScoped<OtpService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<DbConnector>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ FIX 2: Use PORT from environment (Railway sets this dynamically)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ✅ FIX 3: Correct middleware order (UseRouting BEFORE Auth, no duplicates, no HTTPS redirect on Railway)
app.UseStaticFiles();
app.UseRouting();           // ← Must be first
app.UseAuthentication();    // ← Then authentication
app.UseAuthorization();     // ← Then authorization (only ONCE)

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();