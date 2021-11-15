@echo off
setlocal

IF NOT DEFINED XmlValidationRelativeRootFolder set "XmlValidationRelativeRootFolder=..\..\Build"
IF NOT DEFINED XmlBuildType set "XmlBuildType=Release"

echo BuildType is %XmlBuildType%
echo Removing build folder at %XmlValidationRelativeRootFolder% if it exists
if exist %XmlValidationRelativeRootFolder% rmdir %XmlValidationRelativeRootFolder% /Q /S

echo *********************************************************************
echo * Publishing a self contained .net core 3.1 x64 artifact            *
echo *********************************************************************

set SelfContainedXmlValidationRelativeRootFolder=%XmlValidationRelativeRootFolder%\SelfContained

echo Publishing the self contained windows service to %SelfContainedXmlValidationRelativeRootFolder%
dotnet publish -c %XmlBuildType% -r win-x64 --self-contained --output %SelfContainedXmlValidationRelativeRootFolder%

echo Creating the Dependencies folder at %SelfContainedXmlValidationRelativeRootFolder%\Dependencies if it does not already exist
if not exist %SelfContainedXmlValidationRelativeRootFolder%\Dependencies mkdir %SelfContainedXmlValidationRelativeRootFolder%\Dependencies

echo Creating the Resources folder at %SelfContainedXmlValidationRelativeRootFolder%\Resources if it does not already exist
if not exist %SelfContainedXmlValidationRelativeRootFolder%\Resources mkdir %SelfContainedXmlValidationRelativeRootFolder%\Resources

set PlatformDependentXmlValidationRelativeRootFolder=%XmlValidationRelativeRootFolder%\PlatformDependent

echo *********************************************************************
echo * Publishing a platform dependent .net core 3.1 artifact            *
echo *********************************************************************

echo Publishing the platform dependent windows service to %PlatformDependentXmlValidationRelativeRootFolder%
dotnet publish -c %XmlBuildType% --output %PlatformDependentXmlValidationRelativeRootFolder%

echo Creating the Dependencies folder at %PlatformDependentXmlValidationRelativeRootFolder%\Dependencies if it does not already exist
if not exist %PlatformDependentXmlValidationRelativeRootFolder%\Dependencies mkdir %PlatformDependentXmlValidationRelativeRootFolder%\Dependencies

echo Creating the Resources folder at %PlatformDependentXmlValidationRelativeRootFolder%\Resources if it does not already exist
if not exist %PlatformDependentXmlValidationRelativeRootFolder%\Resources mkdir %PlatformDependentXmlValidationRelativeRootFolder%\Resources

endlocal