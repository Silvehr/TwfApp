using System;
using System.Collections.Generic;
using System.Text;

namespace TwfApp.Types.Results
{
    public readonly struct Result<TResult, TError>
    {
        public TResult? Value => IsSuccess ? (TResult?)_value : default;
        public TError? Error => IsSuccess ? default : (TError?)_value;
        public bool IsSuccess { get; }
        
        private readonly object? _value;

        public Result(TResult value)
        {
            IsSuccess = true;
            _value = value;
        }

        public Result(TError error)
        {
            IsSuccess = false;
            _value = error;
        }
    }
}
