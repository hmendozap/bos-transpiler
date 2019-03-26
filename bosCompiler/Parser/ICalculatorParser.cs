//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.7.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Calculator.g4 by ANTLR 4.7.2

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.7.2")]
[System.CLSCompliant(false)]
public partial class ICalculatorParser : Parser {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		LPAREN=1, RPAREN=2, OPERATOR=3, ADD=4, SUBTRACT=5, MULTIPLY=6, DIVIDE=7,
		DIGIT=8, WS=9;
	public const int
		RULE_expression = 0, RULE_operand = 1;
	public static readonly string[] ruleNames = {
		"expression", "operand"
	};

	private static readonly string[] _LiteralNames = {
		null, "'('", "')'", null, "'+'", "'-'", "'*'", "'/'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "LPAREN", "RPAREN", "OPERATOR", "ADD", "SUBTRACT", "MULTIPLY", "DIVIDE",
		"DIGIT", "WS"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "Calculator.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static ICalculatorParser() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}

		public ICalculatorParser(ITokenStream input) : this(input, Console.Out, Console.Error) { }

		public ICalculatorParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	public partial class ExpressionContext : ParserRuleContext {
		public OperandContext[] operand() {
			return GetRuleContexts<OperandContext>();
		}
		public OperandContext operand(int i) {
			return GetRuleContext<OperandContext>(i);
		}
		public ITerminalNode[] OPERATOR() { return GetTokens(ICalculatorParser.OPERATOR); }
		public ITerminalNode OPERATOR(int i) {
			return GetToken(ICalculatorParser.OPERATOR, i);
		}
		public ExpressionContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_expression; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			ICalculatorVisitor<TResult> typedVisitor = visitor as ICalculatorVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitExpression(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public ExpressionContext expression() {
		ExpressionContext _localctx = new ExpressionContext(Context, State);
		EnterRule(_localctx, 0, RULE_expression);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 4; operand();
			State = 7;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			do {
				{
				{
				State = 5; Match(OPERATOR);
				State = 6; operand();
				}
				}
				State = 9;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			} while ( _la==OPERATOR );
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class OperandContext : ParserRuleContext {
		public ITerminalNode DIGIT() { return GetToken(ICalculatorParser.DIGIT, 0); }
		public ITerminalNode LPAREN() { return GetToken(ICalculatorParser.LPAREN, 0); }
		public OperandContext[] operand() {
			return GetRuleContexts<OperandContext>();
		}
		public OperandContext operand(int i) {
			return GetRuleContext<OperandContext>(i);
		}
		public ITerminalNode RPAREN() { return GetToken(ICalculatorParser.RPAREN, 0); }
		public ITerminalNode[] OPERATOR() { return GetTokens(ICalculatorParser.OPERATOR); }
		public ITerminalNode OPERATOR(int i) {
			return GetToken(ICalculatorParser.OPERATOR, i);
		}
		public OperandContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_operand; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			ICalculatorVisitor<TResult> typedVisitor = visitor as ICalculatorVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitOperand(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public OperandContext operand() {
		OperandContext _localctx = new OperandContext(Context, State);
		EnterRule(_localctx, 2, RULE_operand);
		int _la;
		try {
			State = 22;
			ErrorHandler.Sync(this);
			switch (TokenStream.LA(1)) {
			case DIGIT:
				EnterOuterAlt(_localctx, 1);
				{
				State = 11; Match(DIGIT);
				}
				break;
			case LPAREN:
				EnterOuterAlt(_localctx, 2);
				{
				State = 12; Match(LPAREN);
				State = 13; operand();
				State = 16;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
				do {
					{
					{
					State = 14; Match(OPERATOR);
					State = 15; operand();
					}
					}
					State = 18;
					ErrorHandler.Sync(this);
					_la = TokenStream.LA(1);
				} while ( _la==OPERATOR );
				State = 20; Match(RPAREN);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	private static char[] _serializedATN = {
		'\x3', '\x608B', '\xA72A', '\x8133', '\xB9ED', '\x417C', '\x3BE7', '\x7786',
		'\x5964', '\x3', '\v', '\x1B', '\x4', '\x2', '\t', '\x2', '\x4', '\x3',
		'\t', '\x3', '\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x6', '\x2', '\n',
		'\n', '\x2', '\r', '\x2', '\xE', '\x2', '\v', '\x3', '\x3', '\x3', '\x3',
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x6', '\x3', '\x13', '\n',
		'\x3', '\r', '\x3', '\xE', '\x3', '\x14', '\x3', '\x3', '\x3', '\x3',
		'\x5', '\x3', '\x19', '\n', '\x3', '\x3', '\x3', '\x2', '\x2', '\x4',
		'\x2', '\x4', '\x2', '\x2', '\x2', '\x1B', '\x2', '\x6', '\x3', '\x2',
		'\x2', '\x2', '\x4', '\x18', '\x3', '\x2', '\x2', '\x2', '\x6', '\t',
		'\x5', '\x4', '\x3', '\x2', '\a', '\b', '\a', '\x5', '\x2', '\x2', '\b',
		'\n', '\x5', '\x4', '\x3', '\x2', '\t', '\a', '\x3', '\x2', '\x2', '\x2',
		'\n', '\v', '\x3', '\x2', '\x2', '\x2', '\v', '\t', '\x3', '\x2', '\x2',
		'\x2', '\v', '\f', '\x3', '\x2', '\x2', '\x2', '\f', '\x3', '\x3', '\x2',
		'\x2', '\x2', '\r', '\x19', '\a', '\n', '\x2', '\x2', '\xE', '\xF', '\a',
		'\x3', '\x2', '\x2', '\xF', '\x12', '\x5', '\x4', '\x3', '\x2', '\x10',
		'\x11', '\a', '\x5', '\x2', '\x2', '\x11', '\x13', '\x5', '\x4', '\x3',
		'\x2', '\x12', '\x10', '\x3', '\x2', '\x2', '\x2', '\x13', '\x14', '\x3',
		'\x2', '\x2', '\x2', '\x14', '\x12', '\x3', '\x2', '\x2', '\x2', '\x14',
		'\x15', '\x3', '\x2', '\x2', '\x2', '\x15', '\x16', '\x3', '\x2', '\x2',
		'\x2', '\x16', '\x17', '\a', '\x4', '\x2', '\x2', '\x17', '\x19', '\x3',
		'\x2', '\x2', '\x2', '\x18', '\r', '\x3', '\x2', '\x2', '\x2', '\x18',
		'\xE', '\x3', '\x2', '\x2', '\x2', '\x19', '\x5', '\x3', '\x2', '\x2',
		'\x2', '\x5', '\v', '\x14', '\x18',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
