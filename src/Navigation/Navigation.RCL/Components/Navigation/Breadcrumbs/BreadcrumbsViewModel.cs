namespace Navigation.Components.Navigation.Breadcrumbs
{
    public record BreadcrumbsViewModel
    {
        public IEnumerable<Breadcrumb> Breadcrumbs { get; init; } = Array.Empty<Breadcrumb>();
    }
}
