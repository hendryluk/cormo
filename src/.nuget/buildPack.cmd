delete output\*.nupkg
NuGet.exe pack "..\alpaca\alpaca.csproj" -Build -Symbols -OutputDirectory output
NuGet.exe pack "..\alpaca.web\alpaca.web.csproj" -Build -Symbols -OutputDirectory output