using System;

namespace BosTranspiler
{
    internal class StructureInitializer
    {
        private string typeName;

        public StructureInitializer(string typeName)
        {
            this.typeName = typeName;
        }

        public string Text { get; set; }

        internal void Add(string text)
        {
            if (Body.Length == 0)
            {
                Body.Append($"Sub Init{Name}(){Environment.NewLine}");
            }

            Body.Append($"{text}{Environment.NewLine}");

            throw new NotImplementedException();
        }
    }
}