using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XmlValidationService.Dtos;
using XmlValidationService.Validation;

namespace XmlValidationService.Controllers
{
  /// <summary>
  /// Xml Validation Service
  /// </summary>
  [Produces("application/json")]
  [Route("v1")]
  [ApiController]
  public class XmlValidationServiceController : Controller
  {
    private readonly IServerResourceControl _serverResourceControl;
    private readonly ILogger<XmlValidationServiceController> _logger;

    /// <summary>
    /// Set resource control objects
    /// </summary>
    public XmlValidationServiceController(IConfiguration configRoot, ILogger<XmlValidationServiceController> logger, IServerResourceControl serverResourceControl)
    {
      _logger = logger;
      _serverResourceControl = serverResourceControl ?? throw new Exception($"A resource control is required but was not injected");
    }

    /// <summary>
    /// Gets a list of names all installed schema sets
    /// </summary>
    /// <returns>Names of all installed schema sets</returns>
    [HttpGet]
    [Route("Schemas")]
    [ApiExplorerSettings(GroupName = "Schemas")]
    [SwaggerResponse(StatusCodes.Status200OK, "A list of schema sets", typeof(List<SchemaSetDescriptorDto>))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
    public IActionResult GetSchemaSets()
    {
      _logger.LogInformation($"{nameof(GetSchemaSets)}) was called");
      if (!_serverResourceControl.TryGetSchemaSets(out IList<SchemaSetDescriptorDto> schemas))
      {
        throw new Exception($"There was a problem finding schema sets");
      }

      _logger.LogInformation($"{nameof(GetSchemaSets)} returned {nameof(Ok)} with " +
        $"{string.Join(',', schemas.Select(x => x.Name))} schema sets to the caller");
      return Ok(schemas);
    }

    /// <summary>
    /// Get a specific set of schemas
    /// </summary>
    /// <returns>A specific schema set</returns>
    [HttpGet]
    [Route("Schemas/{setname}")]
    [ApiExplorerSettings(GroupName = "Schemas")]
    [SwaggerResponse(StatusCodes.Status200OK, "The requested schema set", typeof(SchemaSetDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
    public IActionResult GetSchemaSet(string setname)
    {
      _logger.LogInformation($"{nameof(GetSchemaSet)} with name {setname} was called");
      var schemaWasFound = _serverResourceControl.TryGetSchemaSet(setname, out SchemaSetDto schemaSet);

      if (schemaWasFound)
      {
        _logger.LogInformation($"{nameof(GetSchemaSet)} returned {nameof(Ok)} with name {setname} to the caller");
        return Ok(schemaSet);
      }

      _logger.LogInformation($"{nameof(GetSchemaSet)} returned {nameof(NotFound)} with name {setname} to the caller");
      return NotFound();
    }

    /// <summary>
    /// Get whether a specific schema exists
    /// </summary>
    /// <returns>True if a specific schema exists</returns>
    [HttpGet]
    [Route("Schemas/{setname}/{schemaname}")]
    [ApiExplorerSettings(GroupName = "Schemas")]
    [SwaggerResponse(StatusCodes.Status200OK, "The specified schema exists", typeof(bool))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Not found")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
    public IActionResult GetSchema(string setname, string schemaname)
    {
      _logger.LogInformation($"{nameof(GetSchema)} with name {setname} and schema {schemaname} was called");

      var schemaWasFound = _serverResourceControl.TryGetSchema(setname, schemaname);

      if (schemaWasFound)
      {
        _logger.LogInformation($"{nameof(GetSchema)} returned {nameof(Ok)} with name {setname} and schema {schemaname} to the caller");
        return Ok(true);
      }

      _logger.LogInformation($"{nameof(GetSchema)} returned {nameof(NotFound)} with name {setname} and schema {schemaname} to the caller");
      return NotFound();
    }

    /// <summary>
    /// Validates an XML file against a schema
    /// </summary>
    /// <returns>Result</returns>
    [HttpPost]
    [Route("Validate")]
    [ApiExplorerSettings(GroupName = "Validate")]
    [SwaggerResponse(StatusCodes.Status200OK, "XML is validated")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "XML failed validation")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Xml file, schema or schema set not found")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
    public IActionResult ValidateXml([FromBody] ValidateXmlDto dto)
    {
      var prereqErrors = new List<string>();

      _logger.LogInformation($"{nameof(ValidateXml)} with name {dto.SetName} and schema {dto.XmlFilePath} was called");

      // Check if the XML file exists on the system
      if (!System.IO.File.Exists(dto.XmlFilePath))
      {
        _logger.LogInformation($"{nameof(ValidateXml)} could not find the XML file {dto.XmlFilePath}");
        prereqErrors.Add($"XML file {dto.XmlFilePath} cannot be found");
      }

      // Check if the requested schema set exists
      var schemaSetWasFound = _serverResourceControl.TryGetSchemaSet(dto.SetName, out SchemaSetDto set);

      if (!schemaSetWasFound)
      {
        _logger.LogInformation($"{nameof(ValidateXml)} could not find a schema set with the name setName");
        prereqErrors.Add($"Set name {dto.SetName} cannot be found");
      }
      else
      {
        // Check that the requested schema file exists
        var schemaWasFound = _serverResourceControl.TryGetSchema(dto.SetName, dto.Schema);

        if (!schemaWasFound)
        {
          _logger.LogInformation($"{nameof(ValidateXml)} schema file {dto.Schema} cannot be found");
          prereqErrors.Add($"Schema file {dto.Schema} cannot be found");
        }
      }

      if (prereqErrors.Any())
      {
        return NotFound(prereqErrors);
      }

      // Validate the schema
      bool isValid = _serverResourceControl.TryValidate(dto.XmlFilePath, 
        Path.Combine(set.Path, dto.SetName, dto.Schema),
        out List<String> validationErrors);

      if (!isValid)
      {
        return BadRequest(validationErrors);
      }

      return Ok();
    }
  }
}
