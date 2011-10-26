@echo off
for /r tests/parser %%f in (*.test) do (
	del %%f.out
	
	bin\Debug\Compiler.exe -c %%f >> %%f.out
)

for /r tests/parser %%f in (*.test) do (
	fc %%f.sol %%f.out > nul
	if errorlevel 1 (
		echo fail %%f
	) else (
		echo passed
	)
)
