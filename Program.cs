using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TravelPlannerApp.Data;
using TravelPlannerApp.Models;
using TravelPlannerApp.Services;
using TravelPlannerApp; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddHubOptions(options => options.MaximumReceiveMessageSize = 50 * 1024 * 1024);
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=travelplanner.db"));
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PdfExportService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => { options.LoginPath = "/login"; options.LogoutPath = "/api/auth/logout"; options.AccessDeniedPath = "/login"; });
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Bezpieczny endpoint autoryzacyjny wywoływany tylko po poprawnej walidacji w komponencie Blazor
app.MapGet("/api/auth/login-success", async (HttpContext http, AuthService auth, string email, string password) =>
{
    var user = await auth.ValidateLoginAsync(email, password);
    if (user == null) return Results.Redirect("/login");

    var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id.ToString()), new(ClaimTypes.Email, user.Email), new(ClaimTypes.Name, user.Email) };
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
    return Results.Redirect("/");
}).AllowAnonymous();

app.MapGet("/api/auth/logout", async (HttpContext http) => 
{ 
    await http.SignOutAsync(); 
    return Results.Redirect("/login"); 
});

app.MapGet("/api/documents/{id:int}", async (int id, AppDbContext db, ClaimsPrincipal user) =>
{
    if (!int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Results.Unauthorized();
    var doc = await db.Documents.Include(d => d.Trip).ThenInclude(t => t.TripUsers).FirstOrDefaultAsync(d => d.Id == id);
    if (doc == null || !doc.Trip.TripUsers.Any(tu => tu.UserId == userId)) return Results.NotFound();
    return Results.File(doc.StoredPath, "application/pdf", doc.FileName, enableRangeProcessing: true);
}).RequireAuthorization();

app.MapGet("/api/packing/{id:int}/export", async (int id, AppDbContext db, PdfExportService pdf, ClaimsPrincipal user) =>
{
    if (!int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Results.Unauthorized();
    var list = await db.PackingLists.Include(l => l.Items).Include(l => l.Trip).ThenInclude(t => t.TripUsers).FirstOrDefaultAsync(l => l.Id == id);
    if (list == null || !list.Trip.TripUsers.Any(tu => tu.UserId == userId)) return Results.NotFound();
    return Results.File(pdf.CreatePackingPdf(list), "application/pdf", $"pakowanie_{list.Id}.pdf");
}).RequireAuthorization();

app.MapBlazorHub();
app.MapRazorPages();
await SeedData.InitializeAsync(app.Services);
app.Run();