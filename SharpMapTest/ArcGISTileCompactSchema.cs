using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using BruTile;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using SharpMap.Geometries;

namespace SharpMapTest
{
    public class ArcGISTileCompactSchema : TileSchema
{
    // Fields
    private string url;

    // Methods
    public ArcGISTileCompactSchema(string FilePath)
    {
        Resolution resolution2;
        this.url = @"C:\arcgisserver\arcgiscache\GZMap\Layers\conf.xml";
        this.url = FilePath;
        string cdiFile = string.Format(@"{0}\conf.cdi", Path.GetDirectoryName(FilePath));
        XmlDocument document = new XmlDocument();
        document.Load(this.url);
        XmlNode documentElement = document.DocumentElement;
        XmlNodeList elementsByTagName = document.GetElementsByTagName("LODInfo");
        List<LODInfo> list2 = new List<LODInfo>();
        foreach (XmlNode node2 in elementsByTagName)
        {
            string innerText = node2["LevelID"].InnerText;
            string str2 = node2["Scale"].InnerText;
            string str3 = node2["Resolution"].InnerText;
            LODInfo item = new LODInfo();
            item.LevelID = Convert.ToInt32(innerText);
            item.Scale = Convert.ToDouble(str2);
            item.Resolution = Convert.ToDouble(str3);
            list2.Add(item);
        }
        foreach (LODInfo info2 in list2)
        {
            resolution2 = new Resolution();
            Resolution resolution = resolution2;
            resolution.Id = info2.LevelID.ToString();
            resolution.UnitsPerPixel = info2.Resolution;
            base.Resolutions.Add(resolution);
        }
        base.Height = 0x100;
        base.Width = 0x100;
        base.Extent = new Extent(-180.0, -90.0, 180.0, 90.0);
        XmlNodeList list3 = document.GetElementsByTagName("SpatialReference");
        base.OriginX = Convert.ToDouble(list3[0]["XOrigin"].InnerText);
        base.OriginY = Convert.ToDouble(list3[0]["YOrigin"].InnerText);
        list3 = document.GetElementsByTagName("TileCols");
        base.Width = Convert.ToInt32(list3[0].InnerText);
        list3 = document.GetElementsByTagName("TileRows");
        base.Height = Convert.ToInt32(list3[0].InnerText);
        base.Axis = AxisDirection.InvertedY;
        list3 = document.GetElementsByTagName("TileOrigin");
        base.OriginX = Convert.ToDouble(list3[0].FirstChild.InnerText);
        base.OriginY = Convert.ToDouble(list3[0].LastChild.InnerText);
        base.Name = "ArcGISTileCache";
        base.Format = "png";
        base.Srs = "UnKnown";
        DirectoryInfo info3 = new DirectoryInfo(Path.GetDirectoryName(FilePath) + @"\_alllayers\L00");
        BoundingBox box = null;
        //foreach (FileInfo infoBundle in info3.GetFiles())
        //{            
        //    if (infoBundle.FullName.EndsWith("bundle"))
        //    {
        //        BoundingBox box2;
        //        //int row = RowToHex(infoBundle.Name.Substring(1, 4));
        //        int row = RowToHex(infoBundle.Name);
        //        int level = 0;
        //        //int col = ColumnToHex(infoBundle.Name.Substring(6, 4));
        //        int col = ColumnToHex(infoBundle.Name);
        //        TileInfo infoTile = new TileInfo();
        //        infoTile.Extent = this.TileToWorld(new TileRange(col, row), level, this);
        //        resolution2 = base.Resolutions[level];
        //        infoTile.Index = new TileIndex(col, row, resolution2.Id);
        //        try
        //        {
        //            box2 = new BoundingBox(infoTile.Extent.MinX, infoTile.Extent.MinY, infoTile.Extent.MaxX, infoTile.Extent.MaxY);
        //        }
        //        catch (Exception)
        //        {
        //            box2 = new BoundingBox(-180,-90,180,90);
        //        }
        //        if (box2!=null)
        //        {
        //            box = (box == null) ? box2 : box.Join(box2);
        //        }
        //    }
        //}
        //foreach (DirectoryInfo info4 in info3.GetDirectories())
        //{
        //    int row = RowToHex(info4.Name);
        //    int level = 0;
        //    foreach (FileInfo info5 in info4.GetFiles())
        //    {
        //        if (info5.FullName.EndsWith(base.Format))
        //        {
        //            BoundingBox box2;
        //            int col = ColumnToHex(info5.Name);
        //            TileInfo info6 = new TileInfo();
        //            info6.Extent = this.TileToWorld(new TileRange(col, row), level, this);
        //            resolution2 = base.Resolutions[level];
        //            info6.Index = new TileIndex(col, row, resolution2.Id);
        //            try
        //            {
        //                box2 = new BoundingBox(info6.Extent.MinX, info6.Extent.MinY, info6.Extent.MaxX, info6.Extent.MaxY);
        //            }
        //            catch (Exception)
        //            {
        //                box2 = new BoundingBox(-180.0, -90.0, 180.0, 90.0);
        //            }
        //            if (box2 != null)
        //            {
        //                box = (box == null) ? box2 : box.Join(box2);
        //            }
        //        }
        //    }
        //}
        //if (box != null)
        //{
        //    base.Extent = new Extent(box.Min.X, box.Min.Y, box.Max.X, box.Max.Y);
        //}
        try
        {
            XDocument xDoc = XDocument.Load(cdiFile);

            var fullExtent = (from a in xDoc.Descendants("EnvelopeN")
                              select new Extent
                              (
                                    a.Element("XMin") == null ? double.NaN : double.Parse(a.Element("XMin").Value),
                                    a.Element("YMin") == null ? double.NaN : double.Parse(a.Element("YMin").Value),
                                    a.Element("XMax") == null ? double.NaN : double.Parse(a.Element("XMax").Value),
                                    a.Element("YMax") == null ? double.NaN : double.Parse(a.Element("YMax").Value)
                              )).First();
            //{ SpatialReference = this.SpatialReference }).First();

            base.Extent = fullExtent;
            
        }
        catch (Exception ex)
        {
            
        }
        //if (box != null)
        //{
        //    base.Extent = new Extent(box.Min.X, box.Min.Y, box.Max.X, box.Max.Y);
        //}
    }

    private static int ColumnToHex(string Column)
    {
        return Convert.ToInt32(Column.Substring(Column.IndexOf("C")+1, Column.IndexOf(".")-Column.IndexOf("C")-1), 0x10);
    }

    private static int RowToHex(string Row)
    {
        //return Convert.ToInt32(Row.Substring(1, 8), 0x10);
        
        return Convert.ToInt32(Row.Substring(1, Row.IndexOf("C")-1), 0x10);
    }

    private Extent TileToWorld(TileRange range, int level, ITileSchema schema)
    {
        Resolution resolution = schema.Resolutions[level];
        double num = resolution.UnitsPerPixel * schema.Width;
        double minX = (range.FirstCol * num) + schema.OriginX;
        double minY = (-(range.FirstRow + range.RowCount + 1) * num) + schema.OriginY;
        double maxX = ((range.FirstCol + range.ColCount + 1) * num) + schema.OriginX;
        return new Extent(minX, minY, maxX, (-range.FirstRow * num) + schema.OriginY);
    }

    public new IList<TileInfo> GetTilesInView(Extent extent, int level)
    {
        IList<TileInfo> infos = new List<TileInfo>();
        TileRange range = TileTransform.WorldToTile(extent, level, this);
        infos.Clear();
        for (int x = range.FirstCol; x < (range.FirstCol + range.ColCount); x++)
        {
            for (int y = range.FirstRow; y < (range.FirstRow + range.RowCount); y++)
            {
                TileInfo info = new TileInfo();
                info.Extent = TileTransform.TileToWorld(new TileRange(x, y), level, this);
                Resolution res = this.Resolutions[level];
                info.Index = new TileIndex(x, y, res.Id);
                if (WithinSchemaExtent(this.Extent, info.Extent))
                {
                    infos.Add(info);
                }
            }
        }
        return infos;
    }

    private static bool WithinSchemaExtent(Extent schemaExtent, Extent tileExtent)
    {
        if (!tileExtent.Intersects(schemaExtent))
        {
            return false;
        }
        return ((tileExtent.Intersect(schemaExtent).Area / tileExtent.Area) > 0.001);
    }

    // Nested Types
    [StructLayout(LayoutKind.Sequential)]
    private struct LODInfo
    {
        internal int LevelID;
        internal double Scale;
        internal double Resolution;
    }

    //[StructLayout(LayoutKind.Sequential)]
    //public struct TileRange
    //{
    //    [CompilerGenerated]
    //    private int firstCol;
    //    [CompilerGenerated]
    //    private int lastCol;
    //    [CompilerGenerated]
    //    private int firstRow;
    //    [CompilerGenerated]
    //    private int lastRow;
    //    public int FirstCol
    //    {
    //        get
    //        {
    //            return this.firstCol;
    //        }
    //        set
    //        {
    //            this.firstCol = value;
    //        }
    //    }
    //    public int LastCol
    //    {
    //        get
    //        {
    //            return this.lastCol;
    //        }
    //        set
    //        {
    //            this.lastCol = value;
    //        }
    //    }
    //    public int FirstRow
    //    {
    //        get
    //        {
    //            return this.firstRow;
    //        }
    //        set
    //        {
    //            this.firstRow = value;
    //        }
    //    }
    //    public int LastRow
    //    {
    //        get
    //        {
    //            return this.lastRow;
    //        }
    //        set
    //        {
    //            this.lastRow = value;
    //        }
    //    }
    //    public TileRange(int col, int row) : this(col, row, col, row)
    //    {
    //    }

    //    public TileRange(int firstCol, int firstRow, int lastCol, int lastRow)
    //    {
    //        this = new ArcGISTileCompactSchema.TileRange();
    //        this.FirstCol = firstCol;
    //        this.LastCol = lastCol;
    //        this.FirstRow = firstRow;
    //        this.LastRow = lastRow;
    //    }

    //    public override bool Equals(object obj)
    //    {
    //        return ((obj is ArcGISTileCompactSchema.TileRange) && this.Equals((ArcGISTileCompactSchema.TileRange) obj));
    //    }

    //    public bool Equals(ArcGISTileCompactSchema.TileRange tileRange)
    //    {
    //        return ((((this.FirstCol == tileRange.FirstCol) && (this.LastCol == tileRange.LastCol)) && (this.FirstRow == tileRange.FirstRow)) && (this.LastRow == tileRange.LastRow));
    //    }

    //    public override int GetHashCode()
    //    {
    //        return (((this.FirstCol ^ this.LastCol) ^ this.FirstRow) ^ this.LastRow);
    //    }

    //    public static bool operator ==(ArcGISTileCompactSchema.TileRange tileRange1, ArcGISTileCompactSchema.TileRange tileRange2)
    //    {
    //        return object.Equals(tileRange1, tileRange2);
    //    }

    //    public static bool operator !=(ArcGISTileCompactSchema.TileRange tileRange1, ArcGISTileCompactSchema.TileRange tileRange2)
    //    {
    //        return !object.Equals(tileRange1, tileRange2);
    //    }
    //}
}
}
