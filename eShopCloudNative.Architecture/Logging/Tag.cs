namespace eShopCloudNative.Architecture.Logging;

public class Tag
{
    public string Key { get; set; }

    public object Value { get; set; }

    public Tag() { }

    public Tag(string key, object value)
    {
        this.Key = key;
        this.Value = value;
    }
}
