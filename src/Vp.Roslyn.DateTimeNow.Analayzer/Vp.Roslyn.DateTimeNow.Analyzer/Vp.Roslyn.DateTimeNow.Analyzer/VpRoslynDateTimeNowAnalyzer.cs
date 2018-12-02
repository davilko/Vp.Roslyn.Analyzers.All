using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Vp.Roslyn.DateTimeNow.Analyzer;

namespace Vp.Roslyn.DateTimeNow.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class VpRoslynDateTimeNowAnalayzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "VpRoslynDateTimeNowAnalayzer";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

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
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var dateTimeType = compilationContext.Compilation.GetTypeByMetadataName("System.DateTime");
                compilationContext.RegisterSyntaxNodeAction((analysisContext) =>
                {
                    var dateTimeNode = analysisContext.Node as MemberAccessExpressionSyntax;
                    if (dateTimeNode == null) return;
                    var typeInfo = analysisContext.SemanticModel.GetTypeInfo(dateTimeNode.Expression, analysisContext.CancellationToken).Type as INamedTypeSymbol;
                    if (typeInfo == null) return;
                    if (typeInfo?.ConstructedFrom == null)
                        return;

                    if (!typeInfo.ConstructedFrom.Equals(dateTimeType))
                        return;

                    if (dateTimeNode.Name.ToString() == "Now")
                    {
                        analysisContext.ReportDiagnostic(Diagnostic.Create(Rule, dateTimeNode.GetLocation()));
                    }
                }, SyntaxKind.SimpleMemberAccessExpression);
            });
        }
    }
}
