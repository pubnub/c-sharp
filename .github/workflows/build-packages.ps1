$githubWorkspace = $args[0]
$secret = $args[1]

$ghWorkspace1 = $env:WORKSPACE_PATH
$ghWorkspace2 = Get-ChildItem -Path Env:\WORKSPACE_PATH

Write-Output "~~~~> ghWorkspace1: '$ghWorkspace1'"
Write-Output "~~~~> ghWorkspace2: '$ghWorkspace2'"

# Create required directory structure.
$RELEASES_PATH = "$githubWorkspace\.github\.release"
$ARTIFACTS_PATH = "$RELEASES_PATH\artifacts"
mkdir "$RELEASES_PATH"
mkdir "$RELEASES_PATH\Api"
mkdir "$RELEASES_PATH\PCL"
mkdir "$RELEASES_PATH\UWP"
mkdir "$ARTIFACTS_PATH"

Write-Output "~~~~> FIRST ARGUMENT: '$githubWorkspace'"
Write-Output "~~~~> SECOND ARGUMENT: '$secret'"


# Build Api package.
cd "$githubWorkspace\src\Api\PubnubApi"
dotnet pack -o "$RELEASES_PATH\Api" -c Release

# Build PCL package.
#cd "$githubWorkspace\src\Api\PubnubApiPCL"
#dotnet pack -o "$RELEASES_PATH\PCL" -c Release

# Build UWP package.
#cd "$githubWorkspace\src\Api\PubnubApiUWP"
#dotnet restore
#msbuild PubnubApiUWP.csproj /t:Pack /p:Configuration=Release /p:PackageOutputPath="$RELEASES_PATH\UWP" /v:n


# Copy built packages to artifacts folder.
$PACKAGE_NAME = Get-ChildItem -Name -Path "$RELEASES_PATH\Api\*" -Include *.nupkg
$PACKAGE_PATH = "$RELEASES_PATH\Api\$PACKAGE_NAME"
cp "$PACKAGE_PATH" "$ARTIFACTS_PATH\$PACKAGE_NAME"

#$PCL_PACKAGE_NAME = Get-ChildItem -Name -Path "$RELEASES_PATH\PCL\*" -Include *.nupkg
#$PCL_PACKAGE_PATH = "$RELEASES_PATH\PCL\$PCL_PACKAGE_NAME"
#cp "$PCL_PACKAGE_PATH" "$ARTIFACTS_PATH\$PCL_PACKAGE_NAME"

#$UWP_PACKAGE_NAME = Get-ChildItem -Name -Path "$RELEASES_PATH\UWP\*" -Include *.nupkg
#$UWP_PACKAGE_PATH = "$RELEASES_PATH\UWP\$UWP_PACKAGE_NAME"
#cp "$UWP_PACKAGE_PATH" "$ARTIFACTS_PATH\$UWP_PACKAGE_NAME"