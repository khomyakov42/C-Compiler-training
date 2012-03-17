.386
.model flat, stdcall
   include C:\masm32\include\msvcrt.inc
   includelib C:\masm32\lib\msvcrt.lib
.data
   str1 BYTE 5 dup("%d", 13, 10, 0)
   str2 BYTE 5 dup("%d", 13, 10, 0)
   str3 BYTE 5 dup("%d", 13, 10, 0)
   str4 BYTE 5 dup("%d", 13, 10, 0)
   str5 BYTE 5 dup("%d", 13, 10, 0)
   str6 BYTE 5 dup("%d", 13, 10, 0)
   str7 BYTE 5 dup("%d", 13, 10, 0)
   str8 BYTE 5 dup("%d", 13, 10, 0)
   str9 BYTE 5 dup("%d", 13, 10, 0)
   str10 BYTE 5 dup("%d", 13, 10, 0)
   str11 BYTE 5 dup("%d", 13, 10, 0)
   str12 BYTE 5 dup("%d", 13, 10, 0)
   str13 BYTE 5 dup("%d", 13, 10, 0)
   str14 BYTE 5 dup("%d", 13, 10, 0)
   str15 BYTE 5 dup("%d", 13, 10, 0)
   x DWORD 6 dup(?)
   x2 DWORD 12 dup(?)
   x3 DWORD 24 dup(?)
.code
   extern printf:near
   extern scanf:near
   extern getchar:near
   main PROC
      lea eax, x
      push eax
      push 0
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 100
      ;'='
      pop ebx
      pop eax
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      lea eax, x
      push eax
      push 0
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 1
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 1
      ;'='
      pop ebx
      pop eax
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      lea eax, x
      push eax
      push 1
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 10
      ;'='
      pop ebx
      pop eax
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      lea eax, x
      push eax
      push 1
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 1
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 11
      ;'='
      pop ebx
      pop eax
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      lea eax, x2
      push eax
      push 2
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 1
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 21
      ;'='
      pop ebx
      pop eax
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      lea eax, x2
      push eax
      push 1
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 2
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 12
      ;'='
      pop ebx
      pop eax
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      lea eax, x2
      push eax
      push 2
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 3
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 23
      ;'='
      pop ebx
      pop eax
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      lea eax, x2
      push eax
      push 3
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 2
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 32
      ;'='
      pop ebx
      pop eax
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      lea eax, x2
      push eax
      push 3
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 3
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 33
      ;'='
      pop ebx
      pop eax
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      lea eax, x2
      push eax
      push 2
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 20
      ;'='
      pop ebx
      pop eax
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      lea eax, x3
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 1
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 2
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 3
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 123
      ;'='
      pop ebx
      pop eax
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      lea eax, x3
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 3
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 3
      ;'='
      pop ebx
      pop eax
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      lea eax, x3
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      ;'='
      pop ebx
      pop eax
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      lea eax, x3
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 1
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 1
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 11
      ;'='
      pop ebx
      pop eax
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      lea eax, x3
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 1
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 2
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 2
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 122
      ;'='
      pop ebx
      pop eax
      mov [eax], ebx
      push [eax]
      pop edi
      ;";"
      lea eax, x
      push eax
      push 0
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push [eax]
      push offset str1
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      lea eax, x
      push eax
      push 0
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 1
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push [eax]
      push offset str2
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      lea eax, x
      push eax
      push 1
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push [eax]
      push offset str3
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      lea eax, x
      push eax
      push 1
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 1
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push [eax]
      push offset str4
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      lea eax, x2
      push eax
      push 2
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 1
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push [eax]
      push offset str5
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      lea eax, x2
      push eax
      push 1
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 2
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push [eax]
      push offset str6
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      lea eax, x2
      push eax
      push 2
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 3
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push [eax]
      push offset str7
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      lea eax, x2
      push eax
      push 3
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 2
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push [eax]
      push offset str8
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      lea eax, x2
      push eax
      push 3
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 3
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push [eax]
      push offset str9
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      lea eax, x2
      push eax
      push 2
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push [eax]
      push offset str10
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      lea eax, x3
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 1
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 2
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 3
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push [eax]
      push offset str11
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      lea eax, x3
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 3
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push [eax]
      push offset str12
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      lea eax, x3
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push [eax]
      push offset str13
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      lea eax, x3
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 0
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 1
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 1
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push [eax]
      push offset str14
      lea eax, printf
      push eax
      pop eax
      call eax
      push edi
      pop edi
      ;";"
      lea eax, x3
      push eax
      push 0
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 1
      push 8
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 2
      push 12
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push 2
      push 4
      ;"*"
      pop ebx
      pop eax
      imul eax, ebx
      push eax
      ;"[]"
      pop ebx
      pop eax
      add eax, ebx
      push [eax]
      push offset str15
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
