using System.Threading.Tasks;
using HeyImIn.Authentication.Impl;
using HeyImIn.Database.Context;
using HeyImIn.Database.Models;
using HeyImIn.Database.Tests;
using HeyImIn.Shared.Tests;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace HeyImIn.Authentication.Tests
{
	public class AuthenticationServiceTests : TestBase
	{
		[Theory]
		[InlineData(Email, Password, true)]
		[InlineData("wrong@email.com", Password, false)]
		[InlineData(Email, "WrongPassword", false)]
		[InlineData("User@email.com", Password, false)] // Email casing
		[InlineData(Email, "PASSWORD", false)] // Password casing
		public async Task Authenticate_GivenCredentails_VerifiesPasswordAndEmail(string email, string password, bool shouldWork)
		{
			GetDatabaseContext getContext = ContextUtilities.CreateInMemoryContext();

			// Arrange
			using (IDatabaseContext context = getContext())
			{
				EntityEntry<User> userEntry = context.Users.Add(ContextUtilities.JohnDoe);

				userEntry.Entity.Email = Email;
				userEntry.Entity.PasswordHash = PasswordHash;

				await context.SaveChangesAsync();
			}

			// Act
			var passwordServiceMock = new Mock<IPasswordService>();
			passwordServiceMock.Setup(p => p.NeedsRehash(PasswordHash)).Returns(false);
			passwordServiceMock.Setup(p => p.VerifyPassword(Password, PasswordHash)).Returns(true);
			passwordServiceMock.Setup(p => p.HashPassword(Password)).Returns(PasswordHash);

			var authenticationService = new AuthenticationService(passwordServiceMock.Object, getContext, DummyLogger<AuthenticationService>());
			(bool isAuthenticated, _) = await authenticationService.AuthenticateAsync(email, password);

			// Assert
			passwordServiceMock.Verify(p => p.VerifyPassword(password, PasswordHash), Times.AtMostOnce);

			Assert.Equal(isAuthenticated, shouldWork);
		}

		private const string PasswordHash = "PasswordHash";
		private const string Password = "password";
		private const string Email = "user@email.com";

		public AuthenticationServiceTests(ITestOutputHelper output) : base(output)
		{
		}
	}
}
