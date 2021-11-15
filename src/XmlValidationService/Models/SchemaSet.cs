using System.Collections.Generic;

namespace XmlValidationService.Models
{
	/// <summary>
	/// Model for a single schema set
	/// </summary>
	public class SchemaSet
	{
		/// <summary>
		/// Name of the schema set
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Path to the schema set
		/// </summary>
		public string Path { get; set; }
		/// <summary>
		/// List of .xsd file names
		/// </summary>
		public IList<string> Schemas { get; set; }
	}
}
