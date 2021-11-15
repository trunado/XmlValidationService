using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace XmlValidationService.Dtos
{
	/// <summary>
	/// Represents a schema set, with a name and a list of .xsd files.
	/// </summary>
	[DataContract(Name = "SchemaSetDto", Namespace = "XmlValidationService.Dtos")]
	public class SchemaSetDto
	{
		/// <summary>
		/// A schema set
		/// </summary>
		/// <param name="name">The name of the schema set</param>
		/// <param name="path">The path of the schema set</param>
		/// <param name="schemas">List of .xsd file names</param>
		public SchemaSetDto(string name, string path, IList<string> schemas)
		{
			Name = name;
			Schemas = schemas;
			Path = path;
		}

		/// <summary>
		/// Name of the schema set
		/// </summary>
		[DataMember()]
		public string Name { get; }

		/// <summary>
		/// Full path to the directory the schema set is in
		/// </summary>
		[DataMember]
		public string Path { get; }

		/// <summary>
		/// List of .xsd file names
		/// </summary>
		[DataMember()]
		public IList<String> Schemas { get; }
	}
}
