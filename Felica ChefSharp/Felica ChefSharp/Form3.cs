using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Felica_ChefSharp
{
    public partial class Form3 : Form
    {
        private List<Label> usersList = new List<Label>();
        private CombHistory history = new CombHistory();

        private List<Panel> historyPanel = new List<Panel>();
        private string selectedUid = "";
        Font fnt = new Font("Meiryo UI", 11);

        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }

        public void listUpdate(Dictionary<string, string> nameAndId, string[] files)
        {
            for (int idx = panel1.Controls.Count - 1; 0 <= idx; idx--)
            {
                try
                {
                    panel1.Controls[idx].Dispose();
                }
                catch { }
            }
            usersList = new List<Label>();

            foreach (string felicaId in nameAndId.Keys)
            {
                Label fileListOne = new Label();
                fileListOne.Text = nameAndId[felicaId];
                fileListOne.TextAlign = ContentAlignment.MiddleCenter;
                fileListOne.Size = new Size(280, 30);
                fileListOne.Location = new Point(0, usersList.Count * 30);
                fileListOne.Font = new Font("Meiryo UI", 10F);
                fileListOne.Tag = felicaId;
                fileListOne.MouseEnter += label3_MouseEnter;
                fileListOne.MouseLeave += label3_MouseLeave;
                fileListOne.Click += label3_Click;

                panel1.Controls.Add(fileListOne);
                usersList.Add(fileListOne);
            }

            foreach (string file in files)
            {
                string[] fileNameParse = Path.GetFileNameWithoutExtension(file).Split('_');

                comboBox1.Items.Add(fileNameParse[0] + "年" + fileNameParse[1] + "月 のログデータ");
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = -1;
            tabPage1.Text = ((Label)sender).Text + "の出席状況";
            textBox1.Text = ((Label)sender).Text;
            selectedUid = ((Label)sender).Tag.ToString();
            label7.Text = selectedUid.Substring(0, 40) + "..." + selectedUid.Substring(0, 10);
        }

        private void label3_MouseEnter(object sender, EventArgs e)
        {
            ((Label)sender).BackColor = Color.FromArgb(200, 200, 200);
        }

        private void label3_MouseLeave(object sender, EventArgs e)
        {
            ((Label)sender).BackColor = BackColor;

        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        public void logListUpdate(string logFilePath)
        {
            history = new CombHistory();

            FileStream fs = new FileStream("logs\\"+logFilePath, FileMode.Open, FileAccess.Read);
            byte[] saveData = new byte[fs.Length];
            fs.Read(saveData, 0, saveData.Length);
            fs.Close();

            string alldata = "";
            try
            {
                alldata = CombEncrypt.Decrypt(saveData);
            }
            catch { }

            for (int idx = panel2.Controls.Count - 1; 0 <= idx; idx--)
            {
                try
                {
                    panel2.Controls[idx].Dispose();
                }
                catch { }
            }
            panel2.Update();
            historyPanel = new List<Panel>();

            string[] lines = alldata.Split(CombHistory.lineSplit);
            foreach (string line in lines)
            {
                string[] oneObj = line.Split(CombHistory.historySplit);
                if (oneObj.Length == 3)
                {
                    CombHistory.HistoryObject addObj = new CombHistory.HistoryObject();
                    addObj.uid = oneObj[1];
                    addObj.loginTime = CombHistory.FromUnixTime(long.Parse(oneObj[0]));
                    addObj.Name = history.getName(oneObj[1]);
                    addObj.usePC = oneObj[2];

                    if (selectedUid != "*" && addObj.uid != selectedUid) continue;

                    history.history.Add(addObj);

                    Panel parent = new Panel();
                    parent.Location = new Point(0, historyPanel.Count * 50);
                    parent.Size = new Size(panel2.Width - 20, 50);

                    Label nameLabel = new Label();
                    nameLabel.Text = history.getName(oneObj[1]);
                    nameLabel.Location = new Point(3, 3);
                    nameLabel.Size = new Size(parent.Width, 20);
                    nameLabel.TextAlign = ContentAlignment.MiddleLeft;

                    Label usePCLabel = new Label();
                    usePCLabel.Text = "使用したPC: " + oneObj[2].Substring(0, oneObj[2].Length > 20 ? 20 : oneObj[2].Length)
                        + (oneObj[2].Length > 20 ? "..." : "");
                    usePCLabel.Font = new Font("Meiryo UI", 9F);
                    usePCLabel.ForeColor = Color.FromArgb(100, 100, 100);
                    usePCLabel.Location = new Point(2, 28);
                    usePCLabel.Size = new Size((int)(parent.Width * 0.535714), 20);
                    usePCLabel.TextAlign = ContentAlignment.MiddleLeft;

                    Label dateLabel = new Label();
                    dateLabel.Text = CombHistory.FromUnixTime(long.Parse(oneObj[0])).ToString();
                    dateLabel.Font = new Font("Meiryo UI", 9F);
                    dateLabel.ForeColor = Color.FromArgb(100, 100, 100);
                    dateLabel.Location = new Point((int)(parent.Width * 0.535714), 28);
                    dateLabel.Size = new Size((int)(parent.Width * (1 - 0.535714)), 20);
                    dateLabel.TextAlign = ContentAlignment.MiddleRight;

                    PictureBox borderBottom = new PictureBox();
                    borderBottom.Size = new Size(parent.Width, 1);
                    borderBottom.Location = new Point(0, parent.Height - 1);
                    borderBottom.BackColor = Color.FromArgb(180, 180, 180);

                    parent.Controls.Add(nameLabel);
                    parent.Controls.Add(usePCLabel);
                    parent.Controls.Add(dateLabel);
                    parent.Controls.Add(borderBottom);
                    panel2.Controls.Add(parent);

                    historyPanel.Add(parent);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                Regex regexObj = new Regex("^(.*?)年(.*?)月", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                Match matchObj = regexObj.Match(comboBox1.SelectedItem.ToString());

                string logFilePath = matchObj.Groups[1].Value + "_" + matchObj.Groups[2].Value + "_save.cfsd";
                logListUpdate(logFilePath);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (selectedUid == "") return;
            history.nameAndId[selectedUid] = textBox1.Text;
            ((Form1)Owner).history.nameAndId[selectedUid] = textBox1.Text;
            history.saveMember();

            updateName(textBox1.Text);
            ((Form1)Owner).updateName(textBox1.Text, selectedUid);
        }

        private void updateName(string updateText)
        {
            Control.ControlCollection updateItems = panel1.Controls;
            foreach(Control controlOne in updateItems)
            {
                if (controlOne.Tag.ToString() == selectedUid) controlOne.Text = updateText;
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                button1_Click(null, null);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedUid == "") return;

            DialogResult cfm =MessageBox.Show("本当にこのカードの登録を削除しますか？\r\n削除した登録データは復元できません。",
                "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if(cfm == DialogResult.Yes || cfm == DialogResult.OK)
            {
                history.nameAndId.Remove(selectedUid);
                ((Form1)Owner).history.nameAndId.Remove(selectedUid);
                history.saveMember();

                Hide();
                ((Form1)Owner).button7_Click(null, null);
            }
        }
    }
}
