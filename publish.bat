rmdir /s /Q TextureMerge\bin\Release\net6.0-windows\win-x64\publish
dotnet.exe publish --runtime win-x64 --self-contained false -c Release
