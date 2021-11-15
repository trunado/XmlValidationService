using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace XmlValidationService
{
	/// <summary>
	/// Main program
	/// </summary>
	[ExcludeFromCodeCoverage]
	internal class Program
	{
		/// <summary>
		/// Main method
		/// </summary>
		/// <param name="args">Arguments</param>
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		/// <summary>
		/// Creates the hose
		/// </summary>
		/// <param name="args">Arguments</param>
		/// <returns>The hose</returns>
		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureLogging((hostingContext, logging) =>
				{
					logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					logging.AddDebug();
					logging.AddNLog();
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				})
				.UseWindowsService()
				.ConfigureServices((hostContext, services) =>
				{
					IConfiguration configuration = hostContext.Configuration;
					//OperatingDirectorySettings operatingDirectoryPrunerSettings = configuration.GetSection("OperatingDirectory").Get<OperatingDirectorySettings>();
					//services.AddSingleton(operatingDirectoryPrunerSettings);
				});
		}
	}
}
