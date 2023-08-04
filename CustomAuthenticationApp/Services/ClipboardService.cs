using CustomAuthenticationApp.Abstractions;
using Microsoft.JSInterop;

namespace CustomAuthenticationApp.Services
{
    /// <see href="https://code-maze.com/copy-to-clipboard-in-blazor-webassembly/"/>
    public class ClipboardService : IClipboardService
    {
        internal static readonly string Copy = "navigator.clipboard.writeText";

        private readonly IJSRuntime _jsInterop;

        public ClipboardService(IJSRuntime jsInterop) =>
            _jsInterop = jsInterop;

        public ValueTask CopyAsync(string? text) =>
            _jsInterop.InvokeVoidAsync(Copy, text);
    }
}
