using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Analyzer1
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Analyzer1Analyzer : DiagnosticAnalyzer
    {
        internal const string Title = "Missing IValidatable";
        internal const string MessageFormat = "The class '{0}' is missing the IValidatable interface";
        internal const string Category = "CodeRules";
        const string Description = "The string passed as the 'paramName' argument of ArgumentException constructor "
                                   + "must be the name of one of the method arguments.\r\n"
                                   + "It can be either specified directly or using the nameof() operator (C#6 only)";
        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId.MissingIValidatable.ToDiagnosticId(),
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
            => context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

            var separatedSyntaxList = classDeclarationSyntax.BaseList?.Types;
            var any = separatedSyntaxList?.OfType<IdentifierNameSyntax>().Any(x => x.Identifier.Text == "IValidatable") == true;
            if (!any)
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(Rule, classDeclarationSyntax.Identifier.GetLocation(), classDeclarationSyntax.Identifier.Text);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
