using Dawn;

namespace eShopCloudNative.Architecture.Logging;

public class Tag
{
    public string Key { get; private set; }
    public TagType Type { get; private set; }
    public object Value { get; protected set; }

    public Tag() { }

    public Tag(string key, TagType tagType = TagType.None, object value = default)
    {
        Guard.Argument(key).NotNull().NotEmpty().NotWhiteSpace();
        this.Key = key;
        this.Type = tagType; 
        this.Value = value;
    }
}
