namespace Todo.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Todo.Models;
    using Microsoft.AspNetCore.Authorization;
    using System.Linq;

    [ApiController]
    [Route("api/items")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ItemController : Controller
    {
        private readonly ICosmosDbService _cosmosDbService;
        public ItemController(ICosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var appUserId = User.Claims.First(c => c.Type == "sub").Value;
                Console.WriteLine(appUserId);
                return Ok(await _cosmosDbService.GetItemsAsync($"SELECT * FROM Items i WHERE i.userId = \"{appUserId}\""));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }

        }


        [HttpPost]
        public async Task<ActionResult> CreateAsync([Bind("Id,UserId,Name,Description,Completed")] Item item)
        {
            if (ModelState.IsValid)
            {
                item.Id = Guid.NewGuid().ToString();
                await _cosmosDbService.AddItemAsync(item);
                return Ok(item);
            }

            return BadRequest("Invalid item model");
        }


        [HttpPut("{id}")]
        public async Task<ActionResult> EditAsync([Bind("Id,UserId,Name,Description,Completed")] Item item)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    await _cosmosDbService.UpdateItemAsync(item.Id, item);
                    return Ok(item);

                }
                catch (Exception e)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
                }
            }
            return BadRequest("Invalid item model");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Item item = await _cosmosDbService.GetItemAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            await _cosmosDbService.DeleteItemAsync(id);
            return Ok();
        }


    }
}