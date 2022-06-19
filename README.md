# RayTracer.Net

A simple ray tracer implemented in .net core 6.0.
See the scene files in the `Scenes` folder for examples.

## Building and running

The program can be built using the `dotnet` command.
When running the program the single parameter specifies the input scene.

**Windows**
```
dotnet build -c release
bin\Release\net6.0\RayTracer.exe Scenes\misc.xml
```

**Linux / Mac**
- Dependencies on Linux: `sudo apt install libc6-dev libgdiplus`
- Dependencies on Mac: `brew install mono-libgdiplus`
  - `sudo ln -s /opt/homebrew/opt/mono-libgdiplus/lib/libgdiplus.dylib /usr/local/lib/` might also be needed

```
dotnet build -c release
dotnet bin/Release/net6.0/RayTracer.dll Scenes/misc.xml
```
