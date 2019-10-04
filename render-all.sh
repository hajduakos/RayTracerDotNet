for f in Scenes/*.xml; do dotnet bin/Debug/netcoreapp3.0/RayTracer.dll $f "$f.png"; done
