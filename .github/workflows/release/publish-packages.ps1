$githubWorkspace = $env:WORKSPACE_PATH
$nugetApiKey = $env:NUGET_API_KEY

$RELEASES_PATH = "$githubWorkspace\.github\.release"

# Publish package
$PACKAGE_NAME = Get-ChildItem -Name -Path "$RELEASES_PATH\Api\*" -Include *.nupkg
$PACKAGE_PATH = "$RELEASES_PATH\Api\$PACKAGE_NAME"
# nuget.exe push $PACKAGE_PATH -Source "https://www.nuget.org" -ApiKey $nugetApiKey

# Publish PCL package.
$PCL_PACKAGE_NAME = Get-ChildItem -Name -Path "$RELEASES_PATH\PCL\*" -Include *.nupkg
$PCL_PACKAGE_PATH = "$RELEASES_PATH\PCL\$PCL_PACKAGE_NAME"
# nuget.exe push $PCL_PACKAGE_PATH -Source "https://www.nuget.org" -ApiKey $nugetApiKey

# Publish UWP package.
$UWP_PACKAGE_NAME = Get-ChildItem -Name -Path "$RELEASES_PATH\UWP\*" -Include *.nupkg
$UWP_PACKAGE_PATH = "$RELEASES_PATH\UWP\$UWP_PACKAGE_NAME"
# nuget.exe push $UWP_PACKAGE_PATH -Source "https://www.nuget.org" -ApiKey $nugetApiKey