using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Database;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private IImageDB db;

        public ImagesController(IImageDB db)
        {
            this.db = db;
        }

        [HttpPost]
        public async Task<ActionResult<Picture>> AddImage([FromBody] NewPicture image, CancellationToken ct)
        {
            try
            {
                var result = await db.PostImage(image.Bytes, image.FilePath, ct);
                return StatusCode((int)HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NoContent();
            }
        }


        [HttpGet("{emotion}")]
        public async Task<ActionResult<List<Picture>>> GetImages(string emotion)
        {
            var res = await db.GetImagesByEmotion(emotion);
            if (res != null)
                return res;
            else
                return StatusCode(404, "Not found");
        }

        [HttpGet]
        public async Task<ActionResult<List<Picture>>> GetAllImages()
        {
            return await db.GetAllImages();
        }

        [HttpDelete]
        public int DeleteImages()
        {
            return db.DeleteAll();
        }
    }
}

