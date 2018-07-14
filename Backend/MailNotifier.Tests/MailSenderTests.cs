using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HeyImIn.MailNotifier.Impl;
using HeyImIn.Shared.Tests;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using Xunit;
using Xunit.Abstractions;

namespace HeyImIn.MailNotifier.Tests
{
	public class MailSenderTests : TestBase
	{
		public MailSenderTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task GivenEmail_SendGridApiCalledCorrectly()
		{
			// Arrange
			var mock = new Mock<ISendGridClient>();
			var recipients = new List<string> { "max.muster@email.com", "foo@bar.com" };
			SendGridMessage sentMessage = null;
			mock.Setup(m => m.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
				.Callback<SendGridMessage, CancellationToken>((m, _) => sentMessage = m)
				.ReturnsAsync(new Response(HttpStatusCode.OK, null, null));

			// Act
			var mailSender = new MailSender(mock.Object, DummyLogger<MailSender>());
			const string Subject = "Subject text";
			const string Body = "Body text";
			await mailSender.SendMailAsync(recipients, Subject, Body);

			// Assert
			mock.Verify(m => m.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
			Assert.NotNull(sentMessage);

			Assert.Null(sentMessage.HtmlContent);
			Assert.Equal(Body, sentMessage.PlainTextContent);
			Assert.Single(sentMessage.Personalizations);

			Personalization emailToBeSent = sentMessage.Personalizations[0];
			Assert.NotNull(emailToBeSent);
			Assert.Equal(Subject, emailToBeSent.Subject);
			Assert.Equal(recipients, emailToBeSent.Tos.Select(emailAddress => emailAddress.Email));
			Assert.Null(emailToBeSent.Bccs);
			Assert.Null(emailToBeSent.Ccs);
		}
	}
}
