namespace ProfitFood.UI.ViewModels.DashBoardViewModels
{
    /// <summary>
    /// Строка меню на сегодня.
    /// </summary>
    public sealed class DashboardMenuItemViewModel
    {
        public DashboardMenuItemViewModel(string mealTypeName, string dishName, string recipeCardNumber, string outputText)
        {
            MealTypeName = mealTypeName;
            DishName = dishName;
            RecipeCardNumber = recipeCardNumber;
            OutputText = outputText;
        }

        public string MealTypeName { get; }
        public string DishName { get; }
        public string RecipeCardNumber { get; }
        public string OutputText { get; }
    }
}