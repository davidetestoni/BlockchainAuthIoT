using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Extensions
{
    public static class IJSRuntimeExtensions
    {
        public static async Task Log(this IJSRuntime js, string message)
            => await js.InvokeVoidAsync("console.log", message);

        public static async Task<bool> Confirm(this IJSRuntime js, string message)
            => await js.InvokeAsync<bool>("confirm", message);

        public static async Task AlertException(this IJSRuntime js, Exception ex)
        {
            await js.InvokeVoidAsync("console.log", ex.ToString());
            await js.InvokeVoidAsync("alert", $"ERROR: {ex.Message}");
        }

        public static async Task AlertSuccess(this IJSRuntime js, string message)
        {
            await js.InvokeVoidAsync("console.log", message);
            await js.InvokeVoidAsync("alert", message);
        }

        public static ValueTask<string> GetPrompt(this IJSRuntime js, string message)
            => js.InvokeAsync<string>("prompt", message);
    }
}
