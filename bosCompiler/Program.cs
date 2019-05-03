using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Antlr4.Runtime.Tree;
using System.Reflection;
using System.Linq;

namespace BosTranspiler
{
    class Program
    {
        private static string BasePath
        {
            get
            {
                string path = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "../../..");
                return path;
            }
        }

        private static List<(string file, string text)> SourceCode =
         new List<(string file, string text)>();
        private static string NameProgram = "bosCode";

        public static Object locker = new object();

        static void Main(string[] args)
        {
            var dirName = $"{BasePath}/SampleCode/";
            var outDirName = $"{BasePath}/TransOutput/";
            Directory.CreateDirectory(outDirName);

            // Delete ALL old artifacts
            if (Directory.Exists(outDirName + "/files"))
            {
                Directory.Delete(outDirName + "/files", true);
            }

            Console.WriteLine($"=== Starting Transpilation ===");
            Console.WriteLine(Environment.NewLine);
            PrintBanner("Starting Parallel Parsing");

            Stopwatch sw = Stopwatch.StartNew();
            var firstPassErrors = new List<Error>();

            Parallel.ForEach(Directory.EnumerateFiles(dirName, "*.bos"), bosFile =>
            {
                // parsing and first processing (pass)
                ParseFile(bosFile, ref firstPassErrors);
            });

            if (firstPassErrors.Count == 0)
            {
                Console.WriteLine("No Errors!");
            }
            else
            {
                Console.WriteLine("The following errors were found: ");
                foreach (var item in firstPassErrors)
                {
                    Console.WriteLine(item.Msg);
                }
            }
            PrintBanner("Ending Parallel Parsing");

            // Second processing (pass) - transpilation
            TranspileCode(FixerListener.StructuresWithInit);
            // Compilation of the transpiled files
            CompileCode(NameProgram);

            Console.WriteLine("=== Process of Transpilation completed ===");
            sw.Stop();
            Console.WriteLine($"Time elapsed {sw.Elapsed}");
        }

        private static void ParseFile(string bosFile, ref List<Error> firstPassErrors)
        {
            Console.WriteLine($"Parsing {Path.GetFileName(bosFile)}");

            ICharStream inputStream = CharStreams.fromPath(bosFile);
            BosLexer lexer = new BosLexer(inputStream);
            var tokenStreams = new CommonTokenStream(lexer);
            BosParser parser = new BosParser(tokenStreams);

            // remove standard listener to add custom
            parser.RemoveErrorListeners();
            lexer.RemoveErrorListeners();

            BosErrorListener errorListener = new BosErrorListener(ref firstPassErrors);
            lexer.AddErrorListener(errorListener);
            parser.AddErrorListener(errorListener);

            var parsedTree = parser.startRule();

            // first processing (pass) of transpilation
            FixerListener listener = new FixerListener(tokenStreams, Path.GetFileName(bosFile));
            ParseTreeWalker.Default.Walk(listener, parsedTree);

            if (listener.IsMainFile)
                NameProgram = listener.FileName;

            // Ensure additions are multi-thread safe
            lock (locker)
            {
                // Adding of a tuple element to the list
                SourceCode.Add((file: bosFile, text: listener.GetText()));
            }
            // throw new NotImplementedException();
        }

        private static List<IToken> CollectTokens(CommonTokenStream cts)
        {
            IList<IToken> lTokenList = cts.Get(0, cts.Size);
            List<IToken> tokens = new List<IToken>();
            foreach (IToken it in lTokenList)
            {
                if (it.Channel == 2)
                    tokens.Add(it);
            }
            return tokens;
        }

        private static void CompileCode(string nameProgram)
        {
            PrintBanner("Starting Compilation");

            List<SyntaxTree> vbTrees = new List<SyntaxTree>();
            var files = Directory.EnumerateFiles($"{BasePath}/TransOutput/transpiled_files");
            foreach (var file in files)
            {
                vbTrees.Add(
                    VisualBasicSyntaxTree.ParseText(File.ReadAllText(file))
                    .WithFilePath(file));
            }

            string runtimePath = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.2\{0}.dll";
            var refg = MetadataReference.CreateFromFile(String.Format(runtimePath, "System"));
            var regf = MetadataReference.CreateFromFile(String.Format(runtimePath, "System.Core"));
            var refms = MetadataReference.CreateFromFile(String.Format(runtimePath, "mscorlib"));
            //new AssemblyName("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")).Location
            // gathering the assemblies
            HashSet<MetadataReference> references = new HashSet<MetadataReference> {
                //MetadataReference.CreateFromFile(String.Format(runtimePath, "System")),
                //MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")).Location),
                MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")).Location),
                MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")).Location),
                //MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Console, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")).Location),
                //MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")).Location),
                // load the assemblies needed for the runtime
                 MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")).Location)
            };
            references.Add(regf);
            references.Add(refg);
            references.Add(refms);


            // All required options for compiling
            var options = new VisualBasicCompilationOptions(
                outputKind: OutputKind.ConsoleApplication,
                optimizationLevel: OptimizationLevel.Debug,
                platform: Platform.AnyCpu,
                optionInfer: true,
                optionStrict: OptionStrict.Off,
                optionExplicit: true,
                concurrentBuild: true,
                checkOverflow: false,
                deterministic: true,
                rootNamespace: nameProgram,
                parseOptions: VisualBasicParseOptions.Default
            );

            var compilation = VisualBasicCompilation.Create(
                nameProgram,
                vbTrees,
                references,
                options
            );

            Directory.CreateDirectory($"{BasePath}/TransOutput/compilation/");
            var emit = compilation.Emit($"{BasePath}/TransOutput/compilation/{nameProgram}.exe");

            if (!emit.Success)
                ReportErrorsAfterCompilation(emit);
            else
            {
                CopyDllsAfterCompilation();
                PrintBanner("Ending Compilation");
            }
        }

        private static void PrintBanner(string text)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("==========================");
            Console.WriteLine($"== {text} ==");
            Console.WriteLine("==========================");
            Console.WriteLine(Environment.NewLine);
        }

        private static void ReportErrorsAfterCompilation(Microsoft.CodeAnalysis.Emit.EmitResult emit)
        {
            Console.WriteLine("Compilation unsuccessful");
            Console.WriteLine("The following errors were found:");
            //var fmtProv = new System.Globalization.CultureInfo("en-US");
            var fmtProv = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            foreach (var diags in emit.Diagnostics)
            {
                if (diags.Severity == DiagnosticSeverity.Error)
                {
                    Console.WriteLine(diags.GetMessage(fmtProv));
                }
            }

            // We also log the error
            using (StreamWriter errors = new StreamWriter($"{BasePath}/TransOutput/compilation/errors.txt"))
            {
                foreach (var diags in emit.Diagnostics)
                {
                    if (diags.Severity == DiagnosticSeverity.Error)
                    {
                        errors.WriteLine($"{{{Path.GetFileName(diags.Location?.SourceTree?.FilePath)}}} {diags.Location.GetLineSpan().StartLinePosition} {diags.GetMessage(fmtProv)}");
                    }

                }
            }
        }

        private static void CopyDllsAfterCompilation()
        {
            Directory.CreateDirectory($"{BasePath}/TransOutput/compilation/x86");
            Directory.CreateDirectory($"{BasePath}/TransOutput/compilation/x64");
            // we have to copy the Dlls for the runtime
            foreach (var libFile in Directory.EnumerateFiles($"{BasePath}\\Dlls\\"))
            {
                File.Copy(libFile, $"{BasePath}\\TransOutput\\compilation\\{Path.GetFileName(libFile)}", true);
            }
            foreach (var libFile in Directory.EnumerateFiles($"{BasePath}\\Dlls\\x86\\"))
            {
                File.Copy(libFile, $"{BasePath}\\TransOutput\\compilation\\x86\\{Path.GetFileName(libFile)}", true);
            }
            foreach (var libFile in Directory.EnumerateFiles($"{BasePath}\\Dlls\\x64\\"))
            {
                File.Copy(libFile, $"{BasePath}\\TransOutput\\compilation\\x64\\{Path.GetFileName(libFile)}", true);
            }
        }

        private static void TranspileCode(List<string> structuresWithInitializer)
        {
            List<SyntaxTree> bosTrees = new List<SyntaxTree>();
            BosRewriter rewriter = new BosRewriter(structuresWithInitializer);

            Parallel.ForEach(SourceCode, src => {
                Console.WriteLine($"Completing transpilation of {Path.GetFileName(src.file)}");
                bosTrees.Add(
                    rewriter.Visit(
                        VisualBasicSyntaxTree.ParseText(src.text).GetCompilationUnitRoot())
                    .SyntaxTree
                    .WithFilePath(src.file)
                );
            });

            // Unnecesary at the moment
            //bosTrees.Add(VisualBasicSyntaxTree
            //    .ParseText(File.ReadAllText($"{BasePath}/Libs/Runtime.vb"))
            //    .WithFilePath($"{BasePath}/Libs/Runtime.vb"));

            Directory.CreateDirectory($"{BasePath}/TransOutput/transpiled_files/");

            foreach (var vbTree in bosTrees)
            {
                string fName = Path.GetFileName(vbTree.FilePath);
                if (fName.LastIndexOf(".") != -1)
                    fName = fName.Substring(0, fName.LastIndexOf("."));

                fName = fName + ".vb";

                Console.WriteLine($"Writing on disk VB.NET version of {Path.GetFileName(vbTree.FilePath)}");
                File.WriteAllText($"{BasePath}/TransOutput/transpiled_files/{fName}", vbTree.ToString());
            }
        }

        #region Old tryout program's functions

        // The old tryout program uses the visitor pattern from ANTLR
        private string GetInput()
        {
            Console.WriteLine("Enter to evaluate: ");
            return Console.ReadLine();
        }

        private void DisplayError(Exception ex)
        {
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

        #endregion
    }
}
