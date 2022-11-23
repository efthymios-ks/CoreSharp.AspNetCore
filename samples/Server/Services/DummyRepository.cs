using Server.Entities;
using Server.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Services;

public sealed class DummyRepository : IDummyRepository
{
    // Methods 
    public Task<Dummy[]> GetAsync()
    {
        var dummies = Enumerable.Range(0, 2)
                                .Select(_ => new Dummy { Id = Guid.NewGuid() })
                                .ToArray();
        return Task.FromResult(dummies);
    }
}