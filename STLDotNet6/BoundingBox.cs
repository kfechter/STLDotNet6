using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STLDotNet6
{
    public class BoundingBox
    {
        /// <summary>The X dimension of the bounding box of the STL.</summary>
        public double X { get; set; }

        /// <summary>The Y dimension of the bounding box of the STL.</summary>
        public double Y { get; set; }

        /// <summary>The Z dimension of the bounding box of the STL.</summary>
        public double Z { get; set; }

        /// <summary>
        /// Creates an empty center of mass object
        /// </summary>
        public BoundingBox() { }

        /// <summary>Creates a new <see cref="BoundingBox"/> using the provided coordinates.</summary>
        /// <param name="x"><see cref="X"/></param>
        /// <param name="y"><see cref="Y"/></param>
        /// <param name="z"><see cref="Z"/></param>
        public BoundingBox(double x, double y, double z)
            : this()
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}
