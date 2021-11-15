using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using XmlValidationService;
using XmlValidationService.Dtos;
using Xunit;

namespace XmlValidationServiceTests
{
  [ExcludeFromCodeCoverage]
	[Collection("Sequential")]
	public class ServerResourceControlTests
	{
		private readonly ILogger<ServerResourceControl> _mockedLogger = Substitute.For<ILogger<ServerResourceControl>>();

    [Fact]
		public void GetSchemaSets_WhenSchemasAreAvailable_ReturnsAllSchemaSets()
		{
      TestUtilities.CopyDataFolder();

      IServerResourceControl res = new ServerResourceControl(_mockedLogger);

      Assert.True(res.TryGetSchemaSets(out IList<SchemaSetDescriptorDto> schemaSet));
      Assert.True(schemaSet.Where(x => x.Name == TestUtilities.TestSchemaSet).FirstOrDefault() != null);

      TestUtilities.RemoveDataFolder();
    }

    [Fact]
    public void GetSchemaSet_WhenSchemaIsAvailable_ReturnsRequestedSchemaSets()
    {
      TestUtilities.CopyDataFolder();

      IServerResourceControl res = new ServerResourceControl(_mockedLogger);

      Assert.True(res.TryGetSchemaSet(TestUtilities.TestSchemaSet, out SchemaSetDto schemaSet));
      Assert.True(schemaSet.Name == TestUtilities.TestSchemaSet);
      Assert.True(string.Compare(schemaSet.Path, TestUtilities.SchemasFolder, StringComparison.OrdinalIgnoreCase) == 0);

      TestUtilities.RemoveDataFolder();
    }

    [Fact]
    public void TryGetSchema_WhenSchemaIsAvailable_ReturnsTrue()
    {
      TestUtilities.CopyDataFolder();

      IServerResourceControl res = new ServerResourceControl(_mockedLogger);

      Assert.True(res.TryGetSchema(TestUtilities.TestSchemaSet, "test.xsd"));

      TestUtilities.RemoveDataFolder();
    }

    [Fact]
    public void TryGetSchema_WhenSchemaDoesntExist_ReturnsFalse()
    {
      IServerResourceControl res = new ServerResourceControl(_mockedLogger);

      Assert.False(res.TryGetSchema(TestUtilities.TestSchemaSet, "test1.xsd"));
    }

    [Fact]
    public void TryValidate_WhenXmlIsValid_ReturnsTrue()
    {
      TestUtilities.CopyDataFolder();

      IServerResourceControl res = new ServerResourceControl(_mockedLogger);

      Assert.True(res.TryValidate(TestUtilities.ValidXMlFilePath, TestUtilities.ValidSchemaFilePath, out List<String> errors));
      Assert.Null(errors);

      TestUtilities.RemoveDataFolder();
    }

    [Fact]
    public void TryValidate_WhenXmlIsInvalid_ReturnsFalse()
    {
      TestUtilities.CopyDataFolder();

      IServerResourceControl res = new ServerResourceControl(_mockedLogger);

      Assert.False(res.TryValidate(TestUtilities.InvalidXMlFilePath, TestUtilities.ValidSchemaFilePath, out List<String> errors));
      Assert.NotNull(errors);

      TestUtilities.RemoveDataFolder();
    }

    [Fact]
    public void TryValidate_WhenXmlFailsValidationRange_ReturnsFalse()
    {
      TestUtilities.CopyDataFolder();

      IServerResourceControl res = new ServerResourceControl(_mockedLogger);

      Assert.False(res.TryValidate(TestUtilities.InvalidXMIbrReceiverlFilePath, TestUtilities.ValidSchemaIbrReceiverFilePath, out List<String> errors));
      Assert.NotNull(errors);

      TestUtilities.RemoveDataFolder();
    }
  }
}
