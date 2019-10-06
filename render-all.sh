for f in Scenes/*.xml
do
    echo "Rendering $f with $1 s timeout"
    time timeout $1 dotnet bin/Release/netcoreapp3.0/RayTracer.dll $f "$f.png"
    echo "-----"
done
