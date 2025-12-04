del Scenes\*.bmp
FOR %%F IN (Scenes\*.xml) DO (
    ScriptRunner.exe -appvscript RayTracer\bin\Release\net10.0\RayTracer.exe %%F -appvscriptrunnerparameters -wait -timeout=%1
)
