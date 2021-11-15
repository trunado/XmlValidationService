using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XmlValidationService.Dtos;
using XmlValidationService.Models;
using XmlValidationService.Validation;

namespace XmlValidationService
{
	/// <summary>
	/// Provides methods for validating XML
	/// </summary>
	public class ServerResourceControl : IServerResourceControl
	{
		private readonly ILogger<ServerResourceControl> _logger;
		private readonly IList<SchemaSet> _schemas;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="logger">The logger</param>
		public ServerResourceControl(ILogger<ServerResourceControl> logger) : this(GetListOfSchemaSets(logger), logger)
		{

		}

		/// <summary>
		/// Internal constructor
		/// </summary>
		/// <param name="schemas">List of schema sets</param>
		/// <param name="logger">The logger</param>
		internal ServerResourceControl(IList<SchemaSet> schemas, ILogger<ServerResourceControl> logger)
		{
			_logger = logger;
			_schemas = schemas;
		}

		/// <summary>
		/// Reads the filesystem to gather a list of installed schema sets
		/// </summary>
		/// <param name="logger">The logger</param>
		/// <returns>A list of schema sets on the file system</returns>
		internal static IList<SchemaSet> GetListOfSchemaSets(ILogger<ServerResourceControl> logger)
		{
			IList<SchemaSet> schemas = new List<SchemaSet>();
			string schemaPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"XmlValidationService\Schemas");

			try
			{
				foreach (DirectoryInfo di in new DirectoryInfo(schemaPath).EnumerateDirectories())
				{
					SchemaSet set = new SchemaSet();
					set.Name = di.Name;
					set.Path = Path.GetDirectoryName(di.FullName);
					set.Schemas = new List<string>();

					foreach (FileInfo file in di.EnumerateFiles("*.xsd"))
					{
						set.Schemas.Add(file.Name);
					}

					schemas.Add(set);
				}
			}
			catch (DirectoryNotFoundException)
			{
				logger.LogInformation($"{schemaPath} does not exist.");
			}

			if (!schemas.Any())
			{
				logger.LogInformation($"No schema sets were found in {schemaPath}");
			}

			return schemas; ;
		}

		/// <summary>
		/// Gets the list of schema sets
		/// </summary>
		/// <param name="schemaset">The list of schema sets</param>
		/// <returns>Always returns true</returns>
		public bool TryGetSchemaSets(out IList<SchemaSetDescriptorDto> schemaset)
		{
			schemaset = _schemas.Select(x => new SchemaSetDescriptorDto(x.Name)).ToList();
			return true;
		}

		/// <summary>
		/// Tries to get a single schema set based on the name passed in
		/// </summary>
		/// <param name="name">The name of the requested schema set</param>
		/// <param name="schemaset">The schema set that was requested</param>
		/// <returns>True if the requested schema set exists, false otherwise</returns>
		public bool TryGetSchemaSet(string name, out SchemaSetDto schemaset)
		{
			schemaset = _schemas
				.Where(x => string.Compare(x.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
				.Select(x => new SchemaSetDto(x.Name, x.Path, x.Schemas))
				.FirstOrDefault();

			return schemaset != null;
		}

		/// <summary>
		/// Test to see if a specific schema exists within the given schema set
		/// </summary>
		/// <param name="name">Name of the schema set</param>
		/// <param name="schema">Name of the schema</param>
		/// <returns>True if the schema exists in the schema set, false otherwise</returns>
		public bool TryGetSchema(string name, string schema)
		{
			bool found = false;

			SchemaSet set = _schemas.
				Where(x => string.Compare(x.Name, name, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();

			if (set != null)
			{
				found = set.Schemas.Where(s => string.Compare(s, schema, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault() != null;
			}

			return found;
		}

		/// <summary>
		/// Tries to validate the given xml file with the given schema file
		/// </summary>
		/// <param name="xmlPath">Path to the XML file</param>
		/// <param name="schemaPath">Path to the schema file</param>
		/// <param name="errors">List of validation errors</param>
		/// <returns>True if valid, false otherwise</returns>
		public bool TryValidate(string xmlPath, string schemaPath, out List<string> errors)
		{
			// Validate the schema
			XmlValidator val = new XmlValidator(_logger);
			if (!val.Validate(xmlPath, schemaPath))
			{
				errors = val.Errors;
				return false;
			}

			errors = null;
			return true;
		}
	}
}
