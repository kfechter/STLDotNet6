﻿using System.Text;
using System.Text.RegularExpressions;

namespace STLDotNet6.Formats.StereoLithography
{
    /// <summary>The outer-most STL object which contains the <see cref="Facets"/> that make up the model.</summary>
    public class STLDocument : IEquatable<STLDocument>, IEnumerable<Facet>
    {
        /// <summary>Defines the buffer size to use when reading from a <see cref="StreamReader"/>.</summary>
        private const int DefaultBufferSize = 1024;

        /// <summary>The name of the solid.</summary>
        /// <remarks>This property is not used for binary STLs.</remarks>
        public string? Name { get; set; }

        /// <summary>The density of the solid.</summary>
        public double Density => 1.04;

        /// <summary>The weight of the solid.</summary>
        public double Weight => this.Density * this.Volume;

        /// <summary>The area of the solid.</summary>
        public double Area
        {
            get
            {
                var area = 0d;

                foreach(var facet in this.Facets)
                {
                    var ab = facet.Vertices[1].Sub(facet.Vertices[0]);
                    var ac = facet.Vertices[2].Sub(facet.Vertices[0]);

                    var cross = ab.Cross(ac);

                    area += (cross.Length() / 2);
                }

                return area;
            }
        }

        /// <summary>The list of <see cref="Facet"/>s within this solid.</summary>
        public IList<Facet> Facets { get; set; }

        /// <summary>The volume of the solid.</summary>
        public double Volume 
        {
            get
            {
                var volume = 0d;
                foreach(var facet in Facets)
                {
                    var v321 = facet!.Vertices[2]!.X * facet!.Vertices[1]!.Y * facet!.Vertices[0]!.Z;
                    var v231 = facet!.Vertices[1]!.X * facet!.Vertices[2]!.Y * facet!.Vertices[0]!.Z;
                    var v312 = facet!.Vertices[2]!.X * facet!.Vertices[0]!.Y * facet!.Vertices[1]!.Z;
                    var v132 = facet!.Vertices[0]!.X * facet!.Vertices[2]!.Y * facet!.Vertices[1]!.Z;
                    var v213 = facet!.Vertices[1]!.X * facet!.Vertices[0]!.Y * facet!.Vertices[2]!.Z;
                    var v123 = facet!.Vertices[0]!.X * facet!.Vertices[1]!.Y * facet!.Vertices[2]!.Z;

                    var facetVolume = (1.0 / 6.0) * (-v321 + v231 + v312 - v132 - v213 + v123);
                    volume += facetVolume;
                }

                return Math.Abs(volume) / 1000;
            } 
        }

        public BoundingBox BoundingBox
        {
            get
            {
                var minx = 0d;
                var miny = 0d;
                var minz = 0d;
                var maxx = 0d;
                var maxy = 0d;
                var maxz = 0d;

                foreach(var facet in this.Facets)
                {
                    
                    var tminx = new[] { facet!.Vertices[0]!.X, facet!.Vertices[1]!.X, facet!.Vertices[2]!.X }.Min();
                    minx = tminx < minx ? tminx : minx;


                    var tmaxx = new[] { facet!.Vertices[0]!.X, facet!.Vertices[1]!.X, facet!.Vertices[2]!.X }.Max();
                    maxx = tmaxx > maxx ? tmaxx : maxx;


                    var tminy = new[] { facet!.Vertices[0]!.Y, facet!.Vertices[1]!.Y, facet!.Vertices[2]!.Y }.Min();
                    miny = tminy < miny ? tminy : miny;

                    var tmaxy = new[] { facet!.Vertices[0]!.Y, facet!.Vertices[1]!.Y, facet!.Vertices[2]!.Y }.Max();
                    maxy = tmaxy > maxy ? tmaxy : maxy;

                    var tminz = new[] { facet!.Vertices[0]!.Z, facet!.Vertices[1]!.Z, facet!.Vertices[2]!.Z }.Min();
                    minz = tminz < minz ? tminz : minz;

                    var tmaxz = new[] { facet!.Vertices[0]!.Z, facet!.Vertices[1]!.Z, facet!.Vertices[2]!.Z }.Max();
                    maxz = tmaxz > maxz ? tmaxz : maxz;
                }

                var boundingX = maxx - minx;
                var boundingY = maxy - miny;
                var boundingZ = maxz - minz;

                return new BoundingBox(boundingX, boundingY, boundingZ);
            }
        }

        public CenterOfMass CenterOfMass
        {
            get
            {
                var xCenter = 0d;
                var yCenter = 0d;
                var zCenter = 0d;
                foreach (var facet in Facets)
                {
                    var v321 = facet!.Vertices[2]!.X * facet!.Vertices[1]!.Y * facet!.Vertices[0]!.Z;
                    var v231 = facet!.Vertices[1]!.X * facet!.Vertices[2]!.Y * facet!.Vertices[0]!.Z;
                    var v312 = facet!.Vertices[2]!.X * facet!.Vertices[0]!.Y * facet!.Vertices[1]!.Z;
                    var v132 = facet!.Vertices[0]!.X * facet!.Vertices[2]!.Y * facet!.Vertices[1]!.Z;
                    var v213 = facet!.Vertices[1]!.X * facet!.Vertices[0]!.Y * facet!.Vertices[2]!.Z;
                    var v123 = facet!.Vertices[0]!.X * facet!.Vertices[1]!.Y * facet!.Vertices[2]!.Z;

                    var facetVolume = (1.0 / 6.0) * (-v321 + v231 + v312 - v132 - v213 + v123);


                    xCenter += (facet!.Vertices[0]!.X + facet!.Vertices[1]!.X + facet!.Vertices[2]!.X) / 4 * facetVolume;
                    yCenter += (facet!.Vertices[0]!.Y + facet!.Vertices[1]!.Y + facet!.Vertices[2]!.Y) / 4 * facetVolume;
                    zCenter += (facet!.Vertices[0]!.Z + facet!.Vertices[1]!.Z + facet!.Vertices[2]!.Z) / 4 * facetVolume;
                }

                xCenter /= 1000;
                yCenter /= 1000;
                zCenter /= 1000;

                return new CenterOfMass(xCenter, yCenter, zCenter);
            }
        }

        /// <summary>Creates a new, empty <see cref="STLDocument"/>.</summary>
        public STLDocument()
        {
            this.Facets = new List<Facet>();
        }

        /// <summary>Creates a new <see cref="STLDocument"/> with the given <paramref name="name"/> and populated with the given <paramref name="facets"/>.</summary>
        /// <param name="name">
        ///     The name of the solid.
        ///     <remarks>This property is not used for binary STLs.</remarks>
        /// </param>
        /// <param name="facets">The facets with which to populate this solid.</param>
        public STLDocument(string name, IEnumerable<Facet> facets)
            : this()
        {
            this.Name = name;
            this.Facets = facets.ToList();
        }

        /// <summary>Writes the <see cref="STLDocument"/> as text to the provided <paramref name="stream"/>.</summary>
        /// <param name="stream">The stream to which the <see cref="STLDocument"/> will be written.</param>
        public void WriteText(Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.ASCII, DefaultBufferSize, true))
            {
                //Write the header.
                writer.WriteLine(this.ToString());

                //Write each facet.
                this.Facets.ForEach(o => o.Write(writer));

                //Write the footer.
                writer.Write($"end{this}");
            }
        }

        /// <summary>Writes the <see cref="STLDocument"/> as binary to the provided <paramref name="stream"/>.</summary>
        /// <param name="stream">The stream to which the <see cref="STLDocument"/> will be written.</param>
        public void WriteBinary(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                byte[] header = Encoding.ASCII.GetBytes("Binary STL generated by STLdotNET. QuantumConceptsCorp.com");
                byte[] headerFull = new byte[80];

                Buffer.BlockCopy(header, 0, headerFull, 0, Math.Min(header.Length, headerFull.Length));

                //Write the header and facet count.
                writer.Write(headerFull);
                writer.Write((UInt32)this.Facets.Count);

                //Write each facet.
                this.Facets.ForEach(o => o.Write(writer));
            }
        }

        /// <summary>Writes the <see cref="STLDocument"/> as text to the provided <paramref name="path"/>.</summary>
        /// <param name="path">The absolute path where the <see cref="STLDocument"/> will be written.</param>
        public void SaveAsText(string path)
        {
            if (path.IsNullOrEmpty())
                throw new ArgumentNullException("path");

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            using (Stream stream = File.Create(path))
                WriteText(stream);
        }

        /// <summary>Writes the <see cref="STLDocument"/> as binary to the provided <paramref name="path"/>.</summary>
        /// <param name="path">The absolute path where the <see cref="STLDocument"/> will be written.</param>
        public void SaveAsBinary(string path)
        {
            if (path.IsNullOrEmpty())
                throw new ArgumentNullException("path");

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            using (Stream stream = File.Create(path))
                WriteBinary(stream);
        }

        /// <summary>Appends the provided facets to this instance's <see cref="Facets"/>.</summary>
        /// <remarks>An entire <see cref="STLDocument"/> can be passed to this method and all of the facets which it contains will be appended to this instance.</remarks>
        /// <param name="facets">The facets to append.</param>
        public void AppendFacets(IEnumerable<Facet> facets)
        {
            foreach (Facet facet in facets)
                this.Facets.Add(facet);
        }

        /// <summary>Determines if the <see cref="STLDocument"/> contained within the <paramref name="stream"/> is text-based.</summary>
        /// <remarks>The <paramref name="stream"/> will be reset to position 0.</remarks>
        /// <param name="stream">The stream which contains the STL data.</param>
        /// <returns>True if the <see cref="STLDocument"/> is text-based, otherwise false.</returns>
        public static bool IsText(Stream stream)
        {
            const string solid = "solid";

            byte[] buffer = new byte[5];
            string? header = null;

            //Reset the stream to tbe beginning and read the first few bytes, then reset the stream to the beginning again.
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);
            stream.Seek(0, SeekOrigin.Begin);

            //Read the header as ASCII.
            header = Encoding.ASCII.GetString(buffer);

            return solid.Equals(header, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>Determines if the <see cref="STLDocument"/> contained within the <paramref name="stream"/> is binary-based.</summary>
        /// <remarks>The <paramref name="stream"/> will be reset to position 0.</remarks>
        /// <param name="stream">The stream which contains the STL data.</param>
        /// <returns>True if the <see cref="STLDocument"/> is binary-based, otherwise false.</returns>
        public static bool IsBinary(Stream stream)
        {
            return !IsText(stream);
        }

        /// <summary>Reads the <see cref="STLDocument"/> contained within the <paramref name="stream"/> into a new <see cref="STLDocument"/>.</summary>
        /// <remarks>This method will determine how to read the <see cref="STLDocument"/> (whether to read it as text or binary data).</remarks>
        /// <param name="stream">The stream which contains the STL data.</param>
        /// <param name="tryBinaryIfTextFailed">Set to true to try read as binary if reading as text results in zero facets</param>
        /// <returns>An <see cref="STLDocument"/> representing the data contained in the stream or null if the stream is empty.</returns>
        public static STLDocument? Read(Stream stream, bool tryBinaryIfTextFailed = false)
        {
            //Determine if the stream contains a text-based or binary-based <see cref="STLDocument"/>, and then read it.
            var isText = IsText(stream);
            STLDocument? textStlDocument = null;
            if (isText)
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.ASCII, true, DefaultBufferSize, true))
                {
                    textStlDocument = Read(reader);
                }

                if (textStlDocument!.Facets.Count > 0 || !tryBinaryIfTextFailed) return textStlDocument;
                stream.Seek(0, SeekOrigin.Begin);
            }

            //Try binary if zero Facets were read and tryBinaryIfTextFailed==true
            using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var binaryStlDocument = Read(reader);

                //return text reading result if binary reading also failed and tryBinaryIfTextFailed==true
                return (binaryStlDocument!.Facets.Count > 0 || !isText) ? binaryStlDocument : textStlDocument;
            }
        }

        /// <summary>Reads the STL document contained within the <paramref name="stream"/> into a new <see cref="STLDocument"/>.</summary>
        /// <remarks>This method expects a text-based STL document to be contained within the <paramref name="reader"/>.</remarks>
        /// <param name="reader">The reader which contains the text-based STL data.</param>
        /// <returns>An <see cref="STLDocument"/> representing the data contained in the stream or null if the stream is empty.</returns>
        public static STLDocument? Read(StreamReader reader)
        {
            const string regexSolid = @"solid\s+(?<Name>[^\r\n]+)?";

            if (reader == null)
                return null;

            //Read the header.
            string? header = reader.ReadLine();
            Match headerMatch = Regex.Match(header!, regexSolid);
            STLDocument? stl = null;
            Facet? currentFacet = null;

            //Check the header.
            if (!headerMatch.Success)
                throw new FormatException($"Invalid STL header, expected \"solid [name]\" but found \"{header}\".");

            //Create the STL and extract the name (optional).
            stl = new STLDocument()
            {
                Name = headerMatch.Groups["Name"].Value
            };

            //Read each facet until the end of the stream.
            while ((currentFacet = Facet.Read(reader)) != null)
                stl.Facets.Add(currentFacet);

            return stl;
        }

        /// <summary>Reads the STL document contained within the <paramref name="stl"/> parameter into a new <see cref="STLDocument"/>.</summary>
        /// <param name="stl">A string which contains the STL data.</param>
        /// <returns>An <see cref="STLDocument"/> representing the data contained in the <paramref name="stl"/> parameter or null if the parameter is empty.</returns>
        public static STLDocument? Read(string stl)
        {
            if (stl.IsNullOrEmpty())
                return null;

            using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(stl)))
                return Read(stream);
        }

        /// <summary>Reads the STL document located at the <paramref name="path"/> into a new <see cref="STLDocument"/>.</summary>
        /// <param name="path">A full path to a file which contains the STL data.</param>
        /// <returns>An <see cref="STLDocument"/> representing the data contained in the file located at the <paramref name="path"/> specified or null if the parameter is empty.</returns>
        public static STLDocument? Open(string path)
        {
            if (path.IsNullOrEmpty())
                throw new ArgumentNullException("path");

            using (Stream stream = File.OpenRead(path))
                return Read(stream);
        }

        /// <summary>Reads the STL document contained within the <paramref name="stream"/> into a new <see cref="STLDocument"/>.</summary>
        /// <remarks>This method will expects a binary-based <see cref="STLDocument"/> to be contained within the <paramref name="reader"/>.</remarks>
        /// <param name="reader">The reader which contains the binary-based STL data.</param>
        /// <returns>An <see cref="STLDocument"/> representing the data contained in the stream or null if the stream is empty.</returns>
        public static STLDocument? Read(BinaryReader reader)
        {
            if (reader == null)
                return null;

            byte[] buffer = new byte[80];
            STLDocument? stl = new STLDocument();
            Facet? currentFacet = null;

            //Read (and ignore) the header and number of triangles.
            buffer = reader.ReadBytes(80);
            reader.ReadBytes(4);

            //Read each facet until the end of the stream. Stop when the end of the stream is reached.
            while ((reader.BaseStream.Position != reader.BaseStream.Length) && (currentFacet = Facet.Read(reader)) != null)
                stl.Facets.Add(currentFacet);

            return stl;
        }

        /// <summary>Reads the <see cref="STLDocument"/> within the <paramref name="inStream"/> as text into the <paramref name="outStream"/>.</summary>
        /// <param name="inStream">The stream to read from.</param>
        /// <param name="outStream">The stream to read into.</param>
        /// <returns>The <see cref="STLDocument"/> that was copied.</returns>
        public static STLDocument CopyAsText(Stream inStream, Stream outStream)
        {
            STLDocument? stl = Read(inStream);

            stl!.WriteText(outStream);

            return stl;
        }

        /// <summary>Reads the <see cref="STLDocument"/> within the <paramref name="inStream"/> as binary into the <paramref name="outStream"/>.</summary>
        /// <param name="inStream">The stream to read from.</param>
        /// <param name="outStream">The stream to read into.</param>
        /// <returns>The <see cref="STLDocument"/> that was copied.</returns>
        public static STLDocument CopyAsBinary(Stream inStream, Stream outStream)
        {
            STLDocument? stl = Read(inStream);

            stl!.WriteBinary(outStream);

            return stl;
        }

        /// <summary>Returns the header representation of this <see cref="STLDocument"/>.</summary>
        public override string ToString()
        {
            return $"solid {this.Name}";
        }

        /// <summary>Determines whether or not this instance is the same as the <paramref name="other"/> instance.</summary>
        /// <param name="other">The <see cref="STLDocument"/> to which to compare.</param>
        /// <returns>True if this instance is equal to the <paramref name="other"/> instance.</returns>
        public bool Equals(STLDocument? other)
        {
            return (this.Facets.Count == other!.Facets.Count
                    && this.Facets.All((i, o) => o.Equals(other.Facets[i])));
        }

        /// <summary>Iterates through the <see cref="Facets"/> collection.</summary>
        public IEnumerator<Facet> GetEnumerator()
        {
            return this.Facets.GetEnumerator();
        }

        /// <summary>Iterates through the <see cref="Facets"/> collection.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
