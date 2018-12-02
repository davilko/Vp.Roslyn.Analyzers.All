using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Vp.Roslyn.ConfigureAwait.Analyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        [TestMethod]
        public void Should_Fix_AsyncMethod_Without_ConfigureAwait()
        {
            var test = @"
                using System;
                using System.Net.Http;
                using System.Threading.Tasks;

                namespace Test.Sample
                {
                    class Sample
                    {
                        public static async Task DoAsync()
                        {
                            await Task.Delay(1);
                        }
                    }
                }";
            var expected = new DiagnosticResult
            {
                Id = "VpRoslynConfigureAwaitAnalayzer",
                Message = "Not recommended for use async code without ConfigureAwait(false)",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 12, 35)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
                using System;
                using System.Net.Http;
                using System.Threading.Tasks;

                namespace Test.Sample
                {
                    class Sample
                    {
                        public static async Task DoAsync()
                        {
                            await Task.Delay(1).ConfigureAwait(false);
                        }
                    }
                }";

            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new VpRoslynConfigureAwaitAnalayzerCodeFixProvider();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new VpRoslynConfigureAwaitAnalayzerAnalyzer();

    }
}
