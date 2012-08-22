using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BruTile;
using BruTile.Cache;
using System.IO;
using System.Globalization;

namespace SharpMapTest
{
    internal class ArcGISTileProvider : ITileProvider
{
    // Fields
    private readonly MemoryCache<byte[]> _fileCache;
    private readonly string _format = "png";
    private string url = @"C:\arcgisserver\arcgiscache\GZMap\Layers\_alllayers";

    // Methods
    public ArcGISTileProvider(string FilePath)
    {
        this.url = Path.GetDirectoryName(FilePath) + @"\_alllayers";
        this._fileCache = new MemoryCache<byte[]>(50, 80);
    }

    private static string ColumnToHex(int x)
    {
        return ("C" + string.Format("{0:x8}", x));
    }

    public byte[] GetTile(TileInfo tileInfo)
    {
        byte[] buffer = this._fileCache.Find(tileInfo.Index);
        if (buffer == null)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(CultureInfo.InvariantCulture, @"{0}\{1}\{2}\{3}.{4}", new object[] { this.url, LevelToHex(tileInfo.Index.LevelId), RowToHex(tileInfo.Index.Row), ColumnToHex(tileInfo.Index.Col), this._format });
            if (File.Exists(builder.ToString()))
            {
                FileStream stream = File.OpenRead(builder.ToString());
                buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
            }
            if (buffer != null)
            {
                this._fileCache.Add(tileInfo.Index, buffer);
            }
        }
        return buffer;
    }

    private static string LevelToHex(string zoomLevelId)
    {
        int num = int.Parse(zoomLevelId);
        if (num < 10)
        {
            return ("L0" + num);
        }
        return ("L" + num);
    }

    private static string RowToHex(int y)
    {
        return ("R" + string.Format("{0:x8}", y));
    }
}
 

}
