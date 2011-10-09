@echo off
for /r tests/scaner %%f in (*.test) do (
	del %%f.out
	bin\Debug\Compiler.exe -c %%f >> %%f.out
)

@echo on
for /r tests/scaner %%f in (*.test) do (
	fc %%f.sol %%f.out
)
