using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using System.Text.RegularExpressions;

namespace Vp.Roslyn.DateTimeNow.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(VpRoslynDateTimeNowAnalayzerCodeFixProvider)), Shared]
    public class VpRoslynDateTimeNowAnalayzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Change to DateTime.UtcNow";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(VpRoslynDateTimeNowAnalayzerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => ReplaceWithUtcNowAsync(context.Document, diagnosticSpan, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> ReplaceWithUtcNowAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync();
            var repl = "DateTime.UtcNow";
            if (Regex.Replace(text.GetSubText(span).ToString(), @"\s+", string.Empty) == "System.DateTime.Now")
                repl = "System.DateTime.UtcNow";
            var newtext = text.Replace(span, repl);
            return document.WithText(newtext);
        }
    }
}
