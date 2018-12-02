using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using Vp.Roslyn.DateTimeNow.Analyzer;

namespace Vp.Roslyn.DateTimeNow.Analyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Should_Replace_Not_To_UtcNow()
        {
             var test = @"
                using System;
                using System.Net.Http;
                using System.Threading.Tasks;

                namespace Test.Sample
                {
                    class Sample
                    {
                        public static DateTime ReturnDate()
                        {
                            return DateTime.Now;
                        }
                    }
                }";
            var expected = new DiagnosticResult
            {
                Id = "VpRoslynDateTimeNowAnalayzer",
                Message = "DateTime used Now",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 12, 36)
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
                        public static DateTime ReturnDate()
                        {
                            return DateTime.UtcNow;
                        }
                    }
                }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new VpRoslynDateTimeNowAnalayzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new VpRoslynDateTimeNowAnalayzerAnalyzer();
        }
    }
}
