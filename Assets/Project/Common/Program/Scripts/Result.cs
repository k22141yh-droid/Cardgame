#nullable enable

using System;

namespace Common.Result
{
    public readonly struct Result<T, E>
    {
        private readonly T? _value;
        private readonly E? _error;
        private readonly bool _isOk;

        private Result(T? v, E? e, bool isOk)
        {
            _value = v;
            _error = e;
            _isOk = isOk;
        }

        public static implicit operator Result<T, E>(T v) => new(v, default, true);

        public static implicit operator Result<T, E>(E e) => new(default, e, false);

        public R Match<R>(
                Func<T, R> success,
                Func<E, R> failure) =>
            _isOk ? success(_value!) : failure(_error!);
    }
}