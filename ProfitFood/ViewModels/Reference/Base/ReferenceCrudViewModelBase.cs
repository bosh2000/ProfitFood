using ProfitFood.UI.ViewModels;
using ProfitFood.UI.ViewModels.Reference;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProfitFood.UI.Views.Reference.Base
{
    public abstract class ReferenceCrudViewModelBase<TListItem, TEditModel> : ViewModelBase, IInitializableViewModel
        where TListItem : class
        where TEditModel : class, new()
    {
        private TListItem? _selectedItem;
        private TEditModel _editModel = new();
        private string _searchText = string.Empty;
        private string _title = string.Empty;
        private string _statusMessage = "Готово";
        private bool _isBusy;
        private bool _isEditMode;
        private bool _isCreateMode;

        protected ReferenceCrudViewModelBase()
        {
            Items = new ObservableCollection<TListItem>();

            CreateCommand = new RelayCommand(_ => Create());
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedItem is not null && !IsBusy);
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
            CancelCommand = new RelayCommand(_ => Cancel());
            RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
            SearchCommand = new RelayCommand(async _ => await SearchAsync(), _ => !IsBusy);
        }

        public virtual async Task IInitializeAsync()
        {
            await LoadAsync();
        }

        public ObservableCollection<TListItem> Items { get; }

        public TListItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    if (value is not null)
                    {
                        EditModel = BuildEditModel(value);
                        IsCreateMode = false;
                        IsEditMode = true;
                        StatusMessage = "Запись выбрана для редактирования.";
                        UpdateCommandState();
                    }
                }
            }
        }

        public TEditModel EditModel
        {
            get => _editModel;
            set => SetProperty(ref _editModel, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public string Title
        {
            get => _title;
            protected set => SetProperty(ref _title, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            protected set => SetProperty(ref _statusMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            protected set
            {
                SetProperty(ref _isBusy, value);
                UpdateCommandState();
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            protected set => SetProperty(ref _isEditMode, value);
        }

        public bool IsCreateMode
        {
            get => _isCreateMode;
            protected set => SetProperty(ref _isCreateMode, value);
        }

        public ICommand CreateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }

        public virtual async Task InitializeAsync()
        {
            await LoadAsync();
        }

        public abstract Task LoadAsync();

        protected abstract Task SearchAsync();

        protected abstract TEditModel CreateNewEditModel();

        protected abstract TEditModel BuildEditModel(TListItem item);

        protected abstract Task SaveCoreAsync(TEditModel model);

        protected abstract Task DeleteCoreAsync(TListItem item);

        protected virtual void Create()
        {
            EditModel = CreateNewEditModel();
            IsCreateMode = true;
            IsEditMode = true;
            StatusMessage = "Создание новой записи.";
        }

        protected virtual async Task SaveAsync()
        {
            try
            {
                IsBusy = true;
                await SaveCoreAsync(EditModel);
                await LoadAsync();

                IsEditMode = false;
                IsCreateMode = false;
                StatusMessage = "Изменения сохранены.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка сохранения: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected virtual async Task DeleteAsync()
        {
            if (SelectedItem is null)
                return;

            try
            {
                IsBusy = true;
                await DeleteCoreAsync(SelectedItem);
                await LoadAsync();

                EditModel = CreateNewEditModel();
                IsEditMode = false;
                IsCreateMode = false;
                StatusMessage = "Запись удалена.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка удаления: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected virtual void Cancel()
        {
            EditModel = CreateNewEditModel();
            IsEditMode = false;
            IsCreateMode = false;
            StatusMessage = "Редактирование отменено.";
        }

        protected void ReplaceItems(IEnumerable<TListItem> items)
        {
            Items.Clear();

            foreach (var item in items)
                Items.Add(item);
        }

        protected void UpdateCommandState()
        {
            (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}