using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProfitFood.Model.Infrastructure
{
    public class OperationResult
    {
        protected OperationResult() => Errors = Array.Empty<Error>();
        protected OperationResult(IReadOnlyCollection<Error> errors) => Errors = errors;

        public bool IsSuccess => !Errors.Any();
        public IReadOnlyCollection<Error> Errors { get; }

        public static OperationResult Success() => new();
        public static OperationResult Failure(Error error) => new(new[] { error });
        public static OperationResult Failure(IEnumerable<Error> errors) => new(errors.ToArray());
    }
    public sealed class OperationResult<T> : OperationResult
    {
        private OperationResult(T value) : base() => Value = value;
        private OperationResult(IReadOnlyCollection<Error> errors) : base(errors) { }

        public T? Value { get; }

        public static OperationResult<T> Success(T value) => new(value);
        public new static OperationResult<T> Failure(Error error) => new(new[] { error });
        public new static OperationResult<T> Failure(IEnumerable<Error> errors) => new(errors.ToArray());
    }
    public record Error(string Code, string Message, ErrorType Type = ErrorType.Validation);

    public enum ErrorType
    {
        Validation,
        Domain,
        Infrastructure,
        Authorization
    }
}
