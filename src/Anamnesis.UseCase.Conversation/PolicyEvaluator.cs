using System.Text.RegularExpressions;

namespace Anamnesis.UseCase.Conversation;

internal enum PolicyDecision
{
    Allow,
    Deny
}

internal static class PolicyEvaluator
{
    private static readonly Regex[] _denyPatterns =
    [
        new(@"ignore\s+(?:all\s+)?(?:previous|prior|above)\s+instructions?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"you\s+are\s+now\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"disregard\s+(?:your\s+)?(?:system\s+prompt|instructions?|previous|all)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"forget\s+(?:all\s+)?(?:previous|prior|above|your)\s+instructions?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"pretend\s+(?:you\s+are|to\s+be)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled),
    ];

    public static PolicyDecision Evaluate(string message)
    {
        try
        {
            foreach (var pattern in _denyPatterns)
            {
                if (pattern.IsMatch(message))
                    return PolicyDecision.Deny;
            }

            return PolicyDecision.Allow;
        }
        catch
        {
            return PolicyDecision.Deny;
        }
    }
}
