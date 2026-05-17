namespace Anamnesis.Adapter.Llm.Contract;

public class LlmUnavailableException : Exception
{
    public LlmUnavailableException(string message, Exception innerException)
        : base(message, innerException) { }
}
