using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace eShopCloudNative.Architecture.Tests;

public class FakeIConfigurationSection : IConfigurationSection
{
    public string this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public string Key { get; set; }

    public string Path { get; set; }

    public string Value { get; set; }

    public List<IConfigurationSection> FakeChildren { get; set; }

    public IEnumerable<IConfigurationSection> GetChildren() => this.FakeChildren;

    public IChangeToken GetReloadToken() => throw new NotImplementedException();

    public IConfigurationSection GetSection(string key) => this.FakeChildren?.Where(it => it.Key == key).SingleOrDefault() ?? null;
}