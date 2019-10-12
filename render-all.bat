del Scenes\*.png
FOR %%F IN (Scenes\*.xml) DO (
    bin\Release\netcoreapp3.0\RayTracer.exe %%F %%F.png
)
