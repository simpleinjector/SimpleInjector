mkdir Help
mkdir Help.Asm

copy SimpleInjector.Extensions\bin\Release\*.* Help.Asm\*.*

SandcastleGUI.exe /document SimpleInjector.SandcastleGUI

ren Help\Documentation.chm SimpleInjector.chm

ren Help\Presentation.css presentation.xxx
ren Help\presentation.xxx presentation.css

del Help.Asm\*.* /Q
rmdir Help.Asm