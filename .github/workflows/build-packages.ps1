$githubWorkspace = $args[0]


$RELEASES_PATH = "$githubWorkspace\.github\.release"
mkdir "$RELEASES_PATH"

# Build Api package.
cd "$githubWorkspace\src\Api\PubnubApi"
#dotnet pack -o "$RELEASES_PATH\Api" -c Release
dotnet pack -o "$RELEASES_PATH" -c Release

# Build PCL package.
cd "$githubWorkspace\src\Api\PubnubApiPCL"
#dotnet pack -o "$RELEASES_PATH\PCL" -c Release
dotnet pack -o "$RELEASES_PATH" -c Release

# Build UWP package.
cd "$githubWorkspace\src\Api\PubnubApiUWP"
dotnet restore
#msbuild PubnubApiUWP.csproj /t:Pack /p:Configuration=Release /p:PackageOutputPath="$RELEASES_PATH\UWP" /v:n
msbuild PubnubApiUWP.csproj /t:Pack /p:Configuration=Release /p:PackageOutputPath="$RELEASES_PATH" /v:n
