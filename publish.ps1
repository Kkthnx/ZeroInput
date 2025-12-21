# 1. Clean and Publish the App (Release Mode)
dotnet publish .\ZeroInput\ZeroInput.csproj -c Release -r win-x64 --self-contained -o .\publish

# 2. Pack with Velopack
# --packTitle: Title in Add/Remove Programs
# --packId: The ID used for updates (must match .csproj)
# --packVersion: The version (must match .csproj)
# --icon: Your app icon
vpk pack -u "ZeroInput" -v "1.0.0" -p .\publish -e "ZeroInput.exe" --icon ".\ZeroInput\ZeroInput.ico"