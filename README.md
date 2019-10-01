# RayTracer.Net

Simple ray tracer implemented in .net core 3.0.

# Building and running

**Windows**
```
dotnet build RayTracer.sln
bin\Debug\netcoreapp3.0\RayTracer.exe Scenes\test.xml output.png
```

**Linux**
```
sudo apt install libc6-dev
sudo apt install libgdiplus
```

```
dotnet build RayTracer.sln
dotnet bin/Debug/netcoreapp3.0/RayTracer.dll Scenes/test.xml output.png
```
