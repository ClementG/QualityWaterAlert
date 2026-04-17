using System;
using System.Net.Mail;
using System.Threading.Tasks;
using NUnit.Framework;
using QualityWaterAlert.Infrastructure.Services;

namespace QualityWaterAlert.Infrastructure.Tests.Services
{
    [TestFixture]
    public class EmailServiceTests
    {
        [Test]
        public void Constructor_NullSmtpServer_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EmailService(null!, 587, "test@test.com", "Test Sender"));
        }

        [Test]
        public void Constructor_NullFromEmail_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EmailService("smtp.test.com", 587, null!, "Test Sender"));
        }

        [Test]
        public void Constructor_NullSenderName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EmailService("smtp.test.com", 587, "test@test.com", null!));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void SendEmailAsync_InvalidToAddress_ThrowsArgumentException(string? invalidTo)
        {
            var service = new EmailService("smtp.test.com", 587, "test@test.com", "Test Sender");
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await service.SendEmailAsync(invalidTo!, "Subject", "Body"));
            Assert.That(ex.ParamName, Is.EqualTo("to"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void SendEmailAsync_InvalidSubject_ThrowsArgumentException(string? invalidSubject)
        {
            var service = new EmailService("smtp.test.com", 587, "test@test.com", "Test Sender");
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await service.SendEmailAsync("recipient@test.com", invalidSubject!, "Body"));
            Assert.That(ex.ParamName, Is.EqualTo("subject"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void SendEmailAsync_InvalidBody_ThrowsArgumentException(string? invalidBody)
        {
            var service = new EmailService("smtp.test.com", 587, "test@test.com", "Test Sender");
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await service.SendEmailAsync("recipient@test.com", "Subject", invalidBody!));
            Assert.That(ex.ParamName, Is.EqualTo("body"));
        }
    }
}
