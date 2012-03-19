.386
.model flat, stdcall
   include C:\masm32\include\msvcrt.inc
   includelib C:\masm32\lib\msvcrt.lib
.data
   str1 BYTE 8 dup("%d %d", 13, 10, 0)
   str2 BYTE 8 dup("%d %d", 13, 10, 0)
   str3 BYTE 8 dup("%d %d", 13, 10, 0)
   str4 BYTE 8 dup("%d %d", 13, 10, 0)
   str5 BYTE 8 dup("%d %d", 13, 10, 0)
   str6 BYTE 8 dup("%d %d", 13, 10, 0)
   str7 BYTE 8 dup("%d %d", 13, 10, 0)
   arr DWORD 10 dup(?)
   arr2 DWORD 100 dup(?)
   x DWORD ?
.code
   extern printf:near
   extern scanf:near
   extern getchar:near
   main PROC
      lea eax, arr2
      push eax
      lea eax, x
      push eax
      ;'='
      pop eax
      pop ebx
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      push 40
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, arr2
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      push 4
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push x
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      push offset str1
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      push 4
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, arr2
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      push 4
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push x
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      push offset str2
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      lea eax, arr
      push eax
      lea eax, x
      push eax
      ;'='
      pop eax
      pop ebx
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      push 0
      lea eax, arr
      push eax
      ;'='
      pop eax
      pop ebx
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      push 1
      push x
      ;'='
      pop eax
      pop ebx
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      push x
      pop eax
      mov eax, [eax]
      push eax
      lea eax, arr
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push offset str3
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      push 4
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push x
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop edi
      push 4
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push x
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      lea eax, x
      push eax
      ;'='
      pop eax
      pop ebx
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      push 2
      push x
      ;'='
      pop eax
      pop ebx
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      push x
      pop eax
      mov eax, [eax]
      push eax
      push 4
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, arr
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push offset str4
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      push 4
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push x
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      lea eax, x
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
      push x
      ;"-"
      pop eax
      pop ebx
      sub eax, ebx
      push eax
      pop edi
      push 4
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push x
      ;"-"
      pop eax
      pop ebx
      sub eax, ebx
      push eax
      lea eax, x
      push eax
      ;'='
      pop eax
      pop ebx
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      push 3
      push x
      ;'='
      pop eax
      pop ebx
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      push x
      pop eax
      mov eax, [eax]
      push eax
      push 4
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, arr
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push offset str5
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      push 4
      push 4
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push x
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
      push x
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push 4
      push 3
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, arr
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push offset str6
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      push 5
      push 4
      push 4
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, arr
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
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push x
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push 4
      push 4
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, arr
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push offset str7
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      RET
   main ENDP


start:
   call main
   RET
end start
