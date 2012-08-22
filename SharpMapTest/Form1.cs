using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpMap.Layers;

namespace SharpMapTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string filePath = ofd.FileName;
                ArcGISTileSource suyTile = new ArcGISTileSource(filePath);
                TileLayer suyTileLyr = new TileLayer(suyTile, "suy");
                //ArcGISTileCompactSource suyTile = new ArcGISTileCompactSource(filePath);
                //ArcGISTileCompactLayer suyTileLyr = new ArcGISTileCompactLayer(suyTile, "suy");
                this.mapImage1.Map.Layers.Add(suyTileLyr);
                this.mapImage1.Map.ZoomToExtents();
                this.mapImage1.Refresh();
                //SharpMap.Data.Providers.SpatiaLite sb = new SharpMap.Data.Providers.SpatiaLite("Data Source=" + ofd.FileName, "water", "Geometry", "PK_UID");
                //VectorLayer vecLyr = new VectorLayer("sqlSpatial");
                //vecLyr.DataSource = sb;
                //this.mapImage1.Map.Layers.Add(vecLyr);
                //this.mapImage1.Map.ZoomToExtents();
                //this.mapImage1.Refresh();

            }
            //BruTile.ITileProvider tileProvider = 
            ////BruTile.TileSource ts = new BruTile.TileSource(
            //this.mapImage1.Map.Layers.Add();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string filePath = ofd.FileName;

                ArcGISTileCompactSource suyTile = new ArcGISTileCompactSource(filePath);
                ArcGISTileCompactLayer suyTileLyr = new ArcGISTileCompactLayer(suyTile, "suy");
                this.mapImage1.Map.Layers.Add(suyTileLyr);
                this.mapImage1.Map.ZoomToExtents();
                this.mapImage1.Refresh();
                //SharpMap.Data.Providers.SpatiaLite sb = new SharpMap.Data.Providers.SpatiaLite("Data Source=" + ofd.FileName, "water", "Geometry", "PK_UID");
                //VectorLayer vecLyr = new VectorLayer("sqlSpatial");
                //vecLyr.DataSource = sb;
                //this.mapImage1.Map.Layers.Add(vecLyr);
                //this.mapImage1.Map.ZoomToExtents();
                //this.mapImage1.Refresh();

            }
        }
    }
}
