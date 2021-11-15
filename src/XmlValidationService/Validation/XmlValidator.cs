using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace XmlValidationService.Validation
{
	/// <summary>
	/// Class to validate XML files
	/// </summary>
	public class XmlValidator
	{
		private ILogger<ServerResourceControl> _logger;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="logger">Logger</param>
		public XmlValidator(ILogger<ServerResourceControl> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Validates an XML file against a schema
		/// </summary>
		/// <param name="xmlPath">Path to the XML file</param>
		/// <param name="schemaPath">Path to the schema</param>
		/// <returns>True if it's valid, false otherwise</returns>
		public bool Validate(string xmlPath, string schemaPath)
		{
			Errors = new List<string>();

			XmlSchemaSet set = new XmlSchemaSet();
			set.Add(null, schemaPath);

			// TODO: This is only catching the first validation error, update to log them all.
			try
			{
				XmlDocument document = new XmlDocument();
				document.Schemas = set;
				document.Load(xmlPath);

				ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);

				document.Validate(eventHandler);
			}
			catch (XmlSchemaValidationException ex)
			{
				_logger.LogInformation($"{nameof(XmlValidator)} found the following XML validation errors: {ex.Message}");
				Errors.Add(ex.Message);
			}
			catch(XmlException ex)
			{
				_logger.LogInformation($"{nameof(XmlValidator)} found the following XML validation errors: {ex.Message}");
				Errors.Add(ex.Message);
			}

			return Errors.Count == 0;
		}

		/// <summary>
		/// Event handler called when a validation issue occurs
		/// </summary>
		/// <param name="sender">Sender of the event</param>
		/// <param name="e">The event arguments</param>
		private void ValidationEventHandler(object sender, ValidationEventArgs e)
		{
			switch (e.Severity)
			{
				case XmlSeverityType.Error:
					Errors.Add(e.Message);
					_logger.LogInformation($"{nameof(XmlValidator)} found the following XML validation errors: {e.Message}");
					break;
				case XmlSeverityType.Warning:
					_logger.LogInformation($"{nameof(XmlValidator)} found the following XML validation warning: {e.Message}");
					break;
			}
		}

		/// <summary>
		/// Gets a list of validations errors
		/// </summary>
		public List<string> Errors { get; private set; }
	}
}
