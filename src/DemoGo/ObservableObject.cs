using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSGO_Demos_Manager
{
    public class ObservableObject
    {
        public void Set<T>(Func<T> p, ref T _id, T value)
        {
            _id = value;
        }

        public void RaisePropertyChanged<T>(Func<T> p)
        {
            
        }
    }
}
