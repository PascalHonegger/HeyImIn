using System;

namespace HeyImIn.External.DependencyInjection
{
	/// <summary>
	///     Use attribute to mark types that shall not be registered with the DI container
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DoNotRegisterAttribute : Attribute
	{
	}
}
