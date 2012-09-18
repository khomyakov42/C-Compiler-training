@echo off
for /r tests/parser/declaration %%f in (*.test) do (
	del %%f.out
   echo compilation %%f
	bin\Debug\Compiler.exe -c %%f >> %%f.out
)

for /r tests/parser/declaration %%f in (*.test) do (
	fc %%f.sol %%f.out > nul
	if errorlevel 1 (
		echo fail %%f
	) else (
		echo passed %%f
	)
)
