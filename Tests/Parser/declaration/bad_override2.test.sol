<<<table #1>>>
<tags>
T record
   <<<table #4>>>
   <tags>
   <types>
   <vars>
   var x type int
 endrecord
<types>
int
char
double
void
<vars>
var printf type function printf(var  type  to char, ){
} returned void
var scanf type function scanf(var  type  to char, ){
} returned void
var getchar type function getchar(){
} returned int
var f type function (){
              <<<table #3>>>
              <tags>
              <types>
              <vars>
              └──{block}
                 └──{return}
                    └──{5}

} returned int
Семантическая ошибка в строке 7, позиции 3: переопределение f

