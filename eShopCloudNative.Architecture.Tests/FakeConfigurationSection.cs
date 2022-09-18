using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace eShopCloudNative.Architecture.Tests;

public class FakeConfigurationSection : IConfigurationSection
{
    public string this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public string Key { get; set; }

    public string Path { get; set; }

    public string Value { get; set; }

    public List<FakeConfigurationSection> FakeChildren { get; set; }

    public IEnumerable<IConfigurationSection> GetChildren() => this.FakeChildren;

    public IChangeToken GetReloadToken() => throw new NotImplementedException();

    public IConfigurationSection GetSection(string key) => this.FakeChildren?.Where(it => it.Key == key).SingleOrDefault() ?? null;


    public FakeConfigurationSection SetKeyValue(string key, string value) => this.SetKey(key).SetValue(value);


    public FakeConfigurationSection SetKey(string key) { this.Key = key; return this; }

    public FakeConfigurationSection SetPath(string path) { this.Path = path; return this; }

    public FakeConfigurationSection SetValue(string value) { this.Value = value; return this; }

    public FakeConfigurationSection AddChild(Action<FakeConfigurationSection> configure)
    {
        this.FakeChildren ??= new List<FakeConfigurationSection>();
        var configuration = new FakeConfigurationSection();
        this.FakeChildren.Add(configuration);
        configure?.Invoke(configuration);
        return this;
    }

}
