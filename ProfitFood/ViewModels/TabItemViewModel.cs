using ProfitFood.UI.Infrastructure.Commands;
using ProfitFood.UI.ViewModels.Base;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels
{
    public class TabItemViewModel : ViewModel
    {
        public string Id { get; set; }
        public string Header { get; }
        public object Content { get; }
        public ICommand CloseTabCommand { get; }

        public TabItemViewModel(string id, string header, object content, Action<TabItemViewModel> closeCommand)
        {
            this.Id = id;
            this.Header = header;
            this.Content = content;
            this.CloseTabCommand = new LambdaCommand(
                Execute: _ => closeCommand(this),
                CanExecute: _ => true
                );
        }
    }
}