using System.ComponentModel.DataAnnotations;

namespace DevBin.Models;
public class Setting
{
    [Key]
    public string Name { get; set; }
    public string? RawValue { get; set; }
    public bool Enable { get; set; }

    public T? GetValue<T>()
    {
        return (T?)Convert.ChangeType(RawValue, typeof(T));
    }
}
