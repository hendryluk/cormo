delete output\*.nupkg
NuGet.exe pack "..\alpaca\alpaca.csproj" -Build -OutputDirectory output
NuGet.exe pack "..\alpaca.web\alpaca.web.csproj" -Build -OutputDirectory output