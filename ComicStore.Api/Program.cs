using ComicStore.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1) KẾT NỐI DATABASE — đọc ConnectionStrings:DefaultConnection từ appsettings.json,
//    user-secrets (dotnet user-secrets set) hoặc biến môi trường ConnectionStrings__DefaultConnection.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) Controllers + Swagger (Swashbuckle) để test endpoint trực tiếp tại /swagger.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3) CORS — cho phép Front-end (React/Vite) gọi API bằng Axios.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:5173" };
builder.Services.AddCors(options => options.AddPolicy("FrontendPolicy", policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");
app.MapControllers();

// 4) Tùy chọn: tự chạy `dotnet ef database update` + seed khi khởi động (bật/tắt trong appsettings).
if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (app.Configuration.GetValue<bool>("Database:SeedData"))
    {
        DbInitializer.Seed(db);
    }
}

app.Run();