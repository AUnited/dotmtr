/// dotMTR is copyright 2010 Nate McKay (natemckay@gmail.com)
/// dotMTR is release to the public under version 2 of the GPL: http://www.gnu.org/licenses/gpl-2.0.txt

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace dotMTR
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
			OpenURL(@"https://sourceforge.net/p/dotmtr/home/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenURL(@"http://en.wikipedia.org/wiki/MTR_%28software%29");
        }

		private void OpenURL(string _url)
		{
			try
			{
				Process.Start(_url);
			}

			catch (Exception ex)
			{
				// Ignore exceptions that occur. Happens when Firefox is the default browser, but not running
			}
		}

		private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			OpenURL(e.LinkText);
		}
    }
}
