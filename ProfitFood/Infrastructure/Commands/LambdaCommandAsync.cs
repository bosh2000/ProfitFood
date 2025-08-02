using ProfitFood.UI.Infrastructure.Commands.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.UI.Infrastructure.Commands
{
    public class LambdaCommandAsync : Command
    {
        private readonly Func<object, Task> _execute;
        private readonly Func<object, bool> _canExecute;

        public LambdaCommandAsync(Func<object, Task> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public override bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public override async void Execute(object parameter)
        {
            try
            {
                await _execute(parameter);
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                Debug.WriteLine(ex);
            }
        }
    }
}