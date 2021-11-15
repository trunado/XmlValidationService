using System.Runtime.Serialization;

namespace XmlValidationService.Dtos
{
	/// <summary>
	/// Represents the name of a single schema set returned to the caller
	/// </summary>
	[DataContract(Name = "SchemaSetDescriptorDto", Namespace = "XmlValidationService.Dtos")]
	public class SchemaSetDescriptorDto
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">The name of the schema set</param>
		public SchemaSetDescriptorDto(string name)
		{
			Name = name;
		}

		/// <summary>
		/// Gets the name of the schema set
		/// </summary>
		[DataMember()]
		public string Name { get; }
	}
}
