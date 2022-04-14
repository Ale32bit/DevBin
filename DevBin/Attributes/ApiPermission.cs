namespace DevBin.Attributes;
[Flags]
public enum ApiPermission
{
    None = 0,
    Get = 1,
    Create = 2,
    Update = 4,
    Delete = 8,
    GetUser = 16,
    CreateFolder = 32,
    DeleteFolder = 64,
}
