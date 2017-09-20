using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGenetic.Core
{
    public class ImageGenetic
    {
        public List<Bitmap> Population { get; private set; }
        private Random random;
        
        public ImageGenetic(int population)
        {
            if (population % 2 != 0) {
                throw new ArgumentException("Population must be a pair number");
            }

            //Population = new List<Bitmap>(population);

            for (int i = 0; i <= population; i++)
            {
                
            }
            
            
        }

        public void Start()
        {
            //start time counter
            


            //stop time counter
        }

        private void Crossover()
        {

        }

        private void Function()
        {
            
        }
    }
}
