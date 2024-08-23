using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Eurofurence.App.Backoffice.Components
{

    public partial class TinyMarkdownEditor : IDisposable
    {
        private DotNetObjectReference<TinyMarkdownEditor>? _dotNetObjectRef;

        protected TinyMarkdownEditorInterop? jsTinyMdeInterop { get; private set; }

        private string _elementId { get; } = $"tinymde-{Guid.NewGuid()}";

        [Parameter]
        public string? Title { get; set; }

        [Parameter]
        public EventCallback<string> ValueChanged { get; set; }

        [Parameter]
        public string Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;

                ValueChanged.InvokeAsync(value);
            }
        }
        private string _value = "";

        protected bool Initialized { get; set; }

        public async Task SetValueAsync(string value)
        {
            if (!Initialized)
                return;

            await (jsTinyMdeInterop?.SetValueAsync(_elementId, value) ?? ValueTask.CompletedTask);
        }

        public async ValueTask<string> GetValueAsync()
        {
            if (!Initialized)
                return "";

            return await (jsTinyMdeInterop?.GetValueAsync(_elementId) ?? ValueTask.FromResult(""));
        }

        [JSInvokable]
        public async Task OnValueChanged(string value)
        {
            await ValueChanged.InvokeAsync(value);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                jsTinyMdeInterop ??= new TinyMarkdownEditorInterop(JS);
                _dotNetObjectRef ??= DotNetObjectReference.Create(this);

                await jsTinyMdeInterop.InitializeAsync(_dotNetObjectRef, _elementId, Value);

                Initialized = true;
            }

        }
        protected override void OnInitialized()
        {
            jsTinyMdeInterop ??= new TinyMarkdownEditorInterop(JS);

            base.OnInitialized();
        }

        public void Dispose()
        {
            jsTinyMdeInterop?.DestroyAsync(_elementId);
        }

    }


    public class TinyMarkdownEditorInterop
    {
        private IJSRuntime _jsRuntime;

        public TinyMarkdownEditorInterop(IJSRuntime _jsRuntime)
        {
            this._jsRuntime = _jsRuntime;
        }

        public async ValueTask InitializeAsync(DotNetObjectReference<TinyMarkdownEditor> dotNetObjectRef, string elementId, string value)
        {
            await _jsRuntime.InvokeVoidAsync("TinyMarkdownEditorInterop.initialize", dotNetObjectRef, elementId, value);
        }

        public async ValueTask DestroyAsync(string elementId)
        {
            await _jsRuntime.InvokeVoidAsync("TinyMarkdownEditorInterop.destroy", elementId);
        }

        public async ValueTask<string> GetValueAsync(string elementId)
        {
            return await _jsRuntime.InvokeAsync<string>("TinyMarkdownEditorInterop.getValue", elementId);
        }
        public async ValueTask SetValueAsync(string elementId, string value)
        {
            await _jsRuntime.InvokeVoidAsync("TinyMarkdownEditorInterop.setValue", elementId, value);
        }
    }
}
