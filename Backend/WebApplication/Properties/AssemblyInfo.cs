using System.Reflection;

[assembly: AssemblyTitle("Hey I'm in")]

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]