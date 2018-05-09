using HeyImIn.Authentication.Impl;
using Xunit;

namespace HeyImIn.Authentication.Tests
{
	public class PasswordServiceTests
	{
		private const string InputPassword = "Password to be hashed";
		private const int WorkFactor = 10;

		[Fact]
		public void HashedPasswordCanBeVerified()
		{
			var passwordService = new PasswordService(WorkFactor);
			string hash = passwordService.HashPassword(InputPassword);
			bool isVerified = passwordService.VerifyPassword(InputPassword, hash);
			Assert.True(isVerified);
		}

		[Fact]
		public void WrongHashIsNotVerified()
		{
			var passwordService = new PasswordService(WorkFactor);
			string wrongHash = passwordService.HashPassword(InputPassword + "random password addition");
			bool isVerified = passwordService.VerifyPassword(InputPassword, wrongHash);
			Assert.False(isVerified);
		}

		[Fact]
		public void SameWorkFactorDoesNotNeedRehash()
		{
			var passwordService = new PasswordService(WorkFactor);
			var passwordService2 = new PasswordService(WorkFactor);
			string hash = passwordService.HashPassword(InputPassword);
			bool needsRehash = passwordService2.NeedsRehash(hash);
			Assert.False(needsRehash);
		}

		[Fact]
		public void SmallerWorkFactorDoesNotNeedNeedsRehash()
		{
			var passwordService = new PasswordService(WorkFactor);
			var passwordService2 = new PasswordService(WorkFactor - 1);
			string hash = passwordService.HashPassword(InputPassword);
			bool needsRehash = passwordService2.NeedsRehash(hash);
			Assert.False(needsRehash);
		}

		[Fact]
		public void BiggerWorkFactorNeedsRehash()
		{
			var passwordService = new PasswordService(WorkFactor);
			var passwordService2 = new PasswordService(WorkFactor + 1);
			string hash = passwordService.HashPassword(InputPassword);
			bool needsRehash = passwordService2.NeedsRehash(hash);
			Assert.True(needsRehash);
		}
	}
}
