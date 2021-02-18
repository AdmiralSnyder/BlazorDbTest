using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorDbTest.Data;

namespace BlazorDbTest
{
    public static class Applic
    {
        public static PersonApps PersonApps { get; private set; }
        public static void Init()
        {
            PersonApps = new PersonApps();
            PersonApps.Init();
        }
    }
}
