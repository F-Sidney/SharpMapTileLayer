using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BruTile;
using System.Runtime.CompilerServices;

namespace SharpMapTest
{
    public class ArcGISTileCompactSource : ITileSource
    {
        // Fields
        //[CompilerGenerated]        
        private ITileProvider provider;
        //[CompilerGenerated]
        private ITileSchema schema;

        // Methods
        public ArcGISTileCompactSource(string Url)
        {
            this.provider = new ArcGISTileCompactProvider(Url);
            this.schema = new ArcGISTileCompactSchema(Url);
        }

        public ITileProvider Provider
        {
            get { return this.provider; }
        }

        public ITileSchema Schema
        {
            get { return this.schema; }
        }
    }
}
