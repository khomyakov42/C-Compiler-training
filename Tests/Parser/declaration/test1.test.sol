<<<table #1>>>
<tags>
<types>
int
char
double
void
<vars>
var x type int
���{=}
   ���{x}
   ���{5}

var f type function (){
              <<<table #3>>>
              <tags>
              <types>
              <vars>
              ���{block}
                 ���{return}
                    ���{+}
                       ���{x}
                       ���{4}

} returned int
var up type function (){
               <<<table #5>>>
               <tags>
               <types>
               <vars>
               ���{block}
                  ���{+=}
                     ���{x}
                     ���{4}

} returned void
var main type function (){
                 <<<table #7>>>
                 <tags>
                 <types>
                 <vars>
                 var Z type pointer to array size 20 of pointer to  record
                                                                      <<<table #8>>>
                                                                      <tags>
                                                                      <types>
                                                                      <vars>
                                                                      var x type int
                                                                    endrecord
                 var x type int
                 ���{=}
                    ���{x}
                    ���{0}

                 ���{block}
                    ���{=}
                    �  ���{x}
                    �  ���{call}
                    �     ���{f}
                    ���{call}
                    �  ���{up}
                    ���{empty statement}

} returned void
�������᪠� �訡�� � ��ப� 16, ����樨 10: �㭪�� �� ������ �������� ���祭��

