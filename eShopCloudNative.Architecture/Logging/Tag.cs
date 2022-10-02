namespace eShopCloudNative.Architecture.Logging;

public class Tag
{
    public string Key { get; private set; }
    public TagType Type { get; private set; }
    public object Value { get; private set; }

    public Tag() { }

    public Tag(string key, TagType tagType = TagType.None, object value = default)
    {
        this.Key = key;
        this.Type = tagType; 
        this.Value = value;
    }
}

public enum TagType
{ 
    None,
    Argument,
    Property
}
