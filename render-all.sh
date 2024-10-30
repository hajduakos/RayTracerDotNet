rm Scenes/*.bmp
for f in Scenes/*.xml
do
    echo "Rendering $f with $1 s timeout"
    time timeout $1 dotnet RayTracer/bin/Release/net8.0/RayTracer.dll $f
    echo "-----"
done
