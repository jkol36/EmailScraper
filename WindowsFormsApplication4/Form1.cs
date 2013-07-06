using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            label1.Text = "Page count = 0";
            label2.Text = "Email count = 0";
            label3.Text = "In stack = 0";
            label4.Text = "Found now = 0";
            label5.Text = "Depth = 0";
            linkLabel1.Text = "";
        }

        public ISet<Uri> GetNewLinks(string content, Uri sender)
        {
            Regex regexLink = new Regex("(?<=<a\\s*?href=(?:'|\"))[^'\"]*?(?=(?:'|\"))");

            ISet<Uri> newLinks = new HashSet<Uri>();
            Uri s;
            foreach (var match in regexLink.Matches(content))
            {
                s = new Uri(sender, match.ToString());
                if (!newLinks.Contains(s))
                    newLinks.Add(s);

            }

            return newLinks;
        }
        int pageCount = 0;
        int emailCount = 0;
        // 
        public ISet<string> GetNewEmails(string content)
        {
            Regex regexLink = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.(co\.il|com)");

            ISet<string> newLinks = new HashSet<string>();
            foreach (var match in regexLink.Matches(content))
            {
                if (!newLinks.Contains(match.ToString()))
                    newLinks.Add(match.ToString());
            }

            return newLinks;
        }
        Queue<MyUri> readQueue;
        int maximumDepth = 10;
        /// <summary>
        /// Searches for emails from the hosts given in textBox1, and put all the emails in richTextBox1
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            WebRequest myWebRequest;
            WebResponse myWebResponse;
            Uri[] URLs = textBox1.Text.Split(',').Select(x => new Uri(x)).ToArray();
            string Rstring;
            MyUri URL;
            HashSet<string> remembersEmails = new HashSet<string>();
            foreach (Uri myURL in URLs)
            {

                HashSet<Uri> remembersUrls = new HashSet<Uri>();

                readQueue = new Queue<MyUri>();
                readQueue.Enqueue(new MyUri(myURL, 0));
                while (readQueue.Count != 0 && emailCount < 1000)
                {
                    URL = readQueue.Dequeue();
                    try
                    {
                        //if (URL.Uri.Host.IndexOf(".google.") != -1 && URL.Depth >= 3) continue;
                        myWebRequest = WebRequest.Create(URL.Uri);
                        myWebResponse = myWebRequest.GetResponse();//Returns a response from an Internet resource

                        Stream streamResponse = myWebResponse.GetResponseStream();//return the data stream from the internet
                        //and save it in the stream

                        StreamReader sreader = new StreamReader(streamResponse);//reads the data stream
                        Rstring = sreader.ReadToEnd();//reads it to the end
                        ISet<Uri> Links = GetNewLinks(Rstring, URL.Uri);//gets the links only
                        ISet<string> Emails = GetNewEmails(Rstring);//gets the emails only
                        foreach (string item in Emails)
                        {
                            if (remembersEmails.Add(item))
                            {
                                richTextBox1.Text += item + "\n";
                                emailCount++;
                            }
                        }
                        pageCount++;
                        streamResponse.Close();
                        sreader.Close();
                        myWebResponse.Close();

                        label1.Text = "Page count = " + pageCount;
                        label2.Text = "Email count = " + emailCount;
                        label3.Text = "In stack = " + readQueue.Count;
                        label4.Text = "Found now = " + Emails.Count;
                        label5.Text = "Depth = " + URL.Depth;
                        linkLabel1.Text = URL.Uri.ToString();

                        if (URL.Depth < maximumDepth)
                        {
                            foreach (Uri link in Links) //adds the current site links, if we haven't visited them yet, and if they have the same Host.
                            {
                                try
                                {
                                    if (link.Host == URL.Uri.Host && remembersUrls.Add(link))
                                        readQueue.Enqueue(new MyUri(link, URL.Depth + 1));
                                }
                                catch (Exception e2)
                                {
                                    richTextBox2.Text += link + "\n";
                                }
                            }
                        }
                        Application.DoEvents(); // enables a clicking on the stop button.
                    }
                    catch (Exception e2)
                    {
                        richTextBox2.Text += e2.Message + "\n" + URL.Uri + " \n";
                    }
                }
                richTextBox1.Text += "FINISHED";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            readQueue.Clear();

            label1.Text = "Page count = 0";
            label2.Text = "Email count = 0";
            label3.Text = "In stack = 0";
            label4.Text = "Found now = 0";
            linkLabel1.Text = "";
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            maximumDepth = (int)numericUpDown1.Value;
        }
    }
    public struct MyUri
    {
        public MyUri(Uri u, int d)
        {
            Uri = u;
            Depth = d;
        }

        public Uri Uri;
        public int Depth;
    }
}
