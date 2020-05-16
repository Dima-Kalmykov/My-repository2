﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    internal class MyPoint
    {
        internal long X;
        internal long Y;

        public MyPoint(long x, long y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => "{" + $"{X}, {Y}" + "}";
    }
}
