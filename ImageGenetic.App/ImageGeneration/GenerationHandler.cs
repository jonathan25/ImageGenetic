using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageGenetic.Core;
using System.Collections.Concurrent;

namespace ImageGenetic.App.ImageGeneration
{
    public static class GenerationHandler
    {
        private static ConcurrentDictionary<long, Generation> Generations = new ConcurrentDictionary<long, Generation>();

        public static bool CreateNew(Generation generation)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            var toRemove = Generations
                .Where(gen => now >= gen.Value.IdTime.AddMinutes(15))
                .Select(gen => gen.Value);

            foreach (Generation oldGeneration in toRemove)
            {
                Generations.TryRemove(oldGeneration.IdTime.ToUnixTimeMilliseconds(), out Generation toDispose);
                toDispose = null;
            }

            return Generations.TryAdd(generation.IdTime.ToUnixTimeMilliseconds(), generation);
        }

        public static Generation Find(long id)
        {
            bool found = Generations.TryGetValue(id, out Generation generation);
            if (found)
            {
                return generation;
            }
            return null;
        }

        public static Generation Find(string encodedId)
        {
            string incoming = encodedId.Replace('_', '/').Replace('-', '+');
            switch (encodedId.Length % 4)
            {
                case 2: incoming += "=="; break;
                case 3: incoming += "="; break;
            }
            byte[] bytes = Convert.FromBase64String(incoming);
            long id = BitConverter.ToInt64(bytes, 0);

            return Find(id);

        }

    }

    public class Generation
    {
        static readonly char[] padding = { '=' };

        private DateTimeOffset time;
        private Core.ImageGenetic generation;

        public Core.ImageGenetic ImageGeneration
        {
            get
            {
                return generation;
            }
        }

        public void Start()
        {
            Task.Run(() => generation.Start());
        }

        public Generation(int populationSize = 10, int resolutionSquare = 50, double minimalAptitude = 0.99, int timeoutMinutes = 1)
        {
            time = DateTimeOffset.UtcNow;
            generation = new Core.ImageGenetic(populationSize, resolutionSquare, minimalAptitude, timeoutMinutes);
        }

        public Generation(GenerationParameters parameters)
        {
            time = DateTimeOffset.UtcNow;
            generation = new Core.ImageGenetic(parameters.PopulationSize, parameters.ResolutionSquare, parameters.MinimalAptitude, parameters.TimeoutMinutes);
        }

        public DateTimeOffset IdTime
        {
            get
            {
                return time;
            }
        }

        public string Id
        {
            //https://stackoverflow.com/questions/26353710/how-to-achieve-base64-url-safe-encoding-in-c
            get
            {
                byte[] id = BitConverter.GetBytes(time.ToUnixTimeMilliseconds());
                return Convert.ToBase64String(id)
                    .TrimEnd(padding).Replace('+', '-').Replace('/', '_');
            }
        }

    }

    public class GenerationParameters
    {
        public GenerationParameters()
        {
            PopulationSize = 10;
            ResolutionSquare = 50;
            MinimalAptitude = 0.99;
            TimeoutMinutes = 1;
        }

        public int PopulationSize { get; set; }
        public int ResolutionSquare { get; set; }
        public double MinimalAptitude { get; set; }
        public int TimeoutMinutes { get; set; }
    }

    public class GenerationResponse
    {

        public List<object> Images { get; set; }
        public bool Done { get; set; }
        public long Generations { get; set; }
        public bool TimedOut { get; set; }
        public double SecondsElapsed { get; set; }

        public GenerationResponse(Core.ImageGenetic imageGenetic)
        {
            Images = new List<object>();
            var state = imageGenetic.CurrentState;

            var pngImages = state.PopulationAptitudes;
            foreach (var image in pngImages)
            {
                string encodedImage = Convert.ToBase64String(image.Item2);
                var x = new { Aptitude = image.Item1, Image = encodedImage };
                Images.Add(x);
            }

            Generations = state.Generations;
            Done = state.Done;
            TimedOut = state.TimedOut;
            SecondsElapsed = state.SecondsElapsed;
        }
    }

}
