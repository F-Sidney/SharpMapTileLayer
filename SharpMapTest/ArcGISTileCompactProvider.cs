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
    internal class ArcGISTileCompactProvider : ITileProvider
{
    // Fields
    private readonly MemoryCache<byte[]> _fileCache;
    private readonly string _format = "png";
    private string url = @"C:\arcgisserver\arcgiscache\GZMap\Layers\_alllayers";

    private int packetSize=128;
    private bool isCompact = true;
    private const int RECORD_SIZE = 5;
    // Methods
    public ArcGISTileCompactProvider(string FilePath)
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
        byte[] bufferTile = this._fileCache.Find(tileInfo.Index);
        if (bufferTile == null)
        {
            long rowIndex = (tileInfo.Index.Row / packetSize) * packetSize;
            long colIndex = (tileInfo.Index.Col / packetSize) * packetSize;
            string filepath = string.Format(@"{0}\L{1:d2}\R{2:x4}C{3:x4}", this.url,int.Parse(tileInfo.Index.LevelId), rowIndex, colIndex);
            string bundlxFilename = string.Format("{0}.bundlx", filepath);
            string bundleFilename = string.Format("{0}.bundle", filepath);

            if (!File.Exists(bundlxFilename) || !File.Exists(bundleFilename))
                return null;

            try
            {
                long offsetX = getBundlxOffset(int.Parse( tileInfo.Index.LevelId), tileInfo.Index.Row, tileInfo.Index.Col, packetSize);
                byte[] idxData = new byte[5];
                using (Stream bundlx = System.IO.File.OpenRead(bundlxFilename))
                {
                    bundlx.Seek(offsetX, SeekOrigin.Begin);
                    bundlx.Read(idxData, 0, 5);
                }

                var bundleOffset = ((idxData[4] & 0xFF) << 32) | ((idxData[3] & 0xFF) << 24) |
                    ((idxData[2] & 0xFF) << 16) | ((idxData[1] & 0xFF) << 8) | ((idxData[0] & 0xFF));

                using (Stream bundle = System.IO.File.OpenRead(bundleFilename))
                {
                    bundle.Seek(bundleOffset, SeekOrigin.Begin);
                    byte[] buffer = new byte[4];
                    bundle.Read(buffer, 0, 4);
                    int recordLen = ((buffer[3] & 0xFF) << 24) | ((buffer[2] & 0xFF) << 16) | ((buffer[1] & 0xFF) << 8) | ((buffer[0] & 0xFF));
                    byte[] imgData = new byte[recordLen];
                    bundle.Read(imgData, 0, recordLen);
                    bufferTile = imgData;
                }
            }
            catch
            {
                return null;
            }

        //    StringBuilder builder = new StringBuilder();
        //    builder.AppendFormat(CultureInfo.InvariantCulture, @"{0}\{1}\{2}\{3}.{4}", new object[] { this.url, LevelToHex(tileInfo.Index.LevelId), RowToHex(tileInfo.Index.Row), ColumnToHex(tileInfo.Index.Col), this._format });
        //    if (File.Exists(builder.ToString()))
        //    {
        //        FileStream stream = File.OpenRead(builder.ToString());
        //        buffer = new byte[stream.Length];
        //        stream.Read(buffer, 0, buffer.Length);
        //    }
            if (bufferTile != null)
            {
                this._fileCache.Add(tileInfo.Index, bufferTile);
            }
        }
        return bufferTile;
    }

    private static long getBundlxOffset(int level, int row, int column, int packetSize)
    {
        long tileStartRow = (row / packetSize) * packetSize;
        long tileStartCol = (column / packetSize) * packetSize;
        long recordNumber = (((packetSize * (column - tileStartCol)) + (row - tileStartRow)));
        if (recordNumber < 0)
            throw new ArgumentException("Invalid level / row / col");
        long offset = 16 + (recordNumber * RECORD_SIZE);
        return offset;
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
