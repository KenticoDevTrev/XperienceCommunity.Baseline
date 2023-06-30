namespace Core.Services
{
    public interface IStringToEnumParser
    {
        IconSource StringToIconSource(string value);
        LinkTargetType StringToLinkTargetType(string value);
        TextAlignment StringToTextAlignment(string value);
        VisualItemType StringToVisualItemType(string value);
    }
}