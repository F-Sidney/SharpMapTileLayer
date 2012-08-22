using System.Globalization;
using System.Linq;
using System.Xml.Linq;
//using ESRI.ArcGIS.Client.Geometry;
using System;
//using System.Windows.Media.Imaging;
using System.IO;
//using System.Windows.Media;
using SharpMap.Geometries;

namespace ESRI.ArcGIS.Client.Samples
{
	/// <summary>
	/// Pulls data directly from a locally stored arcgis server tile cache.
	/// </summary>
	/// <remarks>
	/// Supports both exploded and compact tile caches.
	/// To use, set the TileCacheFilePath property to point to the folder where the conf.cdi and conf.xml files resides.
	/// </remarks>
	public class LocalTileCacheLayer : ESRI.ArcGIS.Client.TiledMapServiceLayer
	{
		private string ImageExtension = "png";
		private int packetSize;
		private bool isCompact = false;

		/// <summary>
		/// Initializes the resource.
		/// </summary>
		/// <remarks>
		/// 	<para>Override this method if your resource requires asynchronous 
		/// 	requests to initialize,
		/// and call the base method when initialization is completed.</para>
		/// 	<para>Upon completion of initialization, check the 
		/// 	<see cref="ESRI.ArcGIS.Client.Layer.InitializationFailure"/> for
		/// 	any possible errors.</para>
		/// </remarks>
		/// <seealso cref="ESRI.ArcGIS.Client.Layer.Initialized"/>
		/// <seealso cref="ESRI.ArcGIS.Client.Layer.InitializationFailure"/>
		public override void Initialize()
		{
			string configFile = string.Format(@"{0}\conf.xml", TileCacheFilePath);
			string cdiFile = string.Format(@"{0}\conf.cdi", TileCacheFilePath);
			if (!System.IO.File.Exists(configFile))
			{
				InitializationFailure = new System.IO.FileNotFoundException("conf.xml not found in tilecache directory");
			}
			else if (!System.IO.File.Exists(cdiFile))
			{
				InitializationFailure = new System.IO.FileNotFoundException("conf.cdi not found in tilecache directory");
			}
			else
			{
				try
				{
					XDocument xDoc = XDocument.Load(configFile);

					var info = (from Service in xDoc.Descendants("TileCacheInfo")
								select new
								{
									SpatialReference = Service.Element("SpatialReference"),
									TileOrigin = Service.Element("TileOrigin"),
									TileCols = Service.Element("TileCols") == null ? 0 : int.Parse(Service.Element("TileCols").Value),
									TileRows = Service.Element("TileRows") == null ? 0 : int.Parse(Service.Element("TileRows").Value),
									LODInfos = Service.Element("LODInfos")
								}).First();
					MapPoint origin = new MapPoint(
						double.Parse(info.TileOrigin.Element("X").Value, CultureInfo.InvariantCulture),
						double.Parse(info.TileOrigin.Element("Y").Value, CultureInfo.InvariantCulture));
					if (info.SpatialReference.Element("WKID") != null)
					{
						int wkid = int.Parse(info.SpatialReference.Element("WKID").Value);
						this.SpatialReference = new SpatialReference(wkid);
					}
					else if (info.SpatialReference.Element("WKT") != null)
					{
						string wkt = info.SpatialReference.Element("WKT").Value;
						this.SpatialReference = new SpatialReference(wkt);
					}

					var lods = (from lod in info.LODInfos.Elements("LODInfo")
								select new Lod()
								{
									Resolution = lod.Element("Resolution") == null ? 0.0 : double.Parse(lod.Element("Resolution").Value),
								}
								);
					string imageFormat = (from Service in xDoc.Descendants("TileImageInfo")
										  select Service.Element("CacheTileFormat").Value).First();
					switch (imageFormat)
					{
						case "JPEG":
							imageFormat = "jpg"; break;
						case "GIF":
							imageFormat = "gif"; break;
						default:
							imageFormat = "png"; break;
					}
					this.ImageExtension = imageFormat;

					TileInfo tileinfo = new TileInfo()
					{
						Height = info.TileRows,
						Width = info.TileCols,
						Origin = origin,
						SpatialReference = this.SpatialReference,
						Lods = lods.ToArray()
					};
					this.TileInfo = tileinfo;

					//Check cache format
					try
					{
						string cacheStorageInfo = (from Service in xDoc.Descendants("CacheStorageInfo")
												   select (Service.Element("StorageFormat") == null ? "" :
												   Service.Element("StorageFormat").Value)).First();
						if (cacheStorageInfo == "esriMapCacheStorageModeCompact")
						{
							packetSize = int.Parse((from Service in xDoc.Descendants("CacheStorageInfo")
														select (Service.Element("PacketSize") == null ? "" :
													   Service.Element("PacketSize").Value)).First());
							isCompact = true;
						}
					}
					catch { }

					xDoc = XDocument.Load(cdiFile);
					var fullExtent = (from a in xDoc.Descendants("EnvelopeN")
									  select new Envelope
									  (
											a.Element("XMin") == null ? double.NaN : double.Parse(a.Element("XMin").Value),
											a.Element("YMin") == null ? double.NaN : double.Parse(a.Element("YMin").Value),
											a.Element("XMax") == null ? double.NaN : double.Parse(a.Element("XMax").Value),
											a.Element("YMax") == null ? double.NaN : double.Parse(a.Element("YMax").Value)
									  ) { SpatialReference = this.SpatialReference }).First();

					this.FullExtent = fullExtent;
				}
				catch (System.Exception ex)
				{
					this.InitializationFailure = new System.Exception("Couldn't parse cache information", ex);
				}
			}

			base.Initialize();
		}

		/// <summary>
		/// Returns a URL to the specified tile
		/// </summary>
		/// <param name="level">Layer level</param>
		/// <param name="row">Tile row</param>
		/// <param name="col">Tile column</param>
		/// <returns>URL to the tile image</returns>
		public override string GetTileUrl(int level, int row, int col)
		{
			string path = string.Format(@"{0}\_alllayers\L{1:d2}\R{2:x8}\C{3:x8}.{4}",
				TileCacheFilePath, level, row, col, ImageExtension);
			if (System.IO.File.Exists(path))
				return path;
			return null;
		}

		protected override void GetTileSource(int level, int row, int col, Action<ImageSource> onComplete)
		{
			if (!isCompact)
			{
				base.GetTileSource(level, row, col, onComplete);
			}
			else
			{
				GetCompactTile(level, row, col, onComplete);
			}
		}

		/// <summary>
		/// Gets a tile from a compact cache.
		/// </summary>
		/// <param name="level">The level.</param>
		/// <param name="row">The row.</param>
		/// <param name="col">The col.</param>
		/// <param name="onComplete">The on complete.</param>
		private void GetCompactTile(int level, int row, int col, Action<ImageSource> onComplete)
		{
			Action<byte[]> complete = delegate(byte[] tileData)
			{
				if (tileData == null)
					onComplete(null);
				else
				{
					BitmapImage bmi = new BitmapImage();
					bmi.BeginInit();
					MemoryStream ms = new MemoryStream(tileData);
					bmi.StreamSource = ms;
					bmi.EndInit();
					onComplete(bmi);
				}
			};
			System.Threading.ThreadPool.QueueUserWorkItem(delegate(object state)
			{
				Action<byte[]> callback = (Action<byte[]>)state;
				var tileData = CacheSource.GetTile(level, row, col, packetSize, TileCacheFilePath);
				Dispatcher.BeginInvoke((Action)delegate() { callback(tileData); });
			}, complete);
		}

		/// <summary>
		/// Gets or sets the local file path to the root of the tile cache.
		/// This should be where the conf.xml and conf.cdi files reside.
		/// </summary>
		/// <value>The tile cache file path.</value>
		public string TileCacheFilePath { get; set; }

		/// <summary>
		/// Reads tile data from a compact cache source
		/// </summary>
		private static class CacheSource
		{
			private const int RECORD_SIZE = 5;

			public static byte[] GetTile(int level, int row, int column, int packetSize, string basePath)
			{
				long rowIndex = (row / packetSize) * packetSize;
				long colIndex = (column / packetSize) * packetSize;
				string filepath = string.Format(@"{0}\_alllayers\L{1:d2}\R{2:x4}C{3:x4}", basePath, level, rowIndex, colIndex);
				string bundlxFilename = string.Format("{0}.bundlx", filepath);
				string bundleFilename = string.Format("{0}.bundle", filepath);
				if (!File.Exists(bundlxFilename) || !File.Exists(bundleFilename))
					return null;

				try
				{
					long offsetX = getBundlxOffset(level, row, column, packetSize);
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
						return imgData;
					}
				}
				catch
				{
					return null;
				}
			}

			private static long getBundlxOffset(int level, long row, long column, int packetSize)
			{
				long tileStartRow = (row / packetSize) * packetSize;
				long tileStartCol = (column / packetSize) * packetSize;
				long recordNumber = (((packetSize * (column - tileStartCol)) + (row - tileStartRow)));
				if (recordNumber < 0)
					throw new ArgumentException("Invalid level / row / col");
				long offset = 16 + (recordNumber * RECORD_SIZE);
				return offset;
			}
		}
	}
}