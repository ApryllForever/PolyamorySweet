using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyamorySweetLove
{
    internal class ManualExclusionList
    {
        public List<string> ExclusionList(string name) 
        {

            List<string> mojovision = new List<string>();

            mojovision.Add("Mateo");

            mojovision.Add("Hector");

            mojovision.Add("Cirrus");

            mojovision.Add("Dandelion");

            mojovision.Add("Roslin");

            mojovision.Add("Solomon");

            mojovision.Add("Stiles");

            

            return new List<string>(ExclusionList(name)); 
        }
    }
}
