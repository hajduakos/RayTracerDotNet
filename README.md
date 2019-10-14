# RayTracer.Net

A simple ray tracer implemented in .net core 3.0.
See the scene files in the `Scenes` folder for examples.

# Building and running

The program can be built using the `dotnet` command.
When running the program the first parameter specifies the input scene and the second parameter specifies the output image path.

**Windows**
```
dotnet build RayTracer.sln -c release
bin\Release\netcoreapp3.0\RayTracer.exe Scenes\misc.xml output.png
```

**Linux**
```
sudo apt install libc6-dev
sudo apt install libgdiplus
```

```
dotnet build RayTracer.sln -c release
dotnet bin/Release/netcoreapp3.0/RayTracer.dll Scenes/misc.xml output.png
```
