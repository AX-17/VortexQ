using System.Net;
using System.Net.Mail;
using System.Text;

namespace Vortex.Bot.Utility;

public class MailUtility : IDisposable
{
    public string Host { get; }
    public int Port { get; }
    public string Password { get; }
    public bool EnableSsl { get; }
    public string Username { get; private set; } = "";

    private readonly SmtpClient _client;
    private readonly MailMessage _mail = new();

    public MailUtility(string host, int port, string password, bool enableSsl)
    {
        Host = host;
        Port = port;
        Password = password;
        EnableSsl = enableSsl;
        _client = new SmtpClient(host, port)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            EnableSsl = enableSsl,
            Timeout = 30000
        };
    }

    public static MailUtility Builder(string host, int port, string password, bool enableSsl) => new(host, port, password, enableSsl);

    public MailUtility SetTitle(string title)
    {
        _mail.Subject = title;
        _mail.SubjectEncoding = Encoding.UTF8;
        return this;
    }

    public MailUtility SetBody(string body)
    {
        _mail.Body = body;
        _mail.BodyEncoding = Encoding.UTF8;
        return this;
    }

    public MailUtility AddTarget(string target)
    {
        _mail.To.Add(target);
        return this;
    }

    public MailUtility AddAttachment(string path)
    {
        var attach = new Attachment(path);
        var disposition = attach.ContentDisposition!;
        disposition.CreationDate = File.GetCreationTime(path);
        disposition.ModificationDate = File.GetLastWriteTime(path);
        disposition.ReadDate = File.GetLastAccessTime(path);
        _mail.Attachments.Add(attach);
        return this;
    }

    public MailUtility SetSender(string sender, string? senderName = null)
    {
        _mail.From = senderName != null ? new MailAddress(sender, senderName, Encoding.UTF8) : new MailAddress(sender);
        Username = sender;
        return this;
    }

    public MailUtility EnableHtmlBody(bool enable)
    {
        _mail.IsBodyHtml = enable;
        return this;
    }

    public void Send()
    {
        if (_mail.From == null)
            throw new InvalidOperationException("发件人未设置");

        if (_mail.To.Count == 0)
            throw new InvalidOperationException("收件人未设置");
        var credentialUser = string.IsNullOrEmpty(Username) ? _mail.From.Address : Username;
        _client.Credentials = new NetworkCredential(credentialUser, Password);

        _client.Send(_mail);
    }

    public async Task SendAsync(CancellationToken cancellationToken = default)
    {
        if (_mail.From == null)
            throw new InvalidOperationException("发件人未设置");

        if (_mail.To.Count == 0)
            throw new InvalidOperationException("收件人未设置");

        var credentialUser = string.IsNullOrEmpty(Username) ? _mail.From.Address : Username;
        _client.Credentials = new NetworkCredential(credentialUser, Password);

        await _client.SendMailAsync(_mail, cancellationToken);
    }

    public void Dispose()
    {
        _mail.Dispose();
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}
