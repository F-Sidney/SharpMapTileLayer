using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BruTile;
using System.Runtime.CompilerServices;

namespace SharpMapTest
{
    public class ArcGISTileSource : ITileSource
{
    // Fields
    //[CompilerGenerated]        
    private ITileProvider provider;
    //[CompilerGenerated]
    private ITileSchema schema;

    // Methods
    public ArcGISTileSource(string Url)
    {
        this.provider = new ArcGISTileProvider(Url);
        this.schema = new ArcGISTileSchema(Url);
    }

    //// Properties
    //public ITileProvider Provider
    //{
    //    [CompilerGenerated]
    //    get
    //    {
    //        return this.<Provider>k__BackingField;
    //    }
    //    [CompilerGenerated]
    //    private set
    //    {
    //        this.<Provider>k__BackingField = value;
    //    }
    //}

    //public ITileSchema Schema
    //{
    //    [CompilerGenerated]
    //    get
    //    {
    //        return this.<Schema>k__BackingField;
    //    }
    //    [CompilerGenerated]
    //    private set
    //    {
    //        this.<Schema>k__BackingField = value;
    //    }
    //}

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
