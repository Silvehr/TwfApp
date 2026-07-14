using System;
using System.Collections.Generic;
using System.Text;

namespace TwfApp.Types.Results
{
    #if DEBUG
    public struct Result<TResult, TError>
    {
        public TResult Value => !_readBeforeAccess ? throw new InvalidResultAccess() : field;

        public TError Error => !_readBeforeAccess ? throw new InvalidResultAccess() : field;

        public bool IsSuccess
        {
            get
            {
                _readBeforeAccess = true;
                return field;
            }
        }

        private bool _readBeforeAccess = false;

        public Result(TResult value)
        {
            IsSuccess = true;
            Value = value;
            Error = default!;
        }

        public Result(TError error)
        {
            IsSuccess = false;
            Error = error;
            Value = default!;
        }
    }
    #else
    public readonly struct Result<TResult, TError>
    {
        public TResult Value { get; }
        public TError Error { get; }
        public bool IsSuccess { get; }

        public Result(TResult value)
        {
            IsSuccess = true;
            Value = value;
            Error = default!;
        }

        public Result(TError error)
        {
            IsSuccess = false;
            Error = error;
            Value = default!;
        }
    }
    #endif
}
