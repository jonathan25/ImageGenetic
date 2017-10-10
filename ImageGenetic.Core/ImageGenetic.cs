using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGenetic.Core
{
    public class ImageGenetic
    {
        private List<byte[]> _population;
        private List<double> _aptitudes;
        private int populationSize;
        private int resolutionSquare;
        private long generations;
        private TimeSpan timeout;
        private Stopwatch stopwatch;
        private Random random = new Random();
        private double minimalAptitude;

        private object _listLock = new object();

        public bool TimedOut
        {
            get;
            private set;
        }

        public bool Done
        {
            get;
            private set;
        }

        public long Generations
        {
            get
            {
                return generations;
            }
        }

        public ReadOnlyCollection<double> Aptitudes
        {
            get
            {
                if (_population == null)
                {
                    return null;
                }
                return _aptitudes.AsReadOnly();
            }
        }

        public ReadOnlyCollection<byte[]> Population
        {
            get
            {
                if (_population == null)
                {
                    return null;
                }
                return _population.AsReadOnly();
            }
        }

        public ImageGeneticState CurrentState
        {
            get
            {
                if (_aptitudes == null || _population == null || _aptitudes.Count != populationSize || _population.Count != populationSize)
                {
                    return new ImageGeneticState(new List<(double, byte[])>(populationSize), 0, false, false, TimeSpan.Zero);
                }

                lock (_listLock)
                {

                    List<(double, byte[])> list = new List<(double, byte[])>();
                    for (int i = 0; i < populationSize; i++)
                    {
                        byte[] png = ImageProcessing.GetImagePNG(ImageProcessing.GetBitmap(_population[i], resolutionSquare));
                        list.Add((_aptitudes[i], png));
                    }
                    ImageGeneticState state = new ImageGeneticState(list, generations, Done, TimedOut, stopwatch.Elapsed);
                    return state;
                }
            }
        }

        public ReadOnlyCollection<Bitmap> PopulationBitmaps
        {
            get
            {
                if (_population == null)
                {
                    return null;
                }

                List<Bitmap> bitmaps = new List<Bitmap>();
                foreach (byte[] bitmapArray in _population)
                {
                    bitmaps.Add(ImageProcessing.GetBitmap(bitmapArray, resolutionSquare));
                }
                return bitmaps.AsReadOnly();
            }
        }

        public ImageGenetic(int populationSize, int resolutionSquare = 50, double minimalAptitude = 0.99, int timeoutMinutes = 1)
        {
            if ((this.populationSize = populationSize) % 2 != 0 && (populationSize) > 20)
            {
                throw new ArgumentException("Population must be a pair number and less than 20");
            }

            if ((this.resolutionSquare = resolutionSquare) > 1024 || resolutionSquare < 50)
            {
                throw new ArgumentException("Resolution must be greater than 50px and no more than 1024px");
            }

            if ((this.minimalAptitude = minimalAptitude) > 1)
            {
                throw new ArgumentException("Minimal aptitude can only be as maximum 1.");
            }

            if (timeoutMinutes > 5)
            {
                throw new ArgumentException("Timeout can only last 5 minutes");
            }
            timeout = TimeSpan.FromMinutes(timeoutMinutes);

            Done = true;
        }

        public void Stop()
        {
            Done = true;
        }

        public void Start()
        {

            _population = new List<byte[]>();
            _aptitudes = new List<double>(populationSize);
            lock (_listLock)
            {
                // First population generation
                for (int i = 0; i < populationSize; i++)
                {
                    byte[] bitmap = ImageProcessing.GenerateRandomImageArray(resolutionSquare);
                    _population.Add(bitmap);
                }

                Done = false;
                generations = 1;

                stopwatch = Stopwatch.StartNew();
            }
            while (!Done)
            {
                lock (_listLock)
                {
                    _aptitudes = ComputeAptitudes();
                }
                if (_aptitudes.Any(a => a >= minimalAptitude))
                {
                    stopwatch.Stop();
                    TimedOut = false;
                    Done = true;
                    break;
                }
                else if (stopwatch.Elapsed >= timeout)
                {
                    stopwatch.Stop();
                    TimedOut = true;
                    Done = true;
                    break;
                }

                double total = _aptitudes.Sum();
                List<double> normalizedAptitudes = Enumerable.Repeat(1.0 / populationSize, populationSize).ToList();
                if (total != 0)
                {
                    normalizedAptitudes = _aptitudes.Select(a => a / total).ToList();
                }

                ProportionValue<byte[]>[] roulette = new ProportionValue<byte[]>[populationSize];
                for (int i = 0; i < populationSize; i++)
                {
                    roulette[i] = ProportionValue.Create(normalizedAptitudes[i], _population[i]);
                }

                //making random copies
                var copies = new List<byte[]>();
                lock (_listLock)
                {
                    for (int i = 0; i < populationSize; i++)
                    {
                        copies.Add(ProportionValue.ChooseByRandom(roulette));
                    }
                }
                _population = copies;

                //Mutation and crossover
                List<byte[]> newPopulation = new List<byte[]>();
                var oldPopulation = _population;
                while (oldPopulation.Count >= 2)
                {
                    byte[] image = oldPopulation[0];
                    oldPopulation.RemoveAt(0);
                    List<(int, double)> imageAptitudes = new List<(int, double)>();
                    for (int i = 0; i < oldPopulation.Count; i++)
                    {
                        byte[] individual = oldPopulation[i];
                        var combined = ImageProcessing.CombineImages(image, individual, resolutionSquare);
                        double first = ComputeSingleAptitude(combined.Item1);
                        double second = ComputeSingleAptitude(combined.Item2);
                        imageAptitudes.Add((i, Math.Max(first, second)));
                    }
                    var bestCandidate = imageAptitudes.Aggregate((a, b) => a.Item2 > b.Item2 ? a : b);
                    byte[] bestImage = oldPopulation[bestCandidate.Item1];
                    oldPopulation.RemoveAt(bestCandidate.Item1);

                    var crossed = Crossover(bestImage, image);

                    newPopulation.Add(Mutate(crossed.Item1));
                    newPopulation.Add(Mutate(crossed.Item2));
                }
                _population = newPopulation;
                generations++;

            }
        }

        private byte[] Mutate(byte[] image)
        {
            if (random.NextDouble() > 0.5)
            {
                return ImageProcessing.ChangeHueHalf(image, resolutionSquare, random.NextDouble() > 0.5, random.NextDouble() * 360);
            }
            return ImageProcessing.LuminosityRandom(image, resolutionSquare);
        }

        private (byte[], byte[]) Crossover(byte[] first, byte[] second)
        {
            return ImageProcessing.CombineImages(first, second, resolutionSquare);
        }

        private List<double> ComputeAptitudes()
        {
            List<double> aptitudes = new List<double>();
            foreach (byte[] individual in _population)
            {
                double aptitude = ComputeSingleAptitude(individual);
                aptitudes.Add(aptitude);
            }
            return aptitudes;
        }

        private double ComputeSingleAptitude(byte[] image)
        {
            var averages = ImageProcessing.GetAveragesHue(image, resolutionSquare);
            var hsl1 = averages;
            //var hsl2 = averages;

            double difference = Math.Abs(hsl1.Item1 * 360 - hsl1.Item2 * 360);

            double aptitude;
            if (difference <= ImageProcessing.GOLDEN_ANGLE)
            {
                aptitude = Math.Abs(difference / ImageProcessing.GOLDEN_ANGLE);
            }
            else
            {
                aptitude = Math.Abs((360 - difference) / (360 - ImageProcessing.GOLDEN_ANGLE));
            }
            return aptitude;// = aptitude / 2 + (hsl1.Item2 + hsl2.Item2) / 4;
        }

        /*private double ComputeSingleAptitude(Bitmap image)
        {
            var averages = ImageProcessing.GetAverages(image);
            var hsl1 = ImageProcessing.FromRGBToHSL(averages.Item1);
            var hsl2 = ImageProcessing.FromRGBToHSL(averages.Item2);
            
            double difference = Math.Abs(hsl1.Item1 * 360 - hsl2.Item1 * 360);

            double aptitude;
            if (difference <= ImageProcessing.GOLDEN_ANGLE)
            {
                aptitude = Math.Abs(difference / ImageProcessing.GOLDEN_ANGLE);
            }
            else
            {
                aptitude = Math.Abs((360 - difference) / (360 - ImageProcessing.GOLDEN_ANGLE));
            }
            return aptitude;
        }*/

    }
}
