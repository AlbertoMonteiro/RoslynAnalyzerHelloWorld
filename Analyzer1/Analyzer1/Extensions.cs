namespace Analyzer1
{
    public static class Extensions
    {
        public static string ToDiagnosticId(this DiagnosticId diagnosticId)
            => $"MA{(int)diagnosticId:D4}";
    }
}