using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;


namespace BosTranspiler
{
    class Program
    {
        static string BasePath = @"C:\Users\hmend\vsCode_Projects\BOS-Transpilation\";

        private static List<(string file, string text)> SourceCode =
         new List<(string text, string file)>();
        private static string NameProgram = "bosCode";

        public static Object locker = new object();

        static void Main(string[] args)
        {
            var dirName = $"{BasePath}/SampleCode/";
            var outDirName = $"{BasePath}/TransOutput/";
            Directory.CreateDirectory(outDirName);

            // Delete ALL old artifacts
            if (Directory.Exists(outDirName + "\\files")) {
                Directory.Delete(outDirName + "\\files", true);
            }

            Console.WriteLine($"=== Starting Transpilation ===");
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("===============================");
            Console.WriteLine("== Starting Parallel Parsing ==");
            Console.WriteLine("===============================");
            Console.WriteLine(Environment.NewLine);

            Stopwatch sw = Stopwatch.StartNew();
            var firstPassErrors = new List<Error>();

            Parallel.ForEach(Directory.EnumerateFiles(dirName, "bos"), bosFile => {
                // parsing and first pass
                ParseFile(bosFile, ref firstPassErrors);
            });
             
            if (firstPassErrors.Count == 0) {
                Console.WriteLine("No Errors!");
            } else {
                Console.WriteLine("The following errors were found: ");
                foreach (var item in firstPassErrors) {
                    Console.WriteLine(item.Msg);
                }
            }
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("=============================");
            Console.WriteLine("== Ending Parallel Parsing ==");
            Console.WriteLine("=============================");
            Console.WriteLine(Environment.NewLine);

            // Second pass - transpilation
            TranspileCode(FixerListener.StructuresWithInit);
            // Compilation of the transpiled files
            CompileCode(NameProgram);

            Console.WriteLine("=== Process of Transpilation completed ===");
            sw.Stop();

            Console.WriteLine("=== Process of Transpilation completed ===");
            Console.WriteLine($"Time elapsed {sw.Elapsed}");

            #region OldExample

            Program program = new Program();
            try {
                string input = program.GetInput();
                int result = program.EvaluateInput(input);

                //program.DisplayResult(result);

            } catch (Exception ex ) {
                program.DisplayError(ex);
            }
            Console.Write($"{Environment.NewLine} Press enter to exit: ");
            Console.ReadKey();
            #endregion 
        }

        private static void CompileCode(string nameProgram)
        {
            throw new NotImplementedException();
        }

        private static void TranspileCode(List<string> structuresWithInit)
        {
            List<SyntaxTree> bosTrees = new List<SyntaxTree>();
            BOSRewriter rewriter = new BOSRewriter(structuresWithInit);
            Parallel.ForEach(SourceCode, src => {
                Console.WriteLine($"Completing transpilation of {Path.GetFileName(src.file)}");
                bosTrees.Add(rewriter.Visit(VisualBasicSyntaxTree.ParseText(src.text).GetCompilationUnitRoot()).SyntaxTree.WithFilePath(src.file));
            });
            
            throw new NotImplementedException();
        }

        private static void ParseFile(string bosFile, ref List<Error> firstPassErrors)
        {
            Console.WriteLine($"Parsing {bosFile}");

            ICharStream inputStream = CharStreams.fromPath(bosFile);
            BOSLexer lexer = new BOSLexer(inputStream);
            var tokenStreams = new CommonTokenStream(lexer);
            BOSParser parser = new BOSParser(tokenStreams);

            // remove standard listener to add custom
            parser.RemoveErrorListeners();
            lexer.RemoveErrorListeners();

            BOSErrorListener errorListener = new BOSErrorListener();
            lexer.AddErrorListener(errorListener);
            parser.AddErrorListener(errorListener);

            tree = parser.startRule();

            FixerListener fixer = new FixerListener(tokenStreams, Path.GetFileName(bosFile));
            ParserTreeWalker.Default.Walk(fixer, tree);

            if (fixer.IsMainFile) {
                NameProgram = fixer.FileName;
            }
            // Ensure additions are multi-thread safe
            lock (locker) {
                SourceCode.Add(bosFile, fixer.GetText());
            }
            // throw new NotImplementedException();
        }

        private string GetInput() {
            Console.WriteLine("Enter to evaluate: ");
            return Console.ReadLine();
        }

        private void DisplayError(Exception ex) {
            Console.WriteLine("Something didn't go as expected'");
            Console.WriteLine(ex.Message);
        }

        private int EvaluateInput(string input)
        {
            CalculatorLexer lexer = new CalculatorLexer(new AntlrInputStream(input));
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new ThrowingErrorListener<int>());

            ICalculatorParser parser = new ICalculatorParser(new CommonTokenStream(lexer));
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new ThrowingErrorListener<IToken>());

            return new CalculatorVisitor().Visit(parser.expression());
        }

    }
}
