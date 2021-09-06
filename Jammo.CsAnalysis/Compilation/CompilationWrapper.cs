using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jammo.CsAnalysis.CodeInspection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jammo.CsAnalysis.Compilation
{
    public class CompilationWrapper
    {
        private readonly List<string> rawText = new();
        private CodeInspector inspector;

        public IEnumerable<Inspection> Inspections => inspector?.Inspections;

        public CSharpCompilation Compilation { get; private set; }
        public INamespaceSymbol GlobalNamespace => Compilation?.GlobalNamespace;
        public string RawText => string.Concat(rawText);

        public void AppendFile(FileInfo file)
        {
            using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);
            
            rawText.Add(reader.ReadToEndAsync().Result);
        }
        
        public void AppendFileRange(IEnumerable<FileInfo> fileRange)
        {
            foreach (var file in fileRange)
                AppendFile(file);
        }

        public void AppendText(string text)
        {
            rawText.Add(text);
        }

        public void AppendTextRange(IEnumerable<string> strings)
        {
            foreach (var raw in strings)
                AppendText(raw);
        }

        public void ClearRaws()
        {
            rawText.Clear();
        }

        public void WithInspector(CodeInspector newInspector)
        {
            inspector = newInspector;
        }

        public void GenerateCompilation()
        {
            var trees = rawText.Select(t => CSharpSyntaxTree.ParseText(t));

            Compilation = CSharpCompilation.Create($"JAMMO_COMP_{Guid.NewGuid()}", trees.ToArray());
            inspector?.Inspect(string.Concat(rawText), this);
        }
    }
}