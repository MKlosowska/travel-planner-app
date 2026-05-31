using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TravelPlannerApp.Models;

public enum TripRole { Owner = 1, Collaborator = 2 }

public class AppUser
{
    public int Id { get; set; }
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    public bool PushNotificationsEnabled { get; set; } = true;
    
    [JsonIgnore]
    public List<TripUser> TripUsers { get; set; } = new();
}

public class PasswordResetCode
{
    public int Id { get; set; }
    public int UserId { get; set; }
    
    [JsonIgnore]
    public AppUser User { get; set; } = default!;
    
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool Used { get; set; }
}

public class Trip
{
    public int Id { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(3);
    public decimal? BudgetAmount { get; set; }
    public string BudgetCurrency { get; set; } = "PLN";
    public bool IsDeleted { get; set; }
    
    [JsonIgnore]
    public List<TripUser> TripUsers { get; set; } = new();
    
    public List<Expense> Expenses { get; set; } = new();
    public List<DocumentFile> Documents { get; set; } = new();
    public List<PhotoFile> Photos { get; set; } = new();
    public List<LocationPoint> Locations { get; set; } = new();
    public List<PackingList> PackingLists { get; set; } = new();
}

public class TripUser
{
    public int Id { get; set; }
    public int TripId { get; set; }
    
    [JsonIgnore]
    public Trip Trip { get; set; } = default!;
    
    public int UserId { get; set; }
    
    [JsonIgnore]
    public AppUser User { get; set; } = default!;
    
    public TripRole Role { get; set; }
}

public class Expense
{
    public int Id { get; set; }
    public int TripId { get; set; }
    
    [JsonIgnore]
    public Trip Trip { get; set; } = default!;
    
    [Range(0.01, 999999)] public decimal Amount { get; set; }
    public string Currency { get; set; } = "PLN";
    [Required] public string Category { get; set; } = "Inne";
    [Required] public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;
}

public class DocumentFile
{
    public int Id { get; set; }
    public int TripId { get; set; }
    
    [JsonIgnore]
    public Trip Trip { get; set; } = default!;
    
    public int OwnerId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredPath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.Now;
}

public class PhotoFile
{
    public int Id { get; set; }
    public int TripId { get; set; }
    
    [JsonIgnore]
    public Trip Trip { get; set; } = default!;
    
    public int OwnerId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredPath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.Now;
}

public class LocationPoint
{
    public int Id { get; set; }
    public int TripId { get; set; }
    
    [JsonIgnore]
    public Trip Trip { get; set; } = default!;
    
    [Required] public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Tag { get; set; } = "Atrakcja";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class PackingList
{
    public int Id { get; set; }
    public int TripId { get; set; }
    
    [JsonIgnore]
    public Trip Trip { get; set; } = default!;
    
    [Required] public string Name { get; set; } = "Lista pakowania";
    public List<PackingItem> Items { get; set; } = new();
}

public class PackingItem
{
    public int Id { get; set; }
    public int PackingListId { get; set; }
    
    [JsonIgnore]
    public PackingList PackingList { get; set; } = default!;
    
    [Required] public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public bool IsPacked { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}