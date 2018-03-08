using System;

namespace HeyImIn.External.DependencyInjection
{
	/// <summary>
	///     Use attribute to mark types that shall be registered as self with the DI container
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class RegisterAsSelfAttribute : Attribute
	{
	}
}
