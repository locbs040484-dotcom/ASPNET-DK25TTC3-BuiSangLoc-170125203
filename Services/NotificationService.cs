using LaptopStore.Data;
using LaptopStore.Models;
using LaptopStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;

    public NotificationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task NotifyAsync(string userId, string title, string message, string? link = null)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Link = link
        });
        await _db.SaveChangesAsync();
    }

    public Task<List<Notification>> GetLatestAsync(string userId, int take = 8)
        => _db.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();

    public Task<int> UnreadCountAsync(string userId)
        => _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task MarkReadAsync(string userId, int id)
    {
        var item = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (item == null) return;
        item.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(string userId)
    {
        var items = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var item in items)
            item.IsRead = true;
        await _db.SaveChangesAsync();
    }
}
