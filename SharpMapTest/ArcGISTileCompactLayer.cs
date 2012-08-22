using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BruTile.Cache;
using System.Drawing.Imaging;
using BruTile;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Net;
using System.Drawing.Drawing2D;
using SharpMap.Geometries;
using SharpMap;

namespace SharpMapTest
{
    public class ArcGISTileCompactLayer : SharpMap.Layers.TileLayer
    {
        // Fields
        //protected readonly MemoryCache<Bitmap> _bitmaps;
        //protected FileCache _fileCache;
        //protected ImageAttributes _imageAttributes;
        //protected ImageFormat _ImageFormat;
        //protected readonly bool _showErrorInTile;
        //protected readonly ITileSource _source;

        // Methods
        public ArcGISTileCompactLayer(ITileSource tileSource, string layerName)
            : this(tileSource, layerName, new Color(), true, null)
        {
        }

        public ArcGISTileCompactLayer(ITileSource tileSource, string layerName, Color transparentColor, bool showErrorInTile)
            : this(tileSource, layerName, transparentColor, showErrorInTile, null)
        {
        }

        public ArcGISTileCompactLayer(ITileSource tileSource, string layerName, Color transparentColor, bool showErrorInTile, string fileCacheDir)
            :base(tileSource,layerName,transparentColor,showErrorInTile)
        {
            //this._imageAttributes = new ImageAttributes();
            //this._bitmaps = new MemoryCache<Bitmap>(100, 200);
            //this._showErrorInTile = true;
            //this._source = tileSource;
            //base.LayerName = layerName;
            //if (!transparentColor.IsEmpty)
            //{
            //    this._imageAttributes.SetColorKey(transparentColor, transparentColor);
            //}
            //this._showErrorInTile = showErrorInTile;
            //if (!string.IsNullOrEmpty(fileCacheDir))
            //{
            //    this._fileCache = new FileCache(fileCacheDir, "png");
            //    this._ImageFormat = ImageFormat.Png;
            //}
        }

        public ArcGISTileCompactLayer(ITileSource tileSource, string layerName, Color transparentColor, bool showErrorInTile, FileCache fileCache, ImageFormat imgFormat)
            :base(tileSource,layerName,transparentColor,showErrorInTile,fileCache,imgFormat)
        {
            //this._imageAttributes = new ImageAttributes();
            //this._bitmaps = new MemoryCache<Bitmap>(100, 200);
            //this._showErrorInTile = true;
            //this._source = tileSource;
            //base.LayerName = layerName;
            //if (!transparentColor.IsEmpty)
            //{
            //    this._imageAttributes.SetColorKey(transparentColor, transparentColor);
            //}
            //this._showErrorInTile = showErrorInTile;
            //this._fileCache = fileCache;
            //this._ImageFormat = imgFormat;
        }

        protected void AddImageToFileCache(TileInfo tileInfo, Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, this._ImageFormat);
            ms.Seek(0L, SeekOrigin.Begin);
            byte[] data = new byte[ms.Length];
            ms.Read(data, 0, data.Length);
            ms.Dispose();
            this._fileCache.Add(tileInfo.Index, data);
        }

        public void Dispose()
        {
            if (this._bitmaps != null)
            {
                this._bitmaps.Dispose();
            }
            if (this._imageAttributes != null)
            {
                this._imageAttributes.Dispose();
            }
        }

        protected Image GetImageFromFileCache(TileInfo info)
        {
            MemoryStream ms = new MemoryStream(this._fileCache.Find(info.Index));
            Image img = Image.FromStream(ms);
            ms.Dispose();
            return img;
        }

        private void GetTileOnThread(object parameter)
        {
            object[] parameters = (object[])parameter;
            if (parameters.Length != 4)
            {
                throw new ArgumentException("Three parameters expected");
            }
            ITileProvider tileProvider = (ITileProvider)parameters[0];
            TileInfo tileInfo = (TileInfo)parameters[1];
            MemoryCache<Bitmap> bitmaps = (MemoryCache<Bitmap>)parameters[2];
            AutoResetEvent autoResetEvent = (AutoResetEvent)parameters[3];
            try
            {
                Bitmap bitmap = new Bitmap(new MemoryStream(tileProvider.GetTile(tileInfo)));
                bitmaps.Add(tileInfo.Index, bitmap);
                if (this._fileCache != null)
                {
                    this.AddImageToFileCache(tileInfo, bitmap);
                }
            }
            catch (WebException ex)
            {
                if (this._showErrorInTile)
                {
                    Bitmap bitmap = new Bitmap(this._source.Schema.Width, this._source.Schema.Height);
                    Graphics.FromImage(bitmap).DrawString(ex.Message, new Font(FontFamily.GenericSansSerif, 12f), new SolidBrush(Color.Black), new RectangleF(0f, 0f, (float)this._source.Schema.Width, (float)this._source.Schema.Height));
                    bitmaps.Add(tileInfo.Index, bitmap);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                autoResetEvent.Set();
            }
        }

        public override void Render(Graphics graphics, Map map)
        {
            if ((!map.Size.IsEmpty && (map.Size.Width > 0)) && (map.Size.Height > 0))
            {
                Bitmap bmp = new Bitmap(map.Size.Width, map.Size.Height, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(bmp);
                g.Transform = graphics.Transform.Clone();
                Extent extent = new Extent(map.Envelope.Min.X, map.Envelope.Min.Y, map.Envelope.Max.X, map.Envelope.Max.Y);
                int level = Utilities.GetNearestLevel(this._source.Schema.Resolutions, map.PixelSize);
                //IList<TileInfo> tiles = this._source.Schema.GetTilesInView(extent, level);
                IList<TileInfo> tiles = (this._source.Schema as ArcGISTileCompactSchema).GetTilesInView(extent, level);
                IList<WaitHandle> waitHandles = new List<WaitHandle>();
                foreach (TileInfo info in tiles)
                {
                    if (this._bitmaps.Find(info.Index) == null)
                    {
                        if ((this._fileCache != null) && this._fileCache.Exists(info.Index))
                        {
                            this._bitmaps.Add(info.Index, this.GetImageFromFileCache(info) as Bitmap);
                        }
                        else
                        {
                            AutoResetEvent waitHandle = new AutoResetEvent(false);
                            waitHandles.Add(waitHandle);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(this.GetTileOnThread), new object[] { this._source.Provider, info, this._bitmaps, waitHandle });
                        }
                    }
                }
                foreach (WaitHandle handle in waitHandles)
                {
                    handle.WaitOne();
                }
                foreach (TileInfo info in tiles)
                {
                    Bitmap bitmap = this._bitmaps.Find(info.Index);
                    if (bitmap != null)
                    {
                        PointF min = map.WorldToImage(new SharpMap.Geometries.Point(info.Extent.MinX, info.Extent.MinY));
                        PointF max = map.WorldToImage(new SharpMap.Geometries.Point(info.Extent.MaxX, info.Extent.MaxY));
                        min = new PointF((float)Math.Round((double)min.X), (float)Math.Round((double)min.Y));
                        max = new PointF((float)Math.Round((double)max.X), (float)Math.Round((double)max.Y));
                        try
                        {
                            g.DrawImage(bitmap, new Rectangle((int)min.X, (int)max.Y, (int)(max.X - min.X), (int)(min.Y - max.Y)), 0, 0, this._source.Schema.Width, this._source.Schema.Height, GraphicsUnit.Pixel, this._imageAttributes);
                            continue;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
                graphics.Transform = new Matrix();
                graphics.DrawImageUnscaled(bmp, 0, 0);
                graphics.Transform = g.Transform;
                g.Dispose();
            }
        }

        //public void SetOpacity(float opacity)
        //{
        //    ColorMatrix cmxPic = new ColorMatrix();
        //    cmxPic.Matrix33 = opacity;
        //    ImageAttributes attrs = this._imageAttributes;
        //    if (attrs == null)
        //    {
        //        attrs = new ImageAttributes();
        //    }
        //    attrs.SetColorMatrix(cmxPic, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        //    this._imageAttributes = attrs;
        //}

        // Properties
        public override BoundingBox Envelope
        {
            get
            {
                return new BoundingBox(this._source.Schema.Extent.MinX, this._source.Schema.Extent.MinY, this._source.Schema.Extent.MaxX, this._source.Schema.Extent.MaxY);
            }
        }

        //public InterpolationMode InterpolationMode
        //{
        //    get
        //    {
        //        return this._interpolationMode;
        //    }
        //    set
        //    {
        //        this._interpolationMode = value;
        //    }
        //}
    }
}