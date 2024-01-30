using eShopCloudNative.Architecture.Bootstrap.RabbitMQ;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap.RabbitMQ;

public interface IRabbitMQAdminApi: IDisposable
{
    // vhosts

    [Get("/api/vhosts")]
    Task<IEnumerable<VirtualHost>> GetVirtualHostsAsync();

    [Get("/api/vhosts/{vhost}")]
    Task<VirtualHost> GetVirtualHostAsync(string vhost);

    [Put("/api/vhosts/{vhost}")]
    Task CreateVirtualHostAsync(string vhost);

    // Users

    [Get("/api/users")]
    Task<IEnumerable<VirtualHost>> GetUsersAsync();

    [Get("/api/users/{userName}")]
    Task<VirtualHost> GetUserAsync(string userName);

    [Put("/api/users/{userName}")]
    Task CreateUserAsync(string userName, [Body] CreateUserRequest createUserRequest);


    [Get("/api/permissions/{vhost}/{userName}")]
    Task<UserVhostPermission> GetUserVirtualHostPermissionsAsync(string vhost, string userName);

    [Put("/api/permissions/{vhost}/{userName}")]
    Task SetVhostPermissionsAsync(string vhost, string userName, [Body] VhostPermission permission);

    [Put("/api/topic-permissions/{vhost}/{userName}")]
    Task SetTopicPermissionsAsync(string vhost, string userName, [Body] TopicPermission permission);

}





public class Metadata
{

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("tags")]
    public IList<string> Tags { get; set; }
}

public class VirtualHost
{
    
    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("metadata")]
    public Metadata Metadata { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("tags")]
    public IList<string> Tags { get; set; }

    [JsonProperty("tracing")]
    public bool Tracing { get; set; }
}


public class User
{

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("password_hash")]
    public string PasswordHash { get; set; }

    [JsonProperty("hashing_algorithm")]
    public string HashingAlgorithm { get; set; }

    [JsonProperty("tags")]
    public IList<string> Tags { get; set; }


}

public class CreateUserRequest
{

    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("tags")]
    public string Tags { get; set; }

}

public class UserVhostPermission : VhostPermission
{

    [JsonProperty("user")]
    public string User { get; set; }

    [JsonProperty("vhost")]
    public string Vhost { get; set; }
}


public class VhostPermission
{
    [JsonProperty("configure")]
    public string Configure { get; set; }

    [JsonProperty("write")]
    public string Write { get; set; }

    [JsonProperty("read")]
    public string Read { get; set; }

    public VhostPermission ConfitgureAll() { this.Configure = ".*"; return this; }
    public VhostPermission WriteAll() { this.Write = ".*"; return this; }
    public VhostPermission ReadAll() { this.Read = ".*"; return this; }
    public VhostPermission FullAccess() => this.ConfitgureAll().WriteAll().ReadAll();
}

public class TopicPermission
{

    [JsonProperty("exchange")]
    public string Exchange { get; set; } = string.Empty;

    [JsonProperty("write")]
    public string Write { get; set; }

    [JsonProperty("read")]
    public string Read { get; set; }


    public TopicPermission SetExchange(string exchange) { this.Exchange = exchange; return this; }

    public TopicPermission WriteAll() { this.Write = ".*"; return this; }
    public TopicPermission ReadAll() { this.Read = ".*"; return this; }
    public TopicPermission FullAccess() => this.WriteAll().ReadAll();
}