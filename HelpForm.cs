using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TouchpadPeaceFree
{
    public partial class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
        }

        private void OKButtonHelp_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
