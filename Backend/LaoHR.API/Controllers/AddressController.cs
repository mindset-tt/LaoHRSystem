using LaoHR.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LaoHR.API.Controllers;

[ApiController]
[Route("api/address")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _service;

    public AddressController(IAddressService service)
    {
        _service = service;
    }

    [HttpGet("provinces")]
    public async Task<IActionResult> GetProvinces()
    {
        var provinces = await _service.GetProvincesAsync();
        return Ok(provinces);
    }

    [HttpGet("districts/{provinceId}")]
    public async Task<IActionResult> GetDistricts(int provinceId)
    {
        var districts = await _service.GetDistrictsAsync(provinceId);
        return Ok(districts);
    }

    [HttpGet("villages/{districtId}")]
    public async Task<IActionResult> GetVillages(int districtId)
    {
        var villages = await _service.GetVillagesAsync(districtId);
        return Ok(villages);
    }
}
