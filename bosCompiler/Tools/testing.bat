REM Test Automation
echo %cd%
REM if directory not bosCompiler
    REM then change to C:\Progress\Project\bos-transpiler\bosCompiler
REM endif
cd ..\Grammar\
antlr4 -o grOutput Bos.g4
javac grOutput\Bos*.java
cd grOutput
grun Bos startRule -tokens -gui ..\..\SampleCode\tesi.bos
