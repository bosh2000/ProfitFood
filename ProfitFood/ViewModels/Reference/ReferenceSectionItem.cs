using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.UI.Views.Reference
{
    public sealed class ReferenceSectionItem
    {
        public string Title { get; }
        public string Key { get; }

        public ReferenceSectionItem(string title, string key)
        {
            Title = title;
            Key = key;
        }
    }
}