using RaceDay.Mvc.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add MVC
builder.Services.AddControllersWithViews();

// 2. Session for MVC (stores UserId, Role, DisplayName, ApiSessionCookie)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".RaceDay.Mvc.Session";
});

// 3. HttpContextAccessor for accessing session in the API client
builder.Services.AddHttpContextAccessor();

// 4. HttpClient for the Part 2 API
builder.Services.AddHttpClient("RaceDayApi", client =>
{
    string baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
        ?? throw new InvalidOperationException("ApiSettings:BaseUrl missing.");
    client.BaseAddress = new Uri(baseUrl);
});

// 5. Register the API client wrapper
builder.Services.AddScoped<RaceDayApiClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();          // MUST come before MapControllerRoute
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();