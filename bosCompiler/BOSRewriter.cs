using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace BosTranspiler
{
    // Actually is like a visitor (using ANTLR Terminology)
    public class BOSRewriter : VisualBasicSyntaxRewriter
    {
        private List<string> structuresWithInit;
        private StatementSyntax CreateInitializer(string identifier, string type, bool isArray)
        {
            if (isArray)
            {
                return SyntaxFactory.ParseExecutableStatement($"For Index=0 To {identifier}.Length - 1" +
                $"{Environment.NewLine}Call {identifier}(Index).Init{type}(){Environment.NewLine}"+
                $"Next{Environment.NewLine}");
            }
            else
            {
                return SyntaxFactory.ParseExecutableStatement($"Call {identifier}.Init{type}()");
            }
        }

        public BOSRewriter(List<string> structuresWithInit)
        {
            this.structuresWithInit = structuresWithInit;
        }

        public override SyntaxNode VisitModuleBlock(ModuleBlockSyntax node)
        {
            var initInvocations = new SyntaxList<StatementSyntax>();

            var space = SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " ");
            var newline = SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, Environment.NewLine);

            for (int a = 0; a < node.Members.Count; a++)
            {
                if (node.Members[a].IsKind(SyntaxKind.FieldDeclaration))
                {
                    foreach (var declarators in (node.Members[a] as FieldDeclarationSyntax).Declarators)
                    {
                        // Checks for variable declaration 
                        string item = declarators?.AsClause?.Type().WithoutTrivia().ToString();
                        if (structuresWithInit.Contains(item))
                        {
                            foreach (var name in declarators.Names)
                            {
                                if (name.ArrayBounds != null)
                                {
                                    initInvocations.Add(
                                        CreateInitializer(
                                            name.Identifier.ToFullString().Trim(), item, true)
                                        .WithTrailingTrivia(newline));
                                }
                                else
                                {
                                    initInvocations.Add(
                                        CreateInitializer(
                                            name.Identifier.ToFullString().Trim(), item, false)
                                        .WithTrailingTrivia(newline));
                                }
                            }
                        } // end check of structures
                    }
                } // end check of field declarations
            }

            if (initInvocations.Count > 0)
            {
                // Sub New() Statement
                var subStart = SyntaxFactory.SubStatement(
                    SyntaxFactory.Identifier("New()").WithLeadingTrivia(space))
                    .WithLeadingTrivia(newline)
                    .WithTrailingTrivia(newline);

                // End Sub statement
                var subEnd = SyntaxFactory.EndSubStatement(
                    SyntaxFactory.Token(SyntaxKind.EndIfKeyword, "End ") .WithLeadingTrivia(newline),
                    SyntaxFactory.Token(SyntaxKind.SubKeyword, "Sub"))
                    .WithTrailingTrivia(newline);

                // Initializer for the module part of the transpiled file
                var moduleConstructor = SyntaxFactory.SubBlock(subStart, initInvocations, subEnd);

                // WithMembers => adds the specified members to the node declaration
                // it makes sense because the node is the root (I think)
                node = node.WithMembers(node.Members.Add(moduleConstructor));
            }
            // Since we want to continue the visit, we call the base method
            return base.VisitModuleBlock(node);
        }

        public override SyntaxNode VisitArgumentList(ArgumentListSyntax node)
        {
            SeparatedSyntaxList<ArgumentSyntax> arguments = node.Arguments;

            for (int i = 0; i < arguments.Count; i++)
            {
                ArgumentSyntax arg = arguments[i];
                if (arg.IsKind(SyntaxKind.RangeArgument))
                {
                    SyntaxToken tokn = arguments[i].GetFirstToken();
                    if (tokn.IsKind(SyntaxKind.IntegerLiteralToken))
                    {
                        if (tokn.ValueText == "1")
                        {
                            var newIndexStart = SyntaxFactory.IntegerLiteralToken(
                                "0", LiteralBase.Decimal, TypeCharacter.IntegerLiteral, 0);

                            var newArrayArgument = arg.ReplaceToken(tokn, newIndexStart);
                            arguments = arguments.Replace(arg, newArrayArgument);
                        }
                    }
                }
            }

            return node.WithArguments(arguments);
        }
    }
}