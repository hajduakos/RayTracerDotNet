rm Scenes/*.png
for f in Scenes/*.xml
do
    echo "Rendering $f with $1 s timeout"
    time timeout $1 dotnet RayTracer/bin/Release/net6.0/RayTracer.dll $f
    echo "-----"
done
