using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using Chaos.NaCl;
using System.Linq;
using PipeHow.DungeonMastery.Dungeon;

namespace PipeHow.DungeonMastery
{
    public static class DiscordInteraction
    {
        [FunctionName("DiscordInteraction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string bodyString = await new StreamReader(req.Body).ReadToEndAsync();

            // Signature is a hex string
            string signature = req.Headers["X-Signature-Ed25519"];
            string timestamp = req.Headers["X-Signature-Timestamp"];

            log.LogInformation($"Signature: {signature}");
            log.LogInformation($"Timestamp: {timestamp}");
            log.LogInformation($"Body: {bodyString}");
            log.LogInformation("Converting to bytes arrays.");

            // Convert public key from hex string to byte array
            var publicKey = Environment.GetEnvironmentVariable("DiscordBotPublicKey");
            var keyBytes = Enumerable.Range(0, publicKey.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(publicKey.Substring(x, 2), 16))
                .ToArray();

            // Same for signature
            var sigBytes = Enumerable.Range(0, signature.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(signature.Substring(x, 2), 16))
                .ToArray();

            log.LogInformation("Verifying signature.");
            if (!Ed25519.Verify(sigBytes, Encoding.ASCII.GetBytes($"{timestamp}{bodyString}"), keyBytes))
            {
                log.LogError("Could not verify signature!");
                var result = new BadRequestObjectResult("Could not verify signature!");
                result.StatusCode = 401;
                return result;
            }
            log.LogInformation("Verified signature.");

            DiscordInteractionModel data = JsonConvert.DeserializeObject<DiscordInteractionModel>(bodyString);

            object response = "";
            int type = data.Type;
            // Ping
            if (type == 1)
            {
                response = new { type = 1 };
            } // ApplicationCommand / Interaction
            else if (type == 2)
            {
                log.LogInformation($"Received an interaction: {data.Data.Name}.");

                switch (data.Data.Name)
                {
                    case "dungeon":
                        log.LogInformation("Dungeon requested.");

                        // Use 9 digits from interaction id for a seed, enabling recreation of generated maps
                        int seed = int.Parse(data.Id.Substring(0, 9));
                        log.LogInformation($"Seed: {seed}");

                            // Default max size of message in discord is 2000 characters
                        // Square root of 2000 is just below 45, which is 43 + whitespace
                        string dungeon = Dungeon.Dungeon.CreateDungeon(43, 43, seed: seed).ToString();

                        // Create discord message
                        var message = $"```\n{dungeon}\n```";
                        // Create response object
                        log.LogInformation($"Message:\n {message}");
                        response = new
                        {
                            type = 4,
                            data = new
                            {
                                content = message
                            }
                        };
                        break;
                    default:
                        return new BadRequestObjectResult("Unknown command!");
                }
            }
            else
            {
                return new BadRequestObjectResult("Could not understand request!");
            }

            return new OkObjectResult(response);
        }
    }
}
