using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NATS.Client;
using System.Text;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IRedis _redis;

    public IndexModel(ILogger<IndexModel> logger, IRedis redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Redirect("index");
        }
        else
        {
            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();

            string similarityKey = "SIMILARITY-" + id;
            //TODO: посчитать similarity и сохранить в БД по ключу similarityKey
            _redis.Put(similarityKey, getSimilarity(text));

            string textKey = "TEXT-" + id;
            //TODO: сохранить в БД text по ключу textKey
            _redis.Put(textKey, text);

            //TODO: посчитать rank и сохранить в БД по ключу rankKey

            ConnectionFactory connectionFactory = new ConnectionFactory();

            using(IConnection c = connectionFactory.CreateConnection())
            {
                byte[] data = Encoding.UTF8.GetBytes(id);
                c.Publish("valuator.processing.rank", data);

                c.Drain();

                c.Close();
            }

            return Redirect($"summary?id={id}");
        }
    }

    private string getSimilarity(string text)
    {
        var keys = _redis.GetKeys();
        string textKeyPrefix = "TEXT-";

        foreach (var key in keys)
        {
            if (key.StartsWith(textKeyPrefix) && _redis.Get(key) == text)
            {
                return "1";
            }
        }

        return "0";
    }
}
