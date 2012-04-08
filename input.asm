.386
.model flat, stdcall
   include C:\masm32\include\msvcrt.inc
   includelib C:\masm32\lib\msvcrt.lib
.data
   str1 BYTE 3 dup("%s", 0)
   str2 BYTE 7 dup("Hello!", 0)
   f DWORD ?
   pf DWORD 10 dup(?)
   ppf DWORD ?
.code
   extern printf:near
   extern scanf:near
   extern getchar:near
   func PROC
      push offset str2
      push offset str1
      lea eax, printf
      push eax
      pop eax
      call eax
      pop edi
      pop edi
      push edi
      pop edi
      ;";"
      RET
   func ENDP
   main PROC
      push func
      lea eax, f
      push eax
      ;'='
      pop eax
      pop ebx
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      push f
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      push f
      push 4
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, pf
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;'='
      pop eax
      pop ebx
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      push 4
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, pf
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      call eax
      pop eax
      push eax
      pop eax
      mov eax, [eax]
      push eax
      pop edi
      ;";"
      RET
   main ENDP


start:
   call main
   RET
end start
