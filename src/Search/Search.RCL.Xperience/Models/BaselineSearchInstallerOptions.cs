namespace Search.Models
{
    public class BaselineSearchInstallerOptions(bool addSearchPageType = true)
    {
        public bool AddSearchPageType { get; set; } = addSearchPageType;
    }
}
