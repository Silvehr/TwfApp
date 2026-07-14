using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Components;


namespace TwfApp.Types.Modals
{
    
    public class ModalRequest
    {
        public string Title { get; init; } = "";

        public string Message { get; init; } = "";

        public RenderFragment? Content { get; init; }

        public List<ModalButton> Buttons { get; } = [];

        internal TaskCompletionSource<object?>? Completion;
    }
}
