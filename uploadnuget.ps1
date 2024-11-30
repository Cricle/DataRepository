$version=$args[0]
$accessToken=$args[1]
dotnet build -c Release DataRepository.sln
$paths=ls src | select Name
for($x=0;$x -lt $paths.length; $x=$x+1)
{
$fp=-join ("src\",$paths[$x].Name,"\bin\Release\",$paths[$x].Name,".",$version,".nupkg");
dotnet nuget push $fp -k $accessToken -s https://api.nuget.org/v3/index.json
}
