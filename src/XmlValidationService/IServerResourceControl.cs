using System.Collections.Generic;
using XmlValidationService.Dtos;

namespace XmlValidationService
{
	/// <summary>
	/// Provides methods for validating XML
	/// </summary>
	public interface IServerResourceControl
	{
		/// <summary>
		/// Gets the list of schema sets
		/// </summary>
		/// <param name="schemaset">The list of schema sets</param>
		/// <returns>Always returns true</returns>
		bool TryGetSchemaSets(out IList<SchemaSetDescriptorDto> schemaset);

		/// <summary>
		/// Tries to get a single schema set based on the name passed in
		/// </summary>
		/// <param name="name">The name of the requested schema set</param>
		/// <param name="schemaset">The schema set that was requested</param>
		/// <returns>True if the requested schema set exists, false otherwise</returns>
		bool TryGetSchemaSet(string name, out SchemaSetDto schemaset);

		/// <summary>
		/// Test to see if a specific schema exists within the given schema set
		/// </summary>
		/// <param name="name">Name of the schema set</param>
		/// <param name="schema">Name of the schema</param>
		/// <returns>True if the schema exists in the schema set, false otherwise</returns>
		bool TryGetSchema(string name, string schema);

		/// <summary>
		/// Test to see if the given XML file is valid woth the given schema
		/// </summary>
		/// <param name="xmlPath">Path to the XML file</param>
		/// <param name="schemaPath">Path to the schema file</param>
		/// <param name="errors">List of validation errors</param>
		/// <returns></returns>
		bool TryValidate(string xmlPath, string schemaPath, out List<string> errors);
	}
}
