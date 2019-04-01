﻿using Antlr4.Runtime;
using System;
using System.Collections.Generic;

namespace BosTranspiler
{
    // Remember that listener will automatically call the Enter & Exit methods
    // whenever finds the corresponding node (e.g. EnterAssignOperator & EndAssignOperator)
    internal class FixerListener : BOSBaseListener
    {
        #region  Private Props
        private string FileName {get; set; };
        private bool IsMainFile { get; set; } = false;

        // Needed to rewrite "wrong" parts
        private CommonTokenStream TokenStream {get; set; };
        private TokenStreamRewriter StreamRewriter { get; set; }

        private Dictionary<string, StructureInitializer> InitStructures { get; set; } =
         new Dictionary<string, StructureInitializer>();

        private Stack<string> Types { get; set; } = new Stack<string>();
        // Token used to locate where the imports of VB.NET should start
        private IToken TokenImport { get; set; } = null;
        #endregion

        public static List<string> StructuresWithInit = new List<string>();

        public FixerListener(CommonTokenStream tokenStreams, string fileName)
        {
            TokenStream = tokenStreams;
            // FileName = fileName;
            if (fileName.LastIndexOf('.') != -1) {
                FileName = fileName.Substring(0, fileName.LastIndexOf('.'));
            } else {
                FileName = fileName;
            }
            StreamRewriter = new TokenStreamRewriter(tokenStreams);
        }
        // Our listener inherits from the base listener that is generated by 
        // ANTLR. Inheriting from this listener allows us to implement only the 
        // methods we need. The rest remain the default empty one, like the 
        // following example.

        // public virtual void ExitStartRule([NotNull] BOSParser.StartRuleContext ruleContext) { }

        public override void EnterAttributeStmt([NotNull] BOSParser.AttributeStmtContext context) {
            // AttributeStmt Rule is in vba.g4 line 166

            // Initial state = Attribute VB_NAME = "ThisWorkbook"
            if (context.implicitCallStmt_InStmt().GetText() == "VB_Name") {
                // Attribute VB_NAME = ThisWorkbook (Remove "s)
                // It will only do it with the first apparearing literal in the rule
                FileName = context.literal()[0].GetText().Trim('"'); 
            }
            // Remove all attributes, from initial to end IToken
            StreamRewriter.Replace(context.Start, context.Stop, "");
        }

        public override void EnterModule(BOSParser.ModuleContext context) {
            // check if an option is present
            if (context?.moduleOptions() != null && !context.moduleOptions().IsEmpty) {
                TokenImport = context.moduleOptions().Stop;
            }
        }

        // Rule called after the building of the tree, is the exit function of the entry
        // parsing point. To wrap VBA In A Module (for example)
        public override void ExitStartRule([NotNull] BOSParser.StartRuleContext ruleContext) {
            //  If an option is present it must be before everything else, therefore we have
            // to check where to put the start of the module
            var baseText = $"Imports System {Environment.NewLine}{Environment.NewLine}Imports Microsoft.VisualBasic{Environment.NewLine}Imports System.Math{Environment.NewLine}Imports System.Linq{Environment.NewLine}Imports System.Collections.Generic{Environment.NewLine}{Environment.NewLine}Module {FileName}{Environment.NewLine}";

            if (TokenImport != null) {
                // add imports and module
                StreamRewriter.InsertAfter(TokenImport, $"{Environment.NewLine}{baseText}");
            } else {
                StreamRewriter.InsertBefore(ruleContext.Start, baseText);
            }

            StreamRewriter.InsertAfter(ruleContext.Stop.StopIndex, 
                $"{Environment.NewLine}End Module");

        }

        // Example to rewrite incompatible statements ( VBA.g4 Line 296)
        public override void EnterEraseStmt(BOSParser.EraseStmtContext context) {
            string arrayName = context.valueStmt()[0].GetText();
            var translatedStmt = $"Array.Clear({arrayName} , 0, {arrayName}.Length)";
            StreamRewriter.Replace(context.Start, context.Stop, translatedStmt);

        }

        // Returns the translated text
        public string GetText() {
            return StreamRewriter.GetText();
        }

    }
}