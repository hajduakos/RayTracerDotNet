# RayTracer.Net

A simple ray tracer implemented in .net core 3.0.
See the scene files in the `Scenes` folder for examples.

## Building and running

The program can be built using the `dotnet` command.
When running the program the single parameter specifies the input scene.

**Windows**
```
dotnet build -c release
bin\Release\netcoreapp3.0\RayTracer.exe Scenes\misc.xml
```

**Linux**
```
sudo apt install libc6-dev
sudo apt install libgdiplus
```

```
dotnet build -c release
dotnet bin/Release/netcoreapp3.0/RayTracer.dll Scenes/misc.xml
```
