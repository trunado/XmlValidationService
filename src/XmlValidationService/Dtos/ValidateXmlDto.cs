using System.Runtime.Serialization;

namespace XmlValidationService.Dtos
{
	/// <summary>
	/// Represents a schema set, with a name and a list of .xsd files.
	/// </summary>
	[DataContract(Name = "ValidateXmlDto", Namespace = "XmlValidationService.Dtos")]
	public class ValidateXmlDto
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public ValidateXmlDto()
		{

		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="setName">Name of the schema set</param>
		/// <param name="schemaName">Name of the schema</param>
		/// <param name="xmlPath">Path to the XML file</param>
		public ValidateXmlDto(string setName, string schemaName, string xmlPath)
		{
			SetName = setName;
			Schema = schemaName;
			XmlFilePath = xmlPath;
		}

		/// <summary>
		/// The set name that the schema belongs to
		/// </summary>
		[DataMember]
		public string SetName { get; set; }

		/// <summary>
		/// The name (not full path) of the schema
		/// </summary>
		[DataMember]
		public string Schema { get; set; }

		/// <summary>
		/// The full path to the XML file that needs validation
		/// </summary>
		[DataMember]
		public string XmlFilePath { get; set; }
	}
}
