using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Beam
{
    public partial class GitUP : Form
    {
        public GitUP()
        {
            InitializeComponent();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        }

        public string baseUrl = "https://api.github.com/search/users?q=type:org+";
        public int page = 1;
        public int resCount = 0;
        public JArray orgReps;
        public string repoUrl = "";

        private void GitUP_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            resultsBox.Items.Clear();

            string requestUrl = baseUrl + orgTextBox.Text + "+in:name&page=" + page.ToString();

            JObject jResponse = getRequest(requestUrl);
            
            try
            {
                resCount = (int)jResponse["total_count"];
                if (resCount > 30)
                {
                    NextButton.Enabled = true;
                }

                label1.Text = resCount.ToString() + " search results:";
                foreach (JObject item in jResponse["items"])
                {
                   resultsBox.Items.Add(item["login"]);
                }
            }

            catch (System.Net.WebException err)
            {
                resultsBox.Items.Add(jResponse.ToString());
            }
        }

        public JObject getRequest(string requrl)
        {
            var request = System.Net.WebRequest.Create(@requrl);
            request.Timeout = 5000;
            request.Method = "GET";
            ((HttpWebRequest)request).UserAgent = "GitApp";

            try
            {
                using (var response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                        string full = reader.ReadToEnd();
                        var organisations = JObject.Parse(full);
                        return organisations;
                    }
                }
            }

            catch (System.Net.WebException err)
            {
                string response;
                using (var reader = new StreamReader(err.Response.GetResponseStream()))
                {
                    response = reader.ReadToEnd();
                }
                return (JObject)response;
            }

            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            reposBox.Items.Clear();
            resultsBox.Items.Clear();
            page += 1;
            PrevButton.Enabled = true;
            if (page * 30 > resCount)
            {
                NextButton.Enabled = false;
            }

            string requestUrl = baseUrl + orgTextBox.Text + "+in:name&page=" + page.ToString();

            JObject jResponse = getRequest(requestUrl);

            try
            {
                foreach (JObject item in jResponse["items"])
                {
                    resultsBox.Items.Add(item["login"]);
                }
            }

            catch (System.Net.WebException err)
            {
                resultsBox.Items.Add(jResponse.ToString());
            }
        }

        private void PrevButton_Click(object sender, EventArgs e)
        {
            reposBox.Items.Clear();
            resultsBox.Items.Clear();
            page -= 1;
            NextButton.Enabled = true;
            if (page == 1)
            {
                PrevButton.Enabled = false;
            }

            string requestUrl = baseUrl + orgTextBox.Text + "+in:name&page=" + page.ToString();

            JObject jResponse = getRequest(requestUrl);

            try
            {
                foreach (JObject item in jResponse["items"])
                {
                    resultsBox.Items.Add(item["login"]);
                }
            }

            catch (System.Net.WebException err)
            {
                resultsBox.Items.Add(jResponse.ToString());
            }
        }

        private void resultsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            reposBox.Items.Clear();
            repoName.Text = "";

            string organisation = resultsBox.SelectedItem.ToString();

            string repsUrl = "https://api.github.com/orgs/" + organisation + "/repos";


            var request = System.Net.WebRequest.Create(@repsUrl);
            request.Timeout = 5000;
            request.Method = "GET";
            ((HttpWebRequest)request).UserAgent = "GitApp";

            try
            {
                using (var response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                        string full = reader.ReadToEnd();
                        orgReps = JArray.Parse(full);
                        foreach (JObject jRepo in orgReps)
                        {
                            reposBox.Items.Add(jRepo["name"]);
                        }
                    }
                }
            }

            catch (System.Net.WebException err)
            {
                string response;
                using (var reader = new StreamReader(err.Response.GetResponseStream()))
                {
                    response = reader.ReadToEnd();
                }
            }
        }

        private void reposBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = reposBox.SelectedIndex;
            JToken jRepo = orgReps[index];
            repoName.Text = jRepo["name"].ToString();
            repoUrl = jRepo["html_url"].ToString();
            repoName.LinkVisited = false;

            descriptionBox.Text = jRepo["description"].ToString();




        }

        private void repoName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            repoName.LinkVisited = true;
            System.Diagnostics.Process.Start(repoUrl);
        }
    }
}
