using Server.Entities;
using System.Threading.Tasks;

namespace Server.Services.Interfaces;

public interface IDummyRepository
{
    // Methods
    Task<Dummy[]> GetAsync();
}
