using Dawn;

namespace eShopCloudNative.Architecture.Logging;

public class ExceptionTag : Tag
{
    public ExceptionTag(Exception value)
        : base("Exception", TagType.Exception, value)
    {
        Guard.Argument(value).NotNull();
        this.Value = value;
    }

    public void UpdateException(Exception value)
    {
        Guard.Argument(value).NotNull();
        this.Value = value;
    }
}
