using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using XmlValidationService;
using XmlValidationService.Controllers;
using XmlValidationService.Dtos;
using Xunit;

namespace XmlValidationServiceTests
{
  [ExcludeFromCodeCoverage]
	[Collection("Sequential")]
	public class XmlValidationServiceTests
	{
    private static readonly Fixture Fixture = new Fixture();

    [Theory]
    [MemberData(nameof(SchemaTestCases))]
    public void GetSchemas_WhenSchemasAreAvailable_ReturnsAllSchemaSets(IList<SchemaSetDescriptorDto> expectedSchemas)
    {
      IServerResourceControl mockedServerResourceControl = Substitute.For<IServerResourceControl>();
      mockedServerResourceControl
          .TryGetSchemaSets(out Arg.Any<IList<SchemaSetDescriptorDto>>())
              .Returns(x =>
              {
                x[0] = expectedSchemas;
                return true;
              });

      XmlValidationServiceController xmlValidationServiceController = GetXmlValidationServiceController(GetConfiguration(), GetLogger(), mockedServerResourceControl);

      IActionResult result = xmlValidationServiceController.GetSchemaSets();

      Assert.IsType<OkObjectResult>(result);
      var okResult = result as OkObjectResult;

      Assert.Equal(expectedSchemas, okResult?.Value as List<SchemaSetDescriptorDto>);
    }

    public static IEnumerable<object[]> SchemaTestCases =>
        new List<object[]>
        {
                new object[] { new List<SchemaSetDescriptorDto>() },
                new object[] { new List<SchemaSetDescriptorDto> { new SchemaSetDescriptorDto(Fixture.Create<string>()) }}
        };

    [Fact]
    public void GetSchemaSet_WhenSchemaSetExists_SchemaSetIsReturned()
    {
      var expectedSchemaSet = new SchemaSetDto(Fixture.Create<string>(), Fixture.Create<string>(), new List<string>());

      IServerResourceControl mockedServerResourceControl = Substitute.For<IServerResourceControl>();
      SetupMockedTryGetSchemaSet(mockedServerResourceControl, true, expectedSchemaSet.Name);

      XmlValidationServiceController xmlValidationServiceController = GetXmlValidationServiceController(GetConfiguration(), GetLogger(), mockedServerResourceControl);

      IActionResult result = xmlValidationServiceController.GetSchemaSet(expectedSchemaSet.Name);

      Assert.IsType<OkObjectResult>(result);
      var okResult = result as OkObjectResult;

      Assert.Equal(expectedSchemaSet.Name, (okResult.Value as SchemaSetDto).Name);
      Assert.Equal(expectedSchemaSet.Schemas, (okResult.Value as SchemaSetDto).Schemas);
    }

    [Fact]
    public void GetSchemaSet_WhenSchemaSetDoesNotExist_NoSchemaSetIsReturned()
    {
      var setname = Fixture.Create<string>();

      IServerResourceControl mockedServerResourceControl = Substitute.For<IServerResourceControl>();
      SetupMockedTryGetSchemaSet(mockedServerResourceControl, false, setname);
      XmlValidationServiceController xmlValidationServiceController = GetXmlValidationServiceController(GetConfiguration(), GetLogger(), mockedServerResourceControl);

      IActionResult result = xmlValidationServiceController.GetSchemaSet(Fixture.Create<string>());

      Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetSchema_WhenSchemaExists_SchemaExistsReturned()
    {
      var xsd = "test.xsd";
      var expectedSchemaSet = new SchemaSetDto(Fixture.Create<string>(), Fixture.Create<string>(), new List<string> { xsd });

      IServerResourceControl mockedServerResourceControl = Substitute.For<IServerResourceControl>();
      SetupMockedTryGetSchema(mockedServerResourceControl, true, expectedSchemaSet.Name, xsd);

      XmlValidationServiceController xmlValidationServiceController = GetXmlValidationServiceController(GetConfiguration(), GetLogger(), mockedServerResourceControl);

      IActionResult result = xmlValidationServiceController.GetSchema(expectedSchemaSet.Name, expectedSchemaSet.Schemas.First());

      Assert.IsType<OkObjectResult>(result);
      var okResult = result as OkObjectResult;

      Assert.True((bool)okResult.Value);
    }

    [Fact]
    public void GetSchema_WhenSchemaDoesNotExist_NotFoundReturned()
    {
      var xsd = "test.xsd";
      var expectedSchemaSet = new SchemaSetDto(Fixture.Create<string>(), Fixture.Create<string>(), new List<string> { xsd });

      IServerResourceControl mockedServerResourceControl = Substitute.For<IServerResourceControl>();
      SetupMockedTryGetSchema(mockedServerResourceControl, false, expectedSchemaSet.Name, xsd);

      XmlValidationServiceController xmlValidationServiceController = GetXmlValidationServiceController(GetConfiguration(), GetLogger(), mockedServerResourceControl);

      IActionResult result = xmlValidationServiceController.GetSchema(expectedSchemaSet.Name, expectedSchemaSet.Schemas.First());

      Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void ValidateXml_WhenXmlIsValid_OkReturned()
    {
      TestUtilities.CopyDataFolder();

      ValidateXmlDto dto = new ValidateXmlDto(TestUtilities.TestSchemaSet, TestUtilities.ValidSchemaName, TestUtilities.ValidXMlFilePath);
      IServerResourceControl mockedServerResourceControl = Substitute.For<IServerResourceControl>();
      SetupMockedTryValidate(mockedServerResourceControl, true, dto.XmlFilePath, dto.Schema, dto.SetName);

      XmlValidationServiceController xmlValidationServiceController = GetXmlValidationServiceController(GetConfiguration(), GetLogger(), mockedServerResourceControl);

      IActionResult result = xmlValidationServiceController.ValidateXml(dto);

      OkObjectResult ok = result as OkObjectResult;

      Assert.IsType<OkResult>(result);

      TestUtilities.RemoveDataFolder();
    }

    [Fact]
    public void ValidateXml_WhenXmlIsInvalid_BadRequestReturned()
    {
      TestUtilities.CopyDataFolder();

      ValidateXmlDto dto = new ValidateXmlDto(TestUtilities.TestSchemaSet, TestUtilities.ValidSchemaName, TestUtilities.ValidXMlFilePath);
      IServerResourceControl mockedServerResourceControl = Substitute.For<IServerResourceControl>();
      SetupMockedTryValidate(mockedServerResourceControl, false, dto.XmlFilePath, dto.Schema, dto.SetName);

      XmlValidationServiceController xmlValidationServiceController = GetXmlValidationServiceController(GetConfiguration(), GetLogger(), mockedServerResourceControl);

      IActionResult result = xmlValidationServiceController.ValidateXml(dto);

      Assert.IsType<BadRequestObjectResult>(result);

      TestUtilities.RemoveDataFolder();
    }

    [Fact]
    public void ValidateXml_WhenSetDoesNotExist_NotFoundReturned()
    {
      ValidateXmlDto dto = new ValidateXmlDto(Fixture.Create<string>(), Fixture.Create<string>(), Fixture.Create<string>());
      IServerResourceControl mockedServerResourceControl = Substitute.For<IServerResourceControl>();
      SetupMockedTryGetSchemaSet(mockedServerResourceControl, false, dto.SetName);

      XmlValidationServiceController xmlValidationServiceController = GetXmlValidationServiceController(GetConfiguration(), GetLogger(), mockedServerResourceControl);

      IActionResult result = xmlValidationServiceController.ValidateXml(dto);

      NotFoundObjectResult notFound = result as NotFoundObjectResult;
      List<String> errors = notFound.Value as List<string>;

      Assert.IsType<NotFoundObjectResult>(result);
      Assert.NotNull(errors);
      Assert.Contains($"Set name {dto.SetName} cannot be found", errors);
    }

    [Fact]
    public void ValidateXml_WhenSchemaDoesNotExist_NotFoundReturned()
    {
      ValidateXmlDto dto = new ValidateXmlDto(Fixture.Create<string>(), Fixture.Create<string>(), Fixture.Create<string>());
      IServerResourceControl mockedServerResourceControl = Substitute.For<IServerResourceControl>();
      SetupMockedTryGetSchemaSet(mockedServerResourceControl, true, dto.SetName);

      XmlValidationServiceController xmlValidationServiceController = GetXmlValidationServiceController(GetConfiguration(), GetLogger(), mockedServerResourceControl);

      IActionResult result = xmlValidationServiceController.ValidateXml(dto);

      NotFoundObjectResult notFound = result as NotFoundObjectResult;
      List<String> errors = notFound.Value as List<string>;

      Assert.IsType<NotFoundObjectResult>(result);
      Assert.NotNull(errors);
      Assert.Contains($"Schema file {dto.Schema} cannot be found", errors);
    }

    [Fact]
    public void ValidateXml_WhenXmlFileDoesNotExist_NotFoundReturned()
    {
      ValidateXmlDto dto = new ValidateXmlDto(Fixture.Create<string>(), Fixture.Create<string>(), Fixture.Create<string>());
      IServerResourceControl mockedServerResourceControl = Substitute.For<IServerResourceControl>();

      XmlValidationServiceController xmlValidationServiceController = GetXmlValidationServiceController(GetConfiguration(), GetLogger(), mockedServerResourceControl);

      IActionResult result = xmlValidationServiceController.ValidateXml(dto);

      NotFoundObjectResult notFound = result as NotFoundObjectResult;
      List<String> errors = notFound.Value as List<string>;

      Assert.IsType<NotFoundObjectResult>(result);
      Assert.NotNull(errors);
      Assert.Contains($"XML file {dto.XmlFilePath} cannot be found", errors);
    }

    #region Private methods

    private void SetupMockedTryValidate(IServerResourceControl mockedServerResourceControl, bool isValid, string xmlFile, string schemaFile, string setName)
    {
      // Mock that the schema and set are valid. They need to be valid so that the code reaches the call to validate.
      mockedServerResourceControl
          .TryGetSchemaSet(setName, out Arg.Any<SchemaSetDto>())
          .Returns(x =>
          {
            x[1] = new SchemaSetDto(setName, TestUtilities.SchemasFolder, new List<string>());
            return true;
          });

      SetupMockedTryGetSchema(mockedServerResourceControl, true, setName, schemaFile);

      // Mock the TryValidate call
      if (isValid)
      {
        mockedServerResourceControl
         .TryValidate(xmlFile, TestUtilities.ValidSchemaFilePath, out List<string> errors)
         .Returns(x =>
         {
           x[2] = new List<string>();
           return true;
         });
      }
      else
      {
        mockedServerResourceControl
            .TryValidate(xmlFile, TestUtilities.ValidSchemaFilePath, out List<string> errors)
            .Returns(x =>
            {
              x[2] = new List<String> { "error test" };
              return false;
            });
      }
    }

    private void SetupMockedTryGetSchema(IServerResourceControl mockedServerResourceControl, bool schemaSetExists, string setname, string xsd)
    {
      if (schemaSetExists)
      {
        mockedServerResourceControl
            .TryGetSchema(setname, xsd)
            .Returns(x =>
            {
              return true;
            });

        return;

      }

      mockedServerResourceControl
          .TryGetSchema(setname, xsd)
          .Returns(x =>
          {
            return false;
          });
    }

    private void SetupMockedTryGetSchemaSet(IServerResourceControl mockedServerResourceControl, bool schemaSetExists, string setname = null)
    {
      if (schemaSetExists)
      {
        mockedServerResourceControl
            .TryGetSchemaSet(setname, out Arg.Any<SchemaSetDto>())
            .Returns(x =>
            {
              x[1] = new SchemaSetDto(setname, Fixture.Create<string>(), new List<string>());
              return true;
            });

        return;

      }

      mockedServerResourceControl
          .TryGetSchemaSet(Arg.Any<string>(), out Arg.Any<SchemaSetDto>())
          .Returns(x =>
          {
            x[1] = null;
            return false;
          });
    }

    private XmlValidationServiceController GetXmlValidationServiceController(IConfiguration configuration, ILogger<XmlValidationServiceController> logger, IServerResourceControl serverResponseControl)
    {
      return new XmlValidationServiceController(configuration, logger, serverResponseControl)
      {
        ControllerContext = new ControllerContext()
        {
          HttpContext = new DefaultHttpContext()
        }
      };
    }

    private ILogger<XmlValidationServiceController> GetLogger()
    {
      return Substitute.For<ILogger<XmlValidationServiceController>>();
    }

    private IConfiguration GetConfiguration()
    {
      return new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json")
          .Build();
    }

    #endregion
  }
}
