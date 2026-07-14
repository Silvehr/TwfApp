using System;
using System.Collections.Generic;
using System.Text;

namespace TwfApp.Types.Modals
{
    public class ModalButton
    {
        public string Text { get; init; } = "";

        public object? Result { get; init; }

        public string CssClass { get; init; } = "btn btn-primary";

        public static ModalButton Ok() =>
            new()
            {
                Text = "OK",
                Result = true
            };

        public static ModalButton Yes() =>
            new()
            {
                Text = "Tak",
                Result = true
            };

        public static ModalButton No() =>
            new()
            {
                Text = "Nie",
                Result = false
            };
    }
}
