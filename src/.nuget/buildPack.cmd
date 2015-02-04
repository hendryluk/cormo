delete output\*.nupkg
NuGet.exe pack "..\cormo\cormo.csproj" -Build -Symbols -OutputDirectory output
NuGet.exe pack "..\cormo.web\cormo.web.csproj" -Build -Symbols -OutputDirectory output
NuGet.exe pack "..\cormo.data.entityframework\cormo.data.entityframework.csproj" -Build -Symbols -OutputDirectory output
NuGet.exe pack "..\cormo.commonservicelocator\cormo.commonservicelocator.csproj" -Build -Symbols -OutputDirectory output
pause