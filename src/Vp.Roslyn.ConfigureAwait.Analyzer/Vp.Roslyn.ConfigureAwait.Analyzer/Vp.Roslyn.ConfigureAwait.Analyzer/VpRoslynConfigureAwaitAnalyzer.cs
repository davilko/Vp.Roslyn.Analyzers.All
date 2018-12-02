using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Vp.Roslyn.ConfigureAwait.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class VpRoslynConfigureAwaitAnalayzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "VpRoslynConfigureAwaitAnalayzer";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Illegal Method Calls";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeAwaitExpression, SyntaxKind.AwaitExpression);
        }

        private void AnalyzeAwaitExpression(SyntaxNodeAnalysisContext syntaxNodeContext)
        {
            var result = syntaxNodeContext.Node as AwaitExpressionSyntax;
            if (result == null) return;

            var awaitExpression = result.Expression;
            var semanticModel = syntaxNodeContext.SemanticModel;

            var methodSymbol = semanticModel.GetSymbolInfo(awaitExpression, syntaxNodeContext.CancellationToken).Symbol as IMethodSymbol;
            if (methodSymbol == null || methodSymbol.ReturnType.Name != "Task") return;

            syntaxNodeContext.ReportDiagnostic(Diagnostic.Create(Rule, awaitExpression.GetLocation()));
        }
    }
}
