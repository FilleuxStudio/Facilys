namespace Facilys.Components.Services
{
    public class PageTitleService
    {
        private string _currentTitle = string.Empty;

        public string CurrentTitle
        {
            get => _currentTitle;
            set
            {
                if (_currentTitle != value)
                {
                    _currentTitle = value;
                    NotifyTitleChanged();
                }
            }
        }

        public event Action? TitleChanged;

        private void NotifyTitleChanged()
        {
            TitleChanged?.Invoke();
        }
    }
}
