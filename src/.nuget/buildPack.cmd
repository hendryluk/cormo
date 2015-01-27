delete output\*.nupkg
NuGet.exe pack "..\cormo\cormo.csproj" -Build -Symbols -OutputDirectory output
NuGet.exe pack "..\cormo.web\cormo.web.csproj" -Build -Symbols -OutputDirectory output