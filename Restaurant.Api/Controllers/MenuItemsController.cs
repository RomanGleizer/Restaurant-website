using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.Models;
using Restaurant.Api.Services;

namespace Restaurant.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MenuItemsController(ApplicationDbContext db) : ControllerBase
{
    private readonly ApplicationDbContext _db = db;

    [HttpGet]
    public async Task<IEnumerable<MenuItem>> GetAll() =>
        await _db.MenuItems.ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<MenuItem>> Get(int id)
    {
        var item = await _db.MenuItems.FindAsync(id);
        return item == null ? NotFound() : item;
    }

    [HttpPost]
    public async Task<ActionResult<MenuItem>> Create(MenuItem item)
    {
        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, MenuItem input)
    {
        if (id != input.Id) return BadRequest();
        _db.Entry(input).State = EntityState.Modified;
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) when (!_db.MenuItems.Any(e => e.Id == id))
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.MenuItems.FindAsync(id);
        if (item == null) return NotFound();
        _db.MenuItems.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("upload")]
    public async Task<ActionResult<string>> UploadImage(IFormFile file, [FromServices] IObjectStorageService storageService)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty");

        using var stream = file.OpenReadStream();
        var fileName = $"uploads/{Guid.NewGuid()}_{file.FileName}";

        var url = await storageService.UploadFileAsync(stream, fileName);

        return Ok(new { Url = url });
    }
}
