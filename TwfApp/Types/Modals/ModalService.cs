using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace TwfApp.Types.Modals
{
    public class ModalService
    {
        public event Action? ModalHostInitialized;
        public event Func<ModalRequest, Task>? ShowRequested;

        public async Task RaiseModelHostInitialized()
        {
            ModalHostInitialized?.Invoke();
        }

        public async Task<TResult?> ShowAsync<TResult>(ModalRequest request)
        {
            request.Completion = new TaskCompletionSource<object?>();

            if (ShowRequested != null)
                await ShowRequested.Invoke(request);

            var result = await request.Completion.Task;

            return (TResult?)result;
        }

        public Task ShowMessageAsync(string title, string message)
        {
            return ShowAsync<bool>(
                new ModalRequest
                {
                    Title = title,
                    Message = message,
                    Buttons =
                    {
                    ModalButton.Ok()
                    }
                });
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            var result = await ShowAsync<bool>(
                new ModalRequest
                {
                    Title = title,
                    Message = message,
                    Buttons =
                    {
                        ModalButton.Yes(),
                        ModalButton.No()
                    }
                });

            return result;
        }

        public async Task ShowForceExit(string title, string message)
        {
            await ShowAsync<bool>(
                new ModalRequest
                {
                    Title = title,
                    Message = message
                });
        }
    }
}
