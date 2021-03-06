using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PipeHow.DungeonMastery.RandomDungeon;
using PipeHow.DungeonMastery.RandomDungeon.Dungeondraft;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeHow.DungeonMastery
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

            int roomCount = 7;
            if (req.Query.ContainsKey("roomCount"))
            {
                bool parsedRoomCount = int.TryParse(req.Query["roomCount"], out roomCount);
                if (!parsedRoomCount || roomCount < 1)
                {
                    return new BadRequestObjectResult("Please provide the roomCount as a number above 0.");
                }
            }

            int roomMaxSize = 8;
            int roomMinSize = 4;
            if (req.Query.ContainsKey("roomMinSize") || req.Query.ContainsKey("roomMaxSize"))
            {
                bool parsedMinSize = int.TryParse(req.Query["roomMinSize"], out roomMinSize);
                bool parsedMaxSize = int.TryParse(req.Query["roomMaxSize"], out roomMaxSize);
                if (!parsedMinSize || !parsedMaxSize ||
                    roomMinSize < 4 || roomMaxSize < 4 ||
                    roomMinSize > width / 2 || roomMinSize > height / 2 ||
                    roomMaxSize > width / 2 || roomMaxSize > height / 2)
                {
                    return new BadRequestObjectResult("Please provide roomMinSize and roomMaxSize as numbers above 3 and below half of height and width of map, or leave them out.");
                }
            }

            int seed = 0;
            if (req.Query.ContainsKey("seed"))
            {
                bool parsedSeed = int.TryParse(req.Query["seed"], out seed);
                if (!parsedSeed || seed < 0)
                {
                    return new BadRequestObjectResult("Please provide the roomCount as a number above 0.");
                }
            }

            string output = "string";
            if (req.Query.ContainsKey("output"))
            {
                output = req.Query["output"];
                if (!new string[] { "string", "dungeondraft" }.Contains(output))
                {
                    return new BadRequestObjectResult("Please provide a valid output type or leave it out!");
                }
            }

            bool trace = false;
            if (req.Query.ContainsKey("trace"))
            {
                bool parsedTrace = bool.TryParse(req.Query["trace"], out trace);
                if (!parsedTrace)
                {
                    return new BadRequestObjectResult("Trace was not provided as a boolean value!");
                }
            }

            // http://roguebasin.roguelikedevelopment.org/index.php?title=Dungeon-Building_Algorithm
            // TODO: Add configuration DI for other symbols etc
            IDungeon dungeon = Dungeon.CreateDungeon(width, height, roomMinSize, roomMaxSize, roomCount, seed);

            if (output == "string")
            {
                string responseMessage = dungeon.ToString(trace);
                return new OkObjectResult(responseMessage);
            }
            else if (output == "dungeondraft")
            {
                return new FileContentResult(Encoding.UTF8.GetBytes(dungeon.ToDungeondraftMap()), "application/json")
                {
                    FileDownloadName = $"Dungeon_{dungeon.Seed}.dungeondraft_map"
                };
            }
            else
            {
                return new BadRequestObjectResult("Please provide a valid output type or leave it out!");
            }
        }
    }
}

