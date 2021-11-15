using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace XmlValidationServiceTests
{
  [ExcludeFromCodeCoverage]
  public class TestUtilities
	{
		public static string DestinationTestFolder => Path.Combine(SchemasFolder, TestSchemaSet);

		public static string TestSchemaSet => "UpcA";

		public static string SchemasFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
			@"XmlValidationService\schemas");
		public static string TestFolder => Path.Combine(GetDirectoryPath(Assembly.GetExecutingAssembly()), @"testdata\UpcA");

    public static string ValidXMlFilePath => Path.Combine(DestinationTestFolder, "ibr_filter_valid.xml");

    public static string InvalidXMlFilePath => Path.Combine(DestinationTestFolder, "ibr_filter_invalid.xml");

    public static string InvalidXMIbrReceiverlFilePath => Path.Combine(DestinationTestFolder, "ibr_receiver_invalid.xml");

    public static string ValidSchemaName => "IBR_Filter.xsd";

    public static string ValidSchemaFilePath => Path.Combine(DestinationTestFolder, ValidSchemaName);

    public static string ValidSchemaIbrReceiverFilePath => Path.Combine(DestinationTestFolder, "IBR_Receiver.xsd");


    public static void RemoveDataFolder()
    {
      Directory.Delete(DestinationTestFolder, true);
    }

    public static void CopyDataFolder()
    {
      string testFolder = Path.Combine(GetDirectoryPath(Assembly.GetExecutingAssembly()), @"testdata\UpcA");

      // Get the subdirectories for the specified directory.
      DirectoryInfo dir = new DirectoryInfo(testFolder);

      if (!dir.Exists)
      {
        throw new DirectoryNotFoundException(
            "Source directory does not exist or could not be found: "
            + testFolder);
      }

      DirectoryInfo[] dirs = dir.GetDirectories();

      // If the destination directory doesn't exist, create it.       
      Directory.CreateDirectory(DestinationTestFolder);

      // Get the files in the directory and copy them to the new location.
      FileInfo[] files = dir.GetFiles();
      foreach (FileInfo file in files)
      {
        string tempPath = Path.Combine(DestinationTestFolder, file.Name);
        file.CopyTo(tempPath, true);
      }
    }

    private static string GetDirectoryPath(Assembly assembly)
		{
			string filePath = new Uri(assembly.CodeBase).LocalPath;
			return Path.GetDirectoryName(filePath);
		}
	}
}
