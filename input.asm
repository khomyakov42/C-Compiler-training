.386
.model flat, stdcall
   include C:\masm32\include\msvcrt.inc
   includelib C:\masm32\lib\msvcrt.lib
.data
   str1 BYTE 11 dup("%d = 100\n", 0)
   str2 BYTE 9 dup("%d = 1\n", 0)
   str3 BYTE 10 dup("%d = 10\n", 0)
   str4 BYTE 10 dup("%d = 11\n", 0)
   str5 BYTE 10 dup("%d = 21\n", 0)
   str6 BYTE 10 dup("%d = 12\n", 0)
   str7 BYTE 10 dup("%d = 23\n", 0)
   str8 BYTE 10 dup("%d = 32\n", 0)
   str9 BYTE 10 dup("%d = 33\n", 0)
   str10 BYTE 10 dup("%d = 20\n", 0)
   str11 BYTE 11 dup("%d = 123\n", 0)
   str12 BYTE 9 dup("%d = 3\n", 0)
   str13 BYTE 9 dup("%d = 0\n", 0)
   str14 BYTE 10 dup("%d = 11\n", 0)
   str15 BYTE 11 dup("%d = 122\n", 0)
   x DWORD 6 dup(?)
   x2 DWORD 12 dup(?)
   x3 DWORD 24 dup(?)
.code
   extern printf:near
   extern scanf:near
   extern getchar:near
   main PROC
      push 100
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      push 1
      push 4
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      push 10
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      push 11
      push 4
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      push 21
      push 4
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x2
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      push 12
      push 4
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x2
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      push 23
      push 4
      push 3
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x2
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      push 32
      push 4
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 3
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x2
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      push 33
      push 4
      push 3
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 3
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x2
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      push 20
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x2
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      push 123
      push 4
      push 3
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x3
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      push 3
      push 4
      push 3
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x3
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      push 0
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x3
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      push 11
      push 4
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x3
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      push 122
      push 4
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x3
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
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
      push 4
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push offset str2
      lea eax, printf
      push eax
      pop eax
      call eax
      pop edi
      pop edi
      push edi
      pop edi
      ;";"
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push offset str3
      lea eax, printf
      push eax
      pop eax
      call eax
      pop edi
      pop edi
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
      push 8
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      pop edi
      pop edi
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
      push 12
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x2
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      pop edi
      pop edi
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
      push 12
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x2
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      pop edi
      pop edi
      push edi
      pop edi
      ;";"
      push 4
      push 3
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x2
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
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
      pop edi
      pop edi
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
      push 12
      push 3
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x2
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push offset str8
      lea eax, printf
      push eax
      pop eax
      call eax
      pop edi
      pop edi
      push edi
      pop edi
      ;";"
      push 4
      push 3
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 3
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x2
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push offset str9
      lea eax, printf
      push eax
      pop eax
      call eax
      pop edi
      pop edi
      push edi
      pop edi
      ;";"
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x2
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push offset str10
      lea eax, printf
      push eax
      pop eax
      call eax
      pop edi
      pop edi
      push edi
      pop edi
      ;";"
      push 4
      push 3
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x3
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push offset str11
      lea eax, printf
      push eax
      pop eax
      call eax
      pop edi
      pop edi
      push edi
      pop edi
      ;";"
      push 4
      push 3
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x3
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push offset str12
      lea eax, printf
      push eax
      pop eax
      call eax
      pop edi
      pop edi
      push edi
      pop edi
      ;";"
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 12
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x3
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push offset str13
      lea eax, printf
      push eax
      pop eax
      call eax
      pop edi
      pop edi
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
      push 12
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x3
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push offset str14
      lea eax, printf
      push eax
      pop eax
      call eax
      pop edi
      pop edi
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
      push 12
      push 2
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 8
      push 1
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      push 4
      push 0
      ;"*"
      pop eax
      pop ebx
      imul eax, ebx
      push eax
      lea eax, x3
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      ;"+"
      pop eax
      pop ebx
      add eax, ebx
      push eax
      pop eax
      mov eax, [eax]
      push eax
      push offset str15
      lea eax, printf
      push eax
      pop eax
      call eax
      pop edi
      pop edi
      push edi
      pop edi
      ;";"
      lea eax, getchar
      push eax
      pop eax
      call eax
      pop eax
      push eax
      pop edi
      ;";"
      RET
   main ENDP


start:
   call main
   RET
end start
