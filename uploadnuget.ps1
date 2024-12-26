$accessToken=$args[0]
$version=Get-Content version
dotnet build -c Release DataRepository.sln -p:Version=$version
$paths=ls -Directory src | select Name
for($x=0;$x -lt $paths.length; $x=$x+1)
{
$fp=-join ("src\",$paths[$x].Name,"\bin\Release\","D",$paths[$x].Name,".",$version,".nupkg");
dotnet nuget push $fp -k $accessToken -s https://api.nuget.org/v3/index.json --skip-duplicate
}
