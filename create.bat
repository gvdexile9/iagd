copy inno\vcredist_x64.exe IAGrim\bin\Release\
copy inno\vcredist_x86.exe IAGrim\bin\Release\
copy inno\dotNetFx45_Full_setup.exe IAGrim\bin\Release\
copy inno\2010sp1_vcredist_x86.exe IAGrim\bin\Release\
copy inno\postgresql-9.5.2-1-windows.exe IAGrim\bin\Release\

Inno\iscc Inno\gdia.iss
copy IAGrim\bin\Release\IAGrim.exe Installer\IAGrim.exe