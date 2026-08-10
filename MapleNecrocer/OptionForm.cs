using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapleNecrocer;

public partial class OptionForm : Form
{
    public OptionForm()
    {
        InitializeComponent();
        Instance = this;
        LoadMuteState();
    }
    public static OptionForm Instance;
    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
        if (checkBox1.Checked)
        {
            Sound.isMute = true;
            Music.Pause();
        }
        else
        {
            Sound.isMute = false;
            Music.Resume();
        }
    }

    private void OptionForm_Shown(object sender, EventArgs e)
    {
        this.FormClosing += (s, e1) =>
        {
            SaveMuteState();
            this.Hide();
            e1.Cancel = true;
        };
    }

    private void LoadMuteState()
    {
        AppSettings.Load();
        if (AppSettings.IsMute)
        {
            checkBox1.Checked = true;
            Sound.isMute = true;
            Music.Pause();
        }
    }

    private void SaveMuteState()
    {
        AppSettings.IsMute = Sound.isMute;
        AppSettings.Save();
    }

    private void OptionForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Alt)
            e.Handled = true;

        ActiveControl = null;
    }
    
    private void btnSaveMaplePath_Click(object sender, EventArgs e)
    {
        using (FolderBrowserDialog dialog = new FolderBrowserDialog())
        {
            dialog.Description = "Select MapleStory folder";
            dialog.InitialDirectory = string.IsNullOrWhiteSpace(Program.MaplePath) ? Application.StartupPath : Program.MaplePath;
            
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Program.MaplePath = dialog.SelectedPath;
                AppSettings.MaplePath = dialog.SelectedPath;
                AppSettings.Save();
                MessageBox.Show($"MapleStory path updated to:\n{Program.MaplePath}\n\nRestart the application to apply changes.", "Path Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
