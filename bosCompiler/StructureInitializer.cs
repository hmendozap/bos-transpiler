using System;
using System.Text;

namespace BosTranspiler
{
    public class StructureInitializer
    {
        private string TypeName { get; set; }
        private StringBuilder Body { get; set; } = new StringBuilder();
        public string Text {
            get {
                if (Body.Length == 0)
                    return String.Empty;
                else
                    return $"{Body.ToString()}End Sub{Environment.NewLine}";
            }
        }

        public StructureInitializer(string typeName)
        {
            this.TypeName = typeName;
        }

        public void Add(string text)
        {
            if (Body.Length == 0)
            {
                Body.Append($"Sub Init{TypeName}(){Environment.NewLine}");
            }

            Body.Append($"{text}{Environment.NewLine}");
        }
    }
}