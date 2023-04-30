namespace Export;

public class ProgressEventArgs : EventArgs
{
    public int Current { get; } = -1;
    public int Maximum { get; } = -1;
    public string? Action { get; }
    
    public ProgressEventArgs(int current)
    {
        Current = current;
    }

    public ProgressEventArgs(int current, int maximum)
    {
        Current = current;
        Maximum = maximum;
    }

    public ProgressEventArgs(string? action, int current, int maximum)
    {
        Action = action;
        Current = current;
        Maximum = maximum;
    }
}