using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;

namespace SiteTree
{
    public partial class Form1 : Form
    {
        Regex HTMLRegex = new Regex(@"<a.*?href=[""'](?<url>[^""^']+[.]*?)[""'].*?>(?<keywords>[^<]+[.]*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public Form1()
        {
            InitializeComponent();
        }

        private void URLAnalyzeButton_Click(object sender, EventArgs e)
        {
            string baseURL = textBox1.Text;
            string[] userURL = new string[] { baseURL };
            List<string> visitedURLs = new List<string>();
            Dictionary<string, string> links = new Dictionary<string, string>();
            GViewer viewer = new GViewer();
            Graph graph = new Graph("graph");
            WebClient client = new WebClient();

            for (int depth = 1; depth <= numericUpDown1.Value; depth++)
            {
                List<string> newURLs = new List<string>();
                for (int domainNumber = 0; domainNumber < userURL.Length; domainNumber++)
                {
                    try
                    {
                        string htmlCode = client.DownloadString(userURL[domainNumber]);
                        AddMatches(htmlCode, baseURL, userURL[domainNumber], newURLs, visitedURLs, links, graph);
                    }
                    catch (Exception)
                    {

                    }
                }
                visitedURLs.AddRange(newURLs);
                userURL = newURLs.ToArray();
            }

            //graph.AddEdge("A", "C").Attr.Color = Microsoft.c.Drawing.Color.Green;
            //bind the graph to the viewer 
            viewer.CurrentLayoutMethod = LayoutMethod.MDS;
            viewer.Graph = graph;
            GraphForm graphForm = new GraphForm();
            viewer.Dock = DockStyle.Fill;
            //viewer.ToolBarIsVisible = false;
            //viewer.LayoutEditingEnabled = false;
            graphForm.Controls.Add(viewer);
            graphForm.ShowDialog();
        }

        private void FileAnalyzeBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter += "HTML|*.htm?";
            if (fileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string currentURL = fileDialog.SafeFileName;

            Dictionary<string, string> links = new Dictionary<string, string>();
            GViewer viewer = new GViewer();
            Graph graph = new Graph("graph");

            using (TextReader reader = new StreamReader(fileDialog.FileName))
            {
                string htmlCode = reader.ReadToEnd();
                List<string> newURLs = new List<string>();
                List<string> visitedURLs = new List<string>();
                AddMatches(htmlCode, "", currentURL, newURLs, visitedURLs, links, graph);
            }

            viewer.CurrentLayoutMethod = LayoutMethod.MDS;
            viewer.Graph = graph;
            GraphForm graphForm = new GraphForm();
            viewer.Dock = DockStyle.Fill;
            graphForm.Controls.Add(viewer);
            graphForm.ShowDialog();
        }

        private void AddMatches(string html, string baseURL, string currentURL, List<string> newURLs, List<string> visitedURLs, Dictionary<string, string> links, Graph graph)
        {
            MatchCollection matches = HTMLRegex.Matches(html);
            foreach (Match item in matches)
            {
                string url = item.Groups["url"].Value;
                if (!url.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (String.IsNullOrEmpty(baseURL))
                    {
                        continue;
                    }
                    else
                    {
                        url = baseURL + url;
                    }
                }
                int index = url.IndexOf('/', 9);
                if (index == -1)
                {
                    index = url.IndexOf('?', 9);
                }
                if (index == -1)
                {
                    index = url.IndexOf('#', 9);
                }
                if (!newURLs.Contains(url) && !url.EndsWith(".pdf") && !visitedURLs.Contains(url))
                {
                    newURLs.Add(url);
                }
                url = new String(url.ToCharArray(), 0, index == -1 ? url.Length : index);
                if (!links.ContainsKey(url))
                {
                    graph.AddEdge(currentURL, url);
                    links.Add(url, item.Groups["keywords"].Value);
                }
            }
        }
    }
}
