namespace DevBin.Services;
public class SMTPConfig
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string MailAddress { get; set; }
    public string Name { get; set; }
    public bool UseSSL { get; set; }
}
