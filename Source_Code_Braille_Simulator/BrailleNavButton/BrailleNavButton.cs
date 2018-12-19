using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrailleNavButton
{


    public class BrailleNavButton
    {
        public direction Direction { get; set; }

        public enum direction
        {
                top,
                left,
                right,
                bottom,
                middel
        };
    }
}
