using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Felica_ChefSharp
{
    public partial class Form2 : Form
    {
        private List<Label> filesList = new List<Label>();
        private CombHistory history = new CombHistory();

        //UI用
        private List<Panel> historyPanel = new List<Panel>();

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        public void listUpdate(string[] files)
        {
            for (int idx = panel1.Controls.Count - 1; 0 <= idx; idx--)
            {
                try
                {
                    panel1.Controls[idx].Dispose();
                }
                catch { }
            }
            filesList = new List<Label>();

            foreach (string file in files)
            {
                Label fileListOne = new Label();
                string[] fileNameParse = Path.GetFileNameWithoutExtension(file).Split('_');
                fileListOne.Text = fileNameParse[0]+"年"+ fileNameParse[1]+"月";
                fileListOne.TextAlign = ContentAlignment.MiddleCenter;
                fileListOne.Size = new Size(200,30);
                fileListOne.Location = new Point(0, filesList.Count * 30);
                fileListOne.Font = new Font("Meiryo UI", 10F);
                fileListOne.Tag = file;
                fileListOne.MouseEnter += label3_MouseEnter;
                fileListOne.MouseLeave += label3_MouseLeave;
                fileListOne.Click += label3_Click;

                panel1.Controls.Add(fileListOne);
                filesList.Add(fileListOne);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (history.history.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();

                sfd.FileName = "名称未設定.xlsx";
                sfd.Filter = "ExcelLファイル(*.xlsx)|*.xlsx";
                sfd.FilterIndex = 1;
                sfd.Title = "出力先を選択してください。";
                sfd.RestoreDirectory = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    List<CombHistory.HistoryObject> historyData = history.getHistorys();
                    List<excelOutput.outPutExel> writeObject = new List<excelOutput.outPutExel>();

                    foreach (CombHistory.HistoryObject historyOne in historyData)
                    {
                        excelOutput.outPutExel writeOne = new excelOutput.outPutExel();

                        writeOne.name = historyOne.Name;
                        writeOne.dateTime = historyOne.loginTime;
                        writeOne.usePC = historyOne.usePC;

                        writeObject.Insert(0, writeOne);
                    }
                    excelOutput.excel_OutPutEx(writeObject, sfd.FileName);

                    MessageBox.Show("保存しました。", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void label3_MouseEnter(object sender, EventArgs e)
        {
            ((Label)sender).BackColor = Color.FromArgb(200, 200, 200);
        }

        private void label3_MouseLeave(object sender, EventArgs e)
        {
            ((Label)sender).BackColor = BackColor;

        }

        private void label3_Click(object sender, EventArgs e)
        {
            history = new CombHistory();
            string saveFilePath = ((Control)sender).Tag.ToString();

            label2.Text = ((Label)sender).Text + "のログデータ";

            FileStream fs = new FileStream(saveFilePath, FileMode.Open, FileAccess.Read);
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
    }
}
