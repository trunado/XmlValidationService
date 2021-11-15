using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace XmlValidationService.GlobalExceptionHandler
{
	/// <summary>
	/// Provides a method to configure the exception handler for the application
	/// </summary>
	public static class ExceptionMiddlewareExtensions
	{
		/// <summary>
		/// Configures the exception handler
		/// </summary>
		/// <param name="app">The application</param>
		public static void ConfigureExceptionHandler(this IApplicationBuilder app)
		{
			app.UseExceptionHandler(appError =>
			{
				appError.Run(async context =>
							{
								context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
								context.Response.ContentType = "application/json";
								IExceptionHandlerFeature contextFeature = context.Features.Get<IExceptionHandlerFeature>();
								if (contextFeature != null)
								{
									await context.Response.WriteAsync(new ErrorDetails()
									{
										StatusCode = context.Response.StatusCode,
										Message = "Internal Server Error."
									}.ToString());
								}
							});
			});
		}
	}
}