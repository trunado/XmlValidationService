namespace XmlValidationService.Configurations
{
	/// <summary>
	/// Contains settings for enabling swagger
	/// </summary>
	public class DiagnosticSettings
	{
		/// <summary>
		/// Environment variable name which when defined will cause the Swagger UI to be shown
		/// </summary>
		public string EnvironmentVariableToEnableSwagger { get; set; }
	}
}