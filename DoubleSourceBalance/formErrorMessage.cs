using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DoubleSourceBalance
{
    public partial class formErrorMessage : Form
    {
        private bool hasDetails = false;

        public formErrorMessage(string message) 
        {
            InitializeComponent();
            txtMessage.Text = message;
            this.Load += FormErrorMessage_Load;
            btnDetails.Click += BtnDetails_Click;
        }

        private void BtnDetails_Click(object sender, EventArgs e)
        {
            this.Height = 373;
        }

        private void FormErrorMessage_Load(object sender, EventArgs e)
        {
            btnDetails.Enabled = hasDetails;
        }

        public formErrorMessage(string title, string message) : this(message)
        {
            this.Text = title;
        }

        public formErrorMessage(Tuple<string, string> messageDetails):this(messageDetails.Item1)
        {
            txtDetails.Text = messageDetails.Item2;
            hasDetails = true;
        }

        public formErrorMessage(string title, Tuple<string, string> messageDetails) : this(messageDetails)
        {
            this.Text = title;
        }
    }
}
