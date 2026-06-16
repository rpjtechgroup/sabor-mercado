namespace SaborMercado.Web.Features.Support;

public sealed class SupportDialogState
{
    public event Action? Changed;

    public bool IsOpen { get; private set; }

    public void Open()
    {
        IsOpen = true;
        Changed?.Invoke();
    }

    public void Close()
    {
        IsOpen = false;
        Changed?.Invoke();
    }
}
