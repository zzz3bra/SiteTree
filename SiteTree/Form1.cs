using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout;
using Microsoft.Msagl.GraphViewerGdi;
using System.Text.RegularExpressions;
using System.IO;

namespace SiteTree
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Regex regex = new Regex(@"<a\s+href=(?:""([^ ""]+)"" | '([^'] +)').*?>(.*?)</a>");
            Regex regex = new Regex(@"<a.*?href=[""'](?<url>[^""^']+[.]*?)[""'].*?>(?<keywords>[^<]+[.]*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            //Regex siteregex = new Regex(@"/^(?:https?:\/\/)?(?:[^@\/\n]+@)?(?:www\.)?([^:\/\n]+)/im");
            Regex siteregex = new Regex(@"/ (?: https ?:\/\/)?(?: www\.)?(.*?)\//");
            TextReader reader = new StreamReader(@"..\..\samples\1.html");
            string html = reader.ReadToEnd();
            MatchCollection matches = regex.Matches(html);
            Dictionary<string, string> links = new Dictionary<string, string>();
            foreach (Match item in matches)
            {
                string url = item.Groups["url"].Value;
                if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                int index = url.IndexOf('/', 9);
                if (index == -1)
                {
                    index = url.IndexOf('?', 9);
                }
                url = new String(url.ToCharArray(), 0, index == -1 ? url.Length : index);
                //Match site = siteregex.Match(url);
                if (!links.ContainsKey(url))
                {
                    links.Add(url, item.Groups["keywords"].Value);
                }
            }
            GViewer viewer = new GViewer();
            //create a graph object 
            Graph graph = new Graph("graph");
            //create the graph content 
            foreach (var item in links)
            {
                graph.AddEdge("habr", item.Key);
            }
            //graph.AddEdge("A", "B");
            //graph.AddEdge("B", "C");
            //graph.AddEdge("A", "C").Attr.Color = Microsoft.Msagl.Drawing.Color.Green;
            //graph.FindNode("A").Attr.FillColor = Microsoft.Msagl.Drawing.Color.Magenta;
            //graph.FindNode("B").Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
            //Node c = graph.FindNode("C");
            //c.Attr.FillColor = Microsoft.Msagl.Drawing.Color.PaleGreen;
            //c.Attr.Shape = Shape.Diamond;
            //bind the graph to the viewer 
            viewer.CurrentLayoutMethod = LayoutMethod.MDS;
            viewer.Graph = graph;
            //associate the viewer with the form 
            SuspendLayout();
            viewer.Dock = DockStyle.Fill;
            //viewer.ToolBarIsVisible = false;
            //viewer.LayoutEditingEnabled = false;
            Controls.Add(viewer);
            viewer.Refresh();
            ResumeLayout();
            //show the form 
        }
    }
}
