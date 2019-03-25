using Antlr4.Runtime;
using System;

namespace bosCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            try {
                string input = program.GetInput();
                int result = program.EvaluateInput(input);

                program.DisplayResult(result);

            } catch (Exception ex ) {
                program.DisplayError(ex);
            }
            Console.Write($"{Environment.NewLine} Press enter to exit: ");
            Console.ReadKey();
        }

        private string GetInput() {
            Console.WriteLine("Enter to evaluate: ");
            return Console.ReadLine();
        }

        private void DisplayResult(int result) {
            Console.WriteLine($"Result: {result}");
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
