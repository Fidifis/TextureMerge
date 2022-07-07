rmdir /s /Q TextureMerge\bin\Release
dotnet.exe publish --runtime win-x64 --self-contained true -c Release -o TextureMerge/bin/Release/setup_win-x64
dotnet.exe publish --runtime win-x64 --self-contained true -c Release -o TextureMerge/bin/Release/portable_win-x64
dotnet.exe publish --runtime win-x64 --self-contained false -c Release -o TextureMerge/bin/Release/portable_no-runtime_win-x64
