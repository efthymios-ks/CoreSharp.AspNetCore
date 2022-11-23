using System;

namespace Server.Entities;

public sealed class Dummy
{
    // Properties
    public Guid Id { get; set; }
    public DateTime DateCreatedUtc { get; } = DateTime.UtcNow;
}
