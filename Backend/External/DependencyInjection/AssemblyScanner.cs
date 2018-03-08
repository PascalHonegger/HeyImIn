using System;
using System.Linq;
using System.Reflection;
using Autofac;

namespace HeyImIn.External.DependencyInjection
{
	/// <summary>
	///     A simple assembly scanner taken from the MediaGateway project
	/// </summary>
	public static class AssemblyScanner
	{
		public static void Scan(ContainerBuilder builder, params Assembly[] assemblies)
		{
			// register non-singleton types with name matching interfaces
			builder.RegisterAssemblyTypes(assemblies)
				.Where(t => (GetNameMatchingInterfaces(t).Length == 1) && !HasSingletonAttribute(t) && !HasDoNotRegisterAttribute(t) && !IsExcludedName(t))
				.As(t => GetNameMatchingInterfaces(t))
				.PreserveExistingDefaults();

			// register singleton types with name matching interfaces
			builder.RegisterAssemblyTypes(assemblies)
				.Where(t => (GetNameMatchingInterfaces(t).Length == 1) && HasSingletonAttribute(t) && !HasDoNotRegisterAttribute(t) && !IsExcludedName(t))
				.As(t => GetNameMatchingInterfaces(t))
				.SingleInstance()
				.PreserveExistingDefaults();

			// register non-singleton types marked the type with the RegisterAsSelf attribute
			builder.RegisterAssemblyTypes(assemblies)
				.Where(t => HasRegisterAsSelfAttribute(t) && !HasSingletonAttribute(t) && !HasDoNotRegisterAttribute(t) && !IsExcludedName(t))
				.AsSelf()
				.PreserveExistingDefaults();

			// register singleton types marked the type with the RegisterAsSelf attribute
			builder.RegisterAssemblyTypes(assemblies)
				.Where(t => HasRegisterAsSelfAttribute(t) && HasSingletonAttribute(t) && !HasDoNotRegisterAttribute(t) && !IsExcludedName(t))
				.AsSelf()
				.SingleInstance()
				.PreserveExistingDefaults();

			// register modules, allow modules to do more complex registrations themselves
			builder.RegisterAssemblyModules(assemblies); // -> calls Module.Load() of all detected modules
		}

		private static bool HasSingletonAttribute(Type type)
		{
			return type.GetCustomAttribute<SingletonAttribute>() != null;
		}

		private static bool HasDoNotRegisterAttribute(Type type)
		{
			return type.GetCustomAttribute<DoNotRegisterAttribute>() != null;
		}

		private static bool HasRegisterAsSelfAttribute(Type type)
		{
			return type.GetCustomAttribute<RegisterAsSelfAttribute>() != null;
		}

		private static Type[] GetNameMatchingInterfaces(Type type)
		{
			return type.GetInterfaces().Where(i => type.Name == i.Name.Substring(1)).ToArray();
		}

		private static bool IsExcludedName(Type type)
		{
			return type.Name.EndsWith("Attribute", StringComparison.Ordinal) || type.Name.EndsWith("Module", StringComparison.Ordinal);
		}
	}
}
