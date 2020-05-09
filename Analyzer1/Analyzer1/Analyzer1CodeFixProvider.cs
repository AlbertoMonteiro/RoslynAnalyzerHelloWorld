using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Analyzer1
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Analyzer1CodeFixProvider)), Shared]
    public class Analyzer1CodeFixProvider : CodeFixProvider
    {
        private const string title = "Make uppercase";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Analyzer1Analyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(title, c => MakeUppercaseAsync(context.Document, declaration, c), title),
                diagnostic);
        }

        private async Task<Document> MakeUppercaseAsync(Document document, ClassDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            // Compute new uppercase name.
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var identifierName = SyntaxFactory.IdentifierName("IValidatable")
                .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                .WithTrailingTrivia(SyntaxFactory.LineFeed);
            var simpleBaseTypeSyntax = SyntaxFactory.SimpleBaseType(identifierName);
            var newClass = typeDecl
                .ReplaceToken(typeDecl.Identifier, typeDecl.Identifier.WithTrailingTrivia(SyntaxFactory.Whitespace(" ")))
                .AddBaseListTypes(simpleBaseTypeSyntax);

            var newRoot = root.ReplaceNode(typeDecl, newClass);

            // Return the new solution with the now-uppercase type name.
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}
