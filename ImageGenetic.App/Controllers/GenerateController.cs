using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace ImageGenetic.App.Controllers
{
    [Route("api/[controller]")]
    public class GenerateController : Controller
    {
        // GET api/values/5
        [HttpGet("{id}")]
        public IActionResult Get(long id)
        {
            var generation = ImageGeneration.GenerationHandler.Find(id);
            if (generation == null)
            {
                var notFound = new { Error = true, Message = "Not Found" };
                return Json(notFound);
            }

            return Json(new ImageGeneration.GenerationResponse(generation.ImageGeneration));
        }

        [HttpGet("encoded/{id}")]
        public IActionResult GetEncoded(string id)
        {
            var generation = ImageGeneration.GenerationHandler.Find(id);
            if (generation == null)
            {
                var notFound = new { Error = true, Message = "Not Found" };
                return Json(notFound);
            }

            return Json(new ImageGeneration.GenerationResponse(generation.ImageGeneration));
        }

        // POST api/values
        [HttpPost]
        public JsonResult Post(ImageGeneration.GenerationParameters value)
        {
            if (value == null)
            {
                var notFound = new { Error = true, Message = "Invalid request" };
                return Json(notFound);
            }

            ImageGeneration.Generation gen;
            try
            {
                gen = new ImageGeneration.Generation(value);
            } catch (ArgumentException ex)
            {
                var notFound = new { Error = true, Message = ex.Message };
                return Json(notFound);
            }

            bool created = ImageGeneration.GenerationHandler.CreateNew(gen);
            if (created)
            {
                gen.Start();

                var generationObject = new
                {
                    Error = false,
                    Id = gen.Id
                };
                
                return Json(generationObject);
            }

            var unknownError = new { Error = true, Message = "Unknown" };
            return Json(unknownError);
        }

        // PUT api/values/5
        /*[HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
    }
}
