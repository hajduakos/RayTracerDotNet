for f in Scenes/*.xml
do
    timeout $1 dotnet bin/Release/netcoreapp3.0/RayTracer.dll $f "$f.png"
done
