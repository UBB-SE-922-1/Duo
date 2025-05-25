using Duo.Api.Persistence;
using DuoClassLibrary.Services;
using DuoClassLibrary.Services.Interfaces;
using DuoClassLibrary.Repositories.Proxies;
using DuoClassLibrary.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Duo.Services;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

// Load API base URL from configuration
var apiBase = builder.Configuration["Api:BaseUrl"];
if (string.IsNullOrWhiteSpace(apiBase))
{
    throw new InvalidOperationException("Missing Api:BaseUrl in appsettings.json");
}

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add HttpClient configurations
builder.Services.AddHttpClient<IQuizServiceProxy, QuizServiceProxy>(client =>
{
    client.BaseAddress = new Uri(apiBase);
});

builder.Services.AddHttpClient<IExerciseServiceProxy, ExerciseServiceProxy>(client =>
{
    client.BaseAddress = new Uri(apiBase);
});

builder.Services.AddHttpClient<ISectionServiceProxy, SectionServiceProxy>(client =>
{
    client.BaseAddress = new Uri(apiBase);
});

// Configure session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register repositories using proxies
builder.Services.AddScoped<IUserRepository, UserRepositoryProxy>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepositoryProxi>();
builder.Services.AddScoped<IPostRepository, PostRepositoryProxi>();
builder.Services.AddScoped<IHashtagRepository, HashtagRepositoryProxi>();
builder.Services.AddScoped<ICommentRepository, CommentRepositoryProxi>();

// Register services
builder.Services.AddScoped<IUserHelperService, UserHelperService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<SignUpService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IHashtagService, HashtagService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IRoadmapService, RoadmapService>();
builder.Services.AddScoped<ISectionService, SectionService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();

// Register service proxies
builder.Services.AddScoped<IRoadmapServiceProxy, RoadmapServiceProxy>();
builder.Services.AddScoped<ISectionServiceProxy, SectionServiceProxy>();
builder.Services.AddScoped<IQuizServiceProxy, QuizServiceProxy>();
builder.Services.AddScoped<IExerciseServiceProxy, ExerciseServiceProxy>();

// Configure Razor Views location
builder.Services.Configure<RazorViewEngineOptions>(opts =>
{
    opts.ViewLocationFormats.Add("/Views/Exercise/{0}.cshtml");
    opts.ViewLocationFormats.Add("~/Views/Exercise/{0}.cshtml");
});

// Add CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policyBuilder =>
    {
        policyBuilder.WithOrigins("https://localhost:7037")
                     .AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials()
                     .WithExposedHeaders("Content-Type", "Accept");
    });
});

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<DataContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session before authentication
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "quiz",
    pattern: "Quiz/{action}/{id}",
    defaults: new { controller = "Quiz" });

app.MapControllerRoute(
    name: "exam",
    pattern: "Exam/{action}/{id}",
    defaults: new { controller = "Exam" });

app.MapControllerRoute(
    name: "courses",
    pattern: "Course/{action=ViewCourses}/{id?}");

app.MapControllerRoute(
    name: "exercises",
    pattern: "Exercise/{action=Index}/{id?}",
    defaults: new { controller = "Exercise" });

app.MapControllerRoute(
    name: "coursePreview",
    pattern: "Course/{id?}",
    defaults: new { controller = "Course", action = "CoursePreview" });

app.MapControllerRoute(
    name: "module",
    pattern: "Module/{id:int}",
    defaults: new { controller = "Module", action = "Details" });

app.MapRazorPages();

app.Run();
