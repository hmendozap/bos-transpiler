using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

using OperandContext = ICalculatorParser.OperandContext;
using ExpressionContext = ICalculatorParser.ExpressionContext;

namespace BosTranspiler
{
    internal class CalculatorVisitor : CalculatorBaseVisitor<int>
    {
        #region Member Vars
        private readonly Dictionary<string, Func<int, int, int>> _funcMap =
            new Dictionary<string, Func<int, int, int>> {
                {"+", (a,b) => a + b },
                {"-", (a,b) => a - b },
                {"*", (a,b) => a * b },
                {"/", (a,b) => a / b },

            };

        #endregion

        public override int VisitExpression([NotNull] ExpressionContext context)
        {
            return HandleGroup(context.operand(), context.OPERATOR());
            //return base.VisitExpression(context);
        }

        public override int VisitOperand([NotNull] OperandContext context)
        {
            ITerminalNode digit = context.DIGIT();
            var result = 0;
            if (digit != null) {
                result = int.Parse(digit.GetText());
            } else {
                result = HandleGroup(context.operand(), context.OPERATOR());
            }
            return result;
        }

        private int HandleGroup(OperandContext[] operandCtxs, ITerminalNode[] operatorNodes)
        {
            // Visits Default implementation will eventually call the 
            // VisitOperand above
            List<int> operands = operandCtxs.Select(Visit).ToList();
            Queue<string> operators = new Queue<string>(
                operatorNodes.Select(o => o.GetText()));

            var result = operands.Aggregate(
                (a, c) => _funcMap[operators.Dequeue()](a, c) );
            return result;
        }
    }
}
