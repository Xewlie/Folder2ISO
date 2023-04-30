namespace Export;

public class FinishEventArgs : EventArgs
{
    public FinishEventArgs(string message)
    {
        Message = message;
    }

    public string Message { get; }
}