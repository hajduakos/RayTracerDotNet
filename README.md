# RayTracer.Net

[![.NET Core](https://github.com/hajduakos/RayTracerDotNet/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/hajduakos/RayTracerDotNet/actions/workflows/dotnetcore.yml)

A simple, cross platform ray tracer implemented in .NET 10.0.
See the scene files in the `Scenes` folder for examples.

## Building and running

The program can be built using the `dotnet` command.
When running the program the single parameter specifies the input scene.

**Windows**
```
dotnet build -c release
RayTracer\bin\Release\net10.0\RayTracer.exe Scenes\misc.xml
```

**Linux / Mac**
```
dotnet build -c release
dotnet RayTracer/bin/Release/net10.0/RayTracer.dll Scenes/misc.xml
```

## Samples

![Sample](../gh-pages/misc.xml000.bmp)

![Sample](../gh-pages/material-mirror-color.xml000.bmp)

![Sample](../gh-pages/misc-box.xml000.bmp)

![Sample](../gh-pages/material-mirror-blur.xml000.bmp)

![Sample](../gh-pages/scene-dof.xml000.bmp)

![Sample](../gh-pages/material-glass-air-sphere.xml000.bmp)
