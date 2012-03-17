.386
.model flat, stdcall
   include C:\masm32\include\msvcrt.inc
   includelib C:\masm32\lib\msvcrt.lib
.data
   str1 BYTE 15 dup("%d + %d = %d", 13, 10, 0)
   str2 BYTE 15 dup("%d - %d = %d", 13, 10, 0)
   str3 BYTE 15 dup("%d * %d = %d", 13, 10, 0)
   str4 BYTE 15 dup("%d / %d = %d", 13, 10, 0)
   str5 BYTE 15 dup("%d % %d = %d", 13, 10, 0)
   str6 BYTE 16 dup("%d >> %d = %d", 13, 10, 0)
   str7 BYTE 16 dup("%d << %d = %d", 13, 10, 0)
   str8 BYTE 15 dup("%d & %d = %d", 13, 10, 0)
   str9 BYTE 15 dup("%d | %d = %d", 13, 10, 0)
   str10 BYTE 15 dup("%d ^ %d = %d", 13, 10, 0)
   str11 BYTE 16 dup("%d || %d = %d", 13, 10, 0)
   str12 BYTE 16 dup("%d && %d = %d", 13, 10, 0)
   str13 BYTE 16 dup("%d == %d = %d", 13, 10, 0)
   str14 BYTE 16 dup("%d != %d = %d", 13, 10, 0)
   str15 BYTE 15 dup("%d < %d = %d", 13, 10, 0)
   str16 BYTE 15 dup("%d > %d = %d", 13, 10, 0)
   str17 BYTE 16 dup("%d <= %d = %d", 13, 10, 0)
   str18 BYTE 16 dup("%d >= %d = %d", 13, 10, 0)
   x DWORD ?
   y DWORD ?
.code
   extern printf:near
   extern scanf:near
   extern getchar:near
   main PROC
      push x
      push y
      ;"+"
      pop ebx
      pop eax
      add eax, ebx
      push eax
      push y
      push x
      push offset str1
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;"-"
      pop ebx
      pop eax
      sub eax, ebx
      push eax
      push y
      push x
      push offset str2
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;"*"
      pop ebx
      pop eax
      imul eax
      push eax
      push y
      push x
      push offset str3
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;"/"
      pop ebx
      pop eax
      idiv eax, ebx
      push eax
      push y
      push x
      push offset str4
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;"%"
      pop ebx
      pop eax
      idiv eax
      push edx
      push y
      push x
      push offset str5
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;">>"
      pop ebx
      pop eax
      mov cl, ebx
      shr eax, cl
      push eax
      push y
      push x
      push offset str6
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;"<<"
      pop ebx
      pop eax
      mov cl, ebx
      shl eax, cl
      push eax
      push y
      push x
      push offset str7
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;"&"
      pop ebx
      pop eax
      and eax, ebx
      push eax
      push y
      push x
      push offset str8
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;"|"
      pop ebx
      pop eax
      or eax, ebx
      push eax
      push y
      push x
      push offset str9
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;"^"
      pop ebx
      pop eax
      xor eax, ebx
      push eax
      push y
      push x
      push offset str10
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;"||"
      pop ebx
      pop eax
      or eax, ebx
      push eax
      push y
      push x
      push offset str11
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;"&&"
      pop ebx
      pop eax
      and eax, ebx
      push eax
      push y
      push x
      push offset str12
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;"=="
      pop ebx
      pop eax
      cmp eax, ebx
      je label0
      jmp label2
label0:
      mov eax, 1
      jmp label1
label2:
      mov eax, 0
label1:
      push eax
      push y
      push x
      push offset str13
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;"!="
      pop ebx
      pop eax
      cmp eax, ebx
      jne label3
      jmp label5
label3:
      mov eax, 1
      jmp label4
label5:
      mov eax, 0
label4:
      push eax
      push y
      push x
      push offset str14
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;"<"
      pop ebx
      pop eax
      cmp eax, ebx
      jl label6
      jmp label8
label6:
      mov eax, 1
      jmp label7
label8:
      mov eax, 0
label7:
      push eax
      push y
      push x
      push offset str15
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;">"
      pop ebx
      pop eax
      cmp eax, ebx
      jg label9
      jmp label11
label9:
      mov eax, 1
      jmp label10
label11:
      mov eax, 0
label10:
      push eax
      push y
      push x
      push offset str16
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;"<="
      pop ebx
      pop eax
      cmp eax, ebx
      jle label12
      jmp label14
label12:
      mov eax, 1
      jmp label13
label14:
      mov eax, 0
label13:
      push eax
      push y
      push x
      push offset str17
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      push x
      push y
      ;">="
      pop ebx
      pop eax
      cmp eax, ebx
      jge label15
      jmp label17
label15:
      mov eax, 1
      jmp label16
label17:
      mov eax, 0
label16:
      push eax
      push y
      push x
      push offset str18
      lea eax, printf
      push eax
      pop eax
      call eax
      push edx
      pop edx
      ;";"
      lea eax, getchar
      push eax
      pop eax
      call eax
      pop eax
      push eax
      pop edx
      ;";"
      RET
   main ENDP


start:
   lea eax, x
   push eax
   push 1
   ;'='
   pop ebx
   pop eax
   mov [eax], ebx
   push [eax]
   lea eax, y
   push eax
   push 2
   ;'='
   pop ebx
   pop eax
   mov [eax], ebx
   push [eax]
   call main
   RET
end start
