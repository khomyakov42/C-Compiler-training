@echo off
for /r tests/parser/declaration %%f in (*.test) do (
	del %%f.out
	bin\Debug\Compiler.exe -c %%f >> %%f.out
   echo compilation %%f
)

for /r tests/parser/declaration %%f in (*.test) do (
	fc %%f.sol %%f.out > nul
	if errorlevel 1 (
		echo fail %%f
	) else (
		echo passed %%f
	)
)
