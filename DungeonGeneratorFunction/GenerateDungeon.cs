using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PipeHow.DungeonGenerator.Models;
using System.Threading.Tasks;

namespace PipeHow.DungeonGenerator
{
    public static class GenerateDungeon
    {
        [FunctionName("GenerateDungeon")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            int width = 43;
            int height = 43;
            if (req.Query.ContainsKey("width") || req.Query.ContainsKey("height"))
            {
                if (!int.TryParse(req.Query["width"], out width) || !int.TryParse(req.Query["height"], out height))
                {
                    return new BadRequestObjectResult("Please provide width and height as numbers, or leave them out.");
                }
            }
            int roomCount = 8;
            if (req.Query.ContainsKey("roomCount"))
            {
                if (!int.TryParse(req.Query["roomCount"], out roomCount))
                {
                    return new BadRequestObjectResult("Please provide width and height as numbers, or leave them out.");
                }
            }

            // http://roguebasin.roguelikedevelopment.org/index.php?title=Dungeon-Building_Algorithm
            // TODO: Add configuration DI for other symbols etc
            IDungeon dungeon = Dungeon.CreateDungeon(width, height, roomCount);
            string responseMessage = dungeon.ToString();

            return new OkObjectResult(responseMessage);
        }
    }
}

