using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TravelPlannerApp.Data;
using TravelPlannerApp.Models;

namespace TravelPlannerApp.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    public AuthService(AppDbContext db) => _db = db;

    public static string HashPassword(string password)
    {
        var salt = "ZPI_DEMO_SALT";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password + salt));
        return Convert.ToHexString(bytes);
    }

    public async Task<AppUser?> ValidateLoginAsync(string email, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;

        // Jeśli konto jest zablokowane, zwracamy null, aby Program.cs obsłużył blokadę przekierowaniem
        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.Now) 
        {
            return null; 
        }

        // Poprawne hasło
        if (user.PasswordHash == HashPassword(password))
        {
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
            await _db.SaveChangesAsync();
            return user;
        }

        // Błędne hasło - zwiększamy licznik i zapisujemy w bazie
        user.FailedLoginAttempts++;
        if (user.FailedLoginAttempts >= 5)
        {
            user.LockedUntil = DateTime.Now.AddMinutes(15);
            user.FailedLoginAttempts = 0; // Resetujemy licznik, bo blokada została już nałożona
        }

        await _db.SaveChangesAsync();
        return null;
    }

    public async Task<AppUser> EnsureDemoUserAsync()
    {
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == "demo@travel.pl");
        if (existing != null) return existing;
        var user = new AppUser { Email = "demo@travel.pl", PasswordHash = HashPassword("Demo123!") };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }
}