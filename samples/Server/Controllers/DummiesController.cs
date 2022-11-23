using Microsoft.AspNetCore.Mvc;
using Server.Controllers.Common;
using Server.Entities;
using Server.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Controllers;

public sealed class DummiesController : AppControllerBase
{
    // Fields
    private readonly IDummyRepository _dummyRepository;

    // Constructors
    public DummiesController(IDummyRepository dummyRepository)
        => _dummyRepository = dummyRepository;

    // Methods
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Dummy>), 200)]
    public async Task<IActionResult> GetAsync()
    {
        var dummies = await _dummyRepository.GetAsync();
        return Ok(dummies);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(Dummy dummy)
    {
        await Task.CompletedTask;
        return Ok(dummy);
    }
}
