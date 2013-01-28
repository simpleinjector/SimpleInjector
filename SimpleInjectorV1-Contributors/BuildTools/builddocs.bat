mkdir Help
mkdir Help.Asm

copy bin\NET\SimpleInjector.dll Help.Asm\SimpleInjector.dll
copy bin\NET\SimpleInjector.xml Help.Asm\SimpleInjector.xml
copy bin\NET\SimpleInjector.Extensions.dll Help.Asm\SimpleInjector.Extensions.dll
copy bin\NET\SimpleInjector.Extensions.xml Help.Asm\SimpleInjector.Extensions.xml

SandcastleGUI.exe /document SimpleInjector.SandcastleGUI

ren Help\Documentation.chm SimpleInjector.chm

ren Help\Presentation.css presentation.xxx
ren Help\presentation.xxx presentation.css

del Help.Asm\*.* /Q
rmdir Help.Asm