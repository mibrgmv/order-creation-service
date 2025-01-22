namespace OrderCreationService.Infrastructure.Persistence;

public sealed class PersistenceOptions
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public string Database { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string ConnectionString => $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password}";
}