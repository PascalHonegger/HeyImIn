using HeyImIn.Authentication.Impl;
using HeyImIn.Shared;
using HeyImIn.Shared.Tests;
using Xunit;
using Xunit.Abstractions;

namespace HeyImIn.Authentication.Tests
{
	public class PasswordServiceTests : TestBase
	{
		public PasswordServiceTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public void HashedPasswordCanBeVerified()
		{
			var passwordService = new PasswordService(new HeyImInConfiguration());
			string hash = passwordService.HashPassword(InputPassword);
			bool isVerified = passwordService.VerifyPassword(InputPassword, hash);
			Assert.True(isVerified);
		}

		[Fact]
		public void WrongHashIsNotVerified()
		{
			var passwordService = new PasswordService(new HeyImInConfiguration());
			string wrongHash = passwordService.HashPassword(InputPassword + "random password addition");
			bool isVerified = passwordService.VerifyPassword(InputPassword, wrongHash);
			Assert.False(isVerified);
		}

		[Fact]
		public void SameWorkFactorDoesNotNeedRehash()
		{
			var config = new HeyImInConfiguration();
			var passwordService = new PasswordService(config);
			var passwordService2 = new PasswordService(config);
			string hash = passwordService.HashPassword(InputPassword);
			bool needsRehash = passwordService2.NeedsRehash(hash);
			Assert.False(needsRehash);
		}

		[Fact]
		public void SmallerWorkFactorDoesNotNeedNeedsRehash()
		{
			var passwordService = new PasswordService(new HeyImInConfiguration { PasswordHashWorkFactor = 10 });
			var passwordService2 = new PasswordService(new HeyImInConfiguration { PasswordHashWorkFactor = 9 });
			string hash = passwordService.HashPassword(InputPassword);
			bool needsRehash = passwordService2.NeedsRehash(hash);
			Assert.False(needsRehash);
		}

		[Fact]
		public void BiggerWorkFactorNeedsRehash()
		{
			var passwordService = new PasswordService(new HeyImInConfiguration { PasswordHashWorkFactor = 10 });
			var passwordService2 = new PasswordService(new HeyImInConfiguration { PasswordHashWorkFactor = 11 });
			string hash = passwordService.HashPassword(InputPassword);
			bool needsRehash = passwordService2.NeedsRehash(hash);
			Assert.True(needsRehash);
		}

		private const string InputPassword = "Password to be hashed";
	}
}
