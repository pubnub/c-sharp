using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PubNubMessagingExample
{
    public partial class PubnubResult : Form
    {
        public PubnubResult()
        {
            InitializeComponent();
        }
        public void AddMessage(string message)
        {
            lbResult.Items.Insert(0, message);
            while (lbResult.Items.Count > 20)
            {
                lbResult.Items.RemoveAt(lbResult.Items.Count - 1);
            }
        }
    }
}