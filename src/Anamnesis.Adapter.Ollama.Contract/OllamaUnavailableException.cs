namespace Anamnesis.Adapter.Ollama.Contract;

public class OllamaUnavailableException : Exception
{
    public OllamaUnavailableException(string message, Exception innerException)
        : base(message, innerException) { }
}
