using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Model.Infrastructure
{
    internal class Responce<T>
    {
        private readonly ILogger _logger;

        public Responce(ILogger logger)
        {
            this._logger = logger;
        }

        public bool IsSuccess { get; set; } = true;
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }

        public void Error(string? errorCode, string? errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            _logger.LogError(errorCode, errorMessage);
        }
    }
}