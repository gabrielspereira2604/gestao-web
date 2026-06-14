using GestaoWeb.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GestaoWeb.Tests.Services;

public class EmailServiceTests
{
    private static EmailService CreateService(string smtpHost = "")
    {
        var settings = Options.Create(new SmtpSettings
        {
            Host = smtpHost,
            Port = 587,
            User = "user@test.com",
            Password = "password",
            From = "from@test.com"
        });
        return new EmailService(settings, NullLogger<EmailService>.Instance);
    }

    [Fact]
    public async Task SendAsync_DoesNotThrow_WhenSmtpHostIsEmpty()
    {
        var service = CreateService(smtpHost: "");

        var exception = await Record.ExceptionAsync(
            () => service.SendAsync("to@test.com", "Subject", "<p>Body</p>"));

        Assert.Null(exception);
    }

    [Fact]
    public async Task SendAsync_DoesNotThrow_WhenSmtpHostIsWhitespace()
    {
        var service = CreateService(smtpHost: "   ");

        var exception = await Record.ExceptionAsync(
            () => service.SendAsync("to@test.com", "Subject", "<p>Body</p>"));

        Assert.Null(exception);
    }
}
