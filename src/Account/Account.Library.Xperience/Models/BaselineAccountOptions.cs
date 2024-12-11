namespace Account.Models
{
    /// <summary>
    /// Account Options for installation
    /// </summary>
    /// <param name="UseAccountWebpageType">If true, the Generic.Account Webpage type will be installed and added to your web channels.  You still need to add at least 1 RegisterPageTemplate Assembly tag (See "AssemblyTags.md" in documentation)</param>
    public record BaselineAccountOptions(bool UseAccountWebpageType);
}
