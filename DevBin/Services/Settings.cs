using DevBin.Data;
using Microsoft.EntityFrameworkCore;

namespace DevBin.Services;
public class Settings
{
    private readonly ApplicationDbContext _context;
    public Settings(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetValueAsync<T>(string key)
    {
        var setting = await _context.Settings.FirstOrDefaultAsync(q => q.Name == key);
        if (setting == null)
            return default;

        return setting.GetValue<T>();
    }

    public async Task SetValueAsync<T>(string key, T value)
    {
        var rawValue = Convert.ToString(value);
        var setting = await _context.Settings.FirstOrDefaultAsync(q => q.Name == key);
        if (setting == null)
            setting = new()
            {
                Name = key,
                Enable = true,
            };

        setting.RawValue = rawValue;
        _context.Settings.Update(setting);
        await _context.SaveChangesAsync();

    }

    public async Task<bool> HasAsync(string key)
    {
        return await _context.Settings.AnyAsync(q => q.Name == key && q.Enable);
    }

    public async Task DeleteAsync(string key)
    {
        var setting = _context.Settings.FirstOrDefault(q => q.Name == key);
        if (setting != null)
        {
            _context.Remove(setting);
            await _context.SaveChangesAsync();
        }
    }
}
