using Newtonsoft.Json;

namespace XmlValidationService.GlobalExceptionHandler
{
	/// <summary>
	/// Details an error message
	/// </summary>
	public class ErrorDetails
	{
		/// <summary>
		/// Status code for the error
		/// </summary>
		public int StatusCode { get; set; }
		/// <summary>
		/// Error message
		/// </summary>
		public string Message { get; set; }
		/// <summary>
		/// Serializes this to a JSON string
		/// </summary>
		/// <returns>A JSON string representing the error message</returns>
		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}