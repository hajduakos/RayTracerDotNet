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
```
dotnet build -c release
dotnet bin/Release/net6.0/RayTracer.dll Scenes/misc.xml
```
