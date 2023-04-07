using EZ_Updater;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EGStealer
{
    public partial class UpdaterMessage : Form
    {
        public UpdaterMessage()
        {
            InitializeComponent();

            OKbtn.Hide();
            Messagelbl.Text = Updater.Message;
            Updater.Update(UIChange);
        }

        private void UIChange(object sender, EventArgs e)
        {
            Messagelbl.Text = Updater.Message;
            progressBar1.Value = Updater.ProgressPercentage;

            switch (Updater.ShortState)
            {
                case UpdaterShortState.Canceled:
                    OKbtn.Visible = true;
                    break;
                case UpdaterShortState.Installed:
                    Application.Restart();
                    break;
            }
        }
    }
}
