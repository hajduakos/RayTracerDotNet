del Scenes\*.png
FOR %%F IN (Scenes\*.xml) DO (
    bin\Release\net6.0\RayTracer.exe %%F
)
