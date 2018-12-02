using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Vp.Roslyn.ConfigureAwait.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(VpRoslynConfigureAwaitAnalayzerCodeFixProvider)), Shared]
    public class VpRoslynConfigureAwaitAnalayzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Add ConfigureAwait(false)";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(VpRoslynConfigureAwaitAnalayzerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => AddConfigureAwait(context.Document, diagnosticSpan, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> AddConfigureAwait(Document document, TextSpan textSpan, CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync();
            var addText = ".ConfigureAwait(false)";
            var replaceText = text.GetSubText(textSpan).ToString() + addText;
            var newText = text.Replace(textSpan, replaceText);
            return document.WithText(newText);
        }
    }
}
