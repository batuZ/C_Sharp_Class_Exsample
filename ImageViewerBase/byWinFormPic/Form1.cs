﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace byWinFormPic
{
    public partial class Form1 : Form
    {
        PlayPictuerBox a;
        public Form1()
        {
            InitializeComponent();
            a = new PlayPictuerBox();
            a.imageInfo_outPut += (k) => { this.Text = k; };
            this.Controls.Add(a);
        }
        private void openImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            if (DialogResult.OK == openFileDialog1.ShowDialog())
                a.setImage(openFileDialog1.FileName,new Point(5000,3000));
        }
    }
}
