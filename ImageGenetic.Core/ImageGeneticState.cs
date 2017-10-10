using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ImageGenetic.Core
{
    public class ImageGeneticState
    {
        public long Generations { get; }
        public ReadOnlyCollection<(double, byte[])> PopulationAptitudes { get; private set; }
        public bool TimedOut { get; }
        public bool Done { get; }
        public double SecondsElapsed { get; }
        internal ImageGeneticState(List<(double, byte[])> populationAptitudes, long generations, bool done, bool timedOut, TimeSpan elapsed)
        {
            PopulationAptitudes = populationAptitudes.AsReadOnly();
            Generations = generations;
            Done = done;
            TimedOut = timedOut;
            SecondsElapsed = elapsed.TotalSeconds;
        }

    }
}
