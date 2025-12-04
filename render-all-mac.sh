rm Scenes/*.bmp
for f in Scenes/*.xml
do
    echo "Rendering $f"
    time dotnet RayTracer/bin/Release/net10.0/RayTracer.dll $f
    echo "-----"
done
