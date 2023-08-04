namespace CustomAuthenticationApp.Abstractions
{
    public interface IClipboardService
    {
        ValueTask CopyAsync(string? text);
    }
}