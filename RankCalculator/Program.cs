using NATS.Client;
using System.Text;
using StackExchange.Redis;
namespace Producer
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigurationOptions redisConfiguration = ConfigurationOptions.Parse("localhost:6379");
            ConnectionMultiplexer redisConnection = ConnectionMultiplexer.Connect(redisConfiguration);
            IDatabase db = redisConnection.GetDatabase();
            ConnectionFactory cf = new ConnectionFactory();
            IConnection c = cf.CreateConnection();
            Console.WriteLine("RankCalculator started");

            var s = c.SubscribeAsync("valuator.processing.rank", "rankCalculator", (sender, args) =>
            {
                string id = Encoding.UTF8.GetString(args.Message.Data);

                string textKey = "TEXT-" + id;
                string text = db.StringGet(textKey);

                string rankKey = "RANK-" + id;

                string rank = getRank(text);

                db.StringSet(rankKey, rank);

            });

            s.Start();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();

            s.Unsubscribe();

            c.Drain();
            c.Close();
        }
        static string getRank(string text)
        {
            double all = text.Length;
            double nonAlphabetic = 0;

            foreach (char word in text)
            {
                if (!char.IsLetter(word))
                {
                    nonAlphabetic++;
                }
            }

            return (nonAlphabetic / all).ToString();
        }
    }
}