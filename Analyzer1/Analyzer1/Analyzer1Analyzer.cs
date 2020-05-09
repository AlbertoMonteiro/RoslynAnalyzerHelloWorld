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
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            var identifierText = classDeclaration.Identifier.Text;
            if (identifierText == "NotValidatableAttribute")
                return;

            var @namespace = classDeclaration.Ancestors()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault();

            if (@namespace?.Name is IdentifierNameSyntax identifier && identifier.Identifier.Text != "ConsoleApplication1")
                return;

            var isNotValidatable = classDeclaration.AttributeLists
                .SelectMany(x => x.Attributes)
                .Select(x => x.Name)
                .OfType<IdentifierNameSyntax>()
                .Any(x => x.Identifier.Text == "NotValidatable");

            var baseTypes = classDeclaration.BaseList?.Types;
            var simpleBaseTypeSyntaxes = baseTypes?.OfType<SimpleBaseTypeSyntax>().ToArray();
            var implemntsIValidatable = simpleBaseTypeSyntaxes
                                            ?.Select(x => x.Type)
                                            .OfType<IdentifierNameSyntax>()
                                            .Any(x => x.Identifier.Text == "IValidatable") == true;

            if (!implemntsIValidatable && !isNotValidatable)
            {
                var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), identifierText);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
