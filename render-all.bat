FOR %%F IN (Scenes\*.xml) DO (
    bin\Debug\netcoreapp3.0\RayTracer.exe %%F %%F.png
)