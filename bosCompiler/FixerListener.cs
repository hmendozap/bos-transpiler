﻿using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BosTranspiler
{
    // Remember that listener will automatically call the Enter & Exit methods
    // whenever finds the corresponding node (e.g. EnterAssignOperator & EndAssignOperator)
    internal class FixerListener : BOSBaseListener
    {
        #region  Private Props

        // These are needed to rewrite "wrong" parts
        private CommonTokenStream TokenStream {get; set; }
        private TokenStreamRewriter StreamRewriter { get; set; }
        private Dictionary<string, StructureInitializer> InitStructures { get; set; } =
         new Dictionary<string, StructureInitializer>();
        private Stack<string> TypesStack { get; set; } = new Stack<string>();

        // Token used to locate where the imports of VB.NET should start
        private IToken TokenImport { get; set; } = null;
        #endregion

        #region Public Props
        public string FileName {get; set; }
        public bool IsMainFile { get; set; } = false;
        public static List<string> StructuresWithInit = new List<string>();
        #endregion

        public FixerListener(CommonTokenStream tokenStreams, string fileName)
        {
            TokenStream = tokenStreams;
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
            // AttributeStmt Rule is in vba.g4 line 166 / bos.g4 line 134

            // Initial state = Attribute VB_NAME = "ThisWorkbook"
            if (context.implicitCallStmt_InStmt().GetText() == "VB_Name") {
                // Attribute VB_NAME = ThisWorkbook (Remove "s)
                // It will only do it with the first apparearing literal in the rule
                FileName = context.literal()[0].GetText().Trim('"'); 
            }
            // Remove attributes, from initial to end IToken (We will delete all of them though)
            StreamRewriter.Replace(context.Start, context.Stop, "");
        }

        // Is not needed for BOS
        //public override void EnterModule(BOSParser.ModuleContext context) {
        //    // check if an option is present
        //    if (context?.moduleOptions() != null && !context.moduleOptions().IsEmpty) {
        //        TokenImport = context.moduleOptions().Stop;
        //    }
        //}

        // Rule called after the building of the tree, is the exit function of the entry
        // parsing point. To wrap VBA In A Module (for example)
        public override void ExitStartRule([NotNull] BOSParser.StartRuleContext ruleContext) {
            //  If an option is present it must be before everything else, therefore we have
            // to check where to put the start of the module
            var baseText = $"Imports System {Environment.NewLine}{Environment.NewLine}Imports Microsoft.VisualBasic{Environment.NewLine}Imports System.Math{Environment.NewLine}Imports System.Linq{Environment.NewLine}Imports System.Collections.Generic{Environment.NewLine}{Environment.NewLine}";
            var moduleBaseText = $"Module {FileName}{Environment.NewLine}";
            baseText = baseText + moduleBaseText;

            if (TokenImport != null) {
                // add imports and module
                StreamRewriter.InsertAfter(TokenImport, $"{Environment.NewLine}{baseText}");
            } else {
                StreamRewriter.InsertBefore(ruleContext.Start, baseText);
            }

            string moduleEndText = $"{Environment.NewLine}End Module";
            StreamRewriter.InsertAfter(ruleContext.Stop.StopIndex,
                moduleEndText);

        }

        // Example to rewrite incompatible statements ( VBA.g4 Line 296)
        public override void EnterEraseStmt(BOSParser.EraseStmtContext context) {
            string arrayName = context.valueStmt()[0].GetText();
            var translatedStmt = $"Array.Clear({arrayName} , 0, {arrayName}.Length)";
            StreamRewriter.Replace(context.Start, context.Stop, translatedStmt);
            // In the first processing we record the kind of array declared in any declaration
            // and in the second pass we wold transpile the erase statements
        }

        //transform a Type in a Structure
        // typeStmt rule (VBA.g4 Line 502)
        public override void EnterTypeStmt([NotNull] BOSParser.TypeStmtContext context)
        {
            // Find the type name or in the grammar the identifier
            var typeName = context.ambiguousIdentifier().GetText();
            TypesStack.Push(typeName);  // Store it in the stack, helpful when recreating the type
            // Used to create the new type
            InitStructures.Add(typeName, new StructureInitializer(typeName));
            StreamRewriter.Replace(context.TYPE().Symbol, "Structure");
            StreamRewriter.Replace(context.END_TYPE().Symbol, "End Structure");

            string visibility = context.visibility().GetText();
            foreach (var st in context.typeStmt_Element())
            {
                StreamRewriter.InsertBefore(st.Start, "${visibility} ");
            }
        }

        // Cannot initialize elements inside a Structure
        // since VBA Types are transformed in VB.NET Structures
        // Remove the initialization of array
        public override void ExitTypeStmt_Element([NotNull] BOSParser.TypeStmt_ElementContext context)
        {
            // Peek: See the next element, without removing it frmo stack
            var currentType = TypesStack.Peek();
            // subscripts are the (1 TO 10) part of the arrays
            if (context.subscripts() != null && context.subscripts().IsEmpty)
            {
                string nameArray = context.ambiguousIdentifier().GetText();
                string subscriptsArray = context.subscripts().GetText();
                InitStructures[currentType].Add(
                    $"ReDim {nameArray} ({subscriptsArray})");

                StringBuilder commas = new StringBuilder();
                Enumerable.Range(0, context.subscripts().subscript().Length - 1)
                    .ToList()
                    .ForEach(x => commas.Append(","));
                StreamRewriter.Replace(context.subscripts().Start, context.subscripts().Stop,
                    $"{ commas.ToString() }");
            }
        }

        // Add initialization of Sub for the current Structure
        public override void ExitTypeStmt([NotNull] BOSParser.TypeStmtContext context)
        {
            var currentType = TypesStack.Pop();
            if (InitStructures.ContainsKey(currentType) && InitStructures[currentType].Text.Length > 0)
            {
                StreamRewriter.InsertBefore(context.Stop, InitStructures[currentType].Text);
                StructuresWithInit.Add(currentType);
            }
            else
            {
                InitStructures.Remove(currentType);
            }
            //base.ExitTypeStmt(context);
        }

        public override void EnterSubStmt([NotNull] BOSParser.SubStmtContext context)
        {
            if (context.ambiguousIdentifier().GetText().Trim() == "Main_Run" ||
                context.ambiguousIdentifier().GetText().Trim() == "Main_Sub" ||
                context.ambiguousIdentifier().GetText().Trim() == "Main")
            {
                IsMainFile = true;
                StreamRewriter.Replace(context.ambiguousIdentifier().Start, "Main");

                // Some function of VB.Net are culture-aware,
                // this means, for instance, that when parsing a double from a
                // string it searches for the proper-culture decimal separator
                // (e.g, ',' or '.'). So, we set a culture that ensure
                // that VB.Net uses a decimal separator '.'                
                string startWatchStr = $"{ Environment.NewLine} Dim sw As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew(){Environment.NewLine}";
                string invariantCultureStr = $"{Environment.NewLine}System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture{Environment.NewLine}";

                StreamRewriter.InsertBefore(context.block().Start, startWatchStr);
                // StreamRewriter.InsertBefore(context.block().Start, invariantCultureStr);

                // Make the program wait at the end
                var waitStr = $"{Environment.NewLine}Console.WriteLine(\"Press any key to exit the program\"){Environment.NewLine}Console.ReadKey(){Environment.NewLine}";
                var stopWatchStr = $"{ Environment.NewLine}" +
                $"sw.Stop(){ Environment.NewLine}" +
                $"Console.WriteLine($\"Time elapsed {{sw.Elapsed}}\"){Environment.NewLine}";
                // InsertBefore reverses the position of the transpiled lines,
                // so at the end one has stopWatchstr and then waitStr
                StreamRewriter.InsertAfter(context.block().Stop, waitStr);
                StreamRewriter.InsertAfter(context.block().Stop, stopWatchStr);
            }
            //base.EnterSubStmt(context);
        }

        // Returns the translated text
        public string GetText() {
            // The rewriter deos not change the input, it just records them
            // and plays them out when you asks for the text
            return StreamRewriter.GetText();
        }

    }
}