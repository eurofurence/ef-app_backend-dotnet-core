console.log("TinyMarkdownEditor.razor.js loaded successfully!")

var TinyMarkdownEditorInterop = {
    _instances: {},

    initialize: function (dotNetObjectReference, elementId, value) {
        const editor = new TinyMDE.Editor({ textarea: elementId });
        const commandbar = new TinyMDE.CommandBar({
            element: elementId + '-commandbar', editor: editor, commands: [
                'bold',
                'italic',
                'strikethrough',
                '|',
                'h1',
                'h2',
                'ul',
                'ol',
                '|',
                {
                    name: 'markdownHelp',
                    title: 'Basic Markdown Syntax',
                    innerHTML: '<b>?</b>',
                    action: editor => window.open('https://www.markdownguide.org/basic-syntax/', '_blank')
                }
            ]
        });
        this._instances[elementId] = {
            editor: editor,
            commandbar: commandbar,
        };

        editor.addEventListener('change', e => {
            dotNetObjectReference.invokeMethodAsync("OnValueChanged", e.content);
        });
        if (value) {
            editor.setContent(value);
        }
    },

    destroy: function (elementId) {
        const tinyMdeInstances = this._instances || {};
        delete tinyMdeInstances[elementId]
    },

    getValue: function (elementId) {
        return this._instances[elementId]?.editor.getContent();
    },

    setValue: function (elementId, value) {
        this._instances[elementId]?.editor.setContent(value);
    },
};