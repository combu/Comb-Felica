using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using FelicaLib;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography;

namespace Felica_ChefSharp
{
    public partial class Form1 : Form
    {
        public Encoding enc = Encoding.UTF8;
        public struct FelicaInfo{
            public string felicaId;
            public string felicaPm;
            public string felicaUid;
        }
        public struct scanType {
            public int NEW_REGISTRATION = 0;

        }
        private string prevUid = null;
        delegate void loginDe(FelicaInfo info, string name);
        delegate void memberDe(bool success);
        delegate void addHistoryDe(string name, string usePC, DateTime date, string tag, bool mode = false);
        delegate void loginErrorDe(string message);
        delegate void p3HideDe();
        public CombHistory history;
        private bool gone = false;
        private Form2 f2 = new Form2();
        private Form3 f3 = new Form3();
        private 

        //UI用
        private List<Panel> historyPanel = new List<Panel>();

        public Form1()
        {
            InitializeComponent();

            loginDisplay.BackColor = BackColor;
            usePC.BackColor = BackColor;
            loginInfo.BackColor = BackColor;

            //FelicaLibがなければ配置
            if (!File.Exists("felicalib.dll"))
            {
                byte[] felicaLibBin = Properties.Resources.felicalib;
                File.WriteAllBytes("felicalib.dll", felicaLibBin);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            panel3.Left = (Width - panel3.Width) / 2;
            panel3.Top = (Height - panel3.Height) / 2;
            MaximumSize = Size;
            MinimumSize = Size;

            history = new CombHistory();

            List<CombHistory.HistoryObject> historyData = history.getHistorys();
            historyData.Reverse();
            int max = historyData.Count-1, i=0;
            foreach (CombHistory.HistoryObject historyOne in historyData)
            {
                bool mode = false;
                if (50 == i || max == i) mode = true;
                else if (50 < i) break;

                addHistory(historyOne.Name, historyOne.usePC, historyOne.loginTime, historyOne.uid, mode);
                i++;
            }

            new Task(() =>
            {
                while (true)
                {
                    if (gone)
                    {
                        using (Felica felicaObj = new Felica())
                        {
                            FelicaInfo FelicaData = readFelica(felicaObj);
                            if (FelicaData.felicaUid != null && prevUid != FelicaData.felicaUid)
                            {
                                if (groupBox1.Enabled && gone)
                                {
                                    if (history.addMember(FelicaData.felicaUid, textBox1.Text))
                                        Invoke(new memberDe(addMemberSuccess), new object[] { true });
                                    else
                                        Invoke(new memberDe(addMemberSuccess), new object[] { false });
                                }
                                else if (groupBox2.Enabled && gone)
                                {
                                    string usePC = textBox2.Text;
                                    try
                                    {
                                        history.getName(FelicaData.felicaUid, true);
                                    }
                                    catch
                                    {
                                        Invoke(new loginErrorDe(loginError), new object[] { "このカードは登録されていません。" });
                                        continue;
                                    }
                                    history.addHisotry(FelicaData.felicaUid, usePC);
                                    Invoke(new loginDe(login), new object[] { FelicaData, history.getName(FelicaData.felicaUid) });
                                    Invoke(new addHistoryDe(addHistory), new object[] {
                                    history.getName(FelicaData.felicaUid),
                                    usePC,
                                    DateTime.Now,
                                    FelicaData.felicaUid,
                                    true
                                });

                                    prevUid = FelicaData.felicaUid;
                                }
                                else {
                                    Invoke(new loginErrorDe(loginError), new object[] { "新規登録または使用したPCを入力してください" });
                                }

                                Invoke(new p3HideDe(p3Hide));
                            }
                        }
                    }

                    Thread.Sleep(30);
                }
            }).Start();
        }

        private void p3Hide()
        {
            panel3.Hide();
            gone = false;
        }

        private void login(FelicaInfo info, string name)
        {
            loginDisplay.Text = name + "さんが出席しました";
            loginDisplay.Font = new Font("Meiryo UI", 22);
            loginDisplay.ForeColor = Color.FromArgb(64, 64, 64);

            usePC.Text = "使用したPC: " + textBox2.Text;
            loginInfo.Text = "felicaId: " + info.felicaUid.Substring(0, 40) + "..."+ info.felicaUid.Substring(0, 10);

            textBox2.Text = "";
        }

        private void addMemberSuccess(bool success)
        {
            if (success)
            {
                MessageBox.Show(textBox1.Text + "さんを登録しました。", "登録完了"
                    , MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox1.Text = "";
            }
            else
            {
                MessageBox.Show("このカードは既に登録されています", "登録失敗"
                    , MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loginError(string message)
        {
            loginDisplay.Text = message;
            loginDisplay.Font = new Font("Meiryo UI", 15);
            loginDisplay.ForeColor = Color.FromArgb(255, 50, 100);
        }

        private void addHistory(string name, string usePC, DateTime date, string uid, bool mode = false)
        {
            Panel parent = new Panel();
            parent.Location = new Point(0, 0);
            parent.Size = new Size(panel1.Width-20, 50);
            parent.Top = 0;
            parent.Tag = uid;

            Label nameLabel = new Label();
            nameLabel.Text = name;
            nameLabel.Location = new Point(3, 3);
            nameLabel.Size = new Size(parent.Width, 20);
            nameLabel.TextAlign = ContentAlignment.MiddleLeft;
            nameLabel.Name = "nameLabel";

            Label usePCLabel = new Label();
            usePCLabel.Text = "使用したPC: " + usePC.Substring(0, usePC.Length > 20 ? 20 : usePC.Length)
                + (usePC.Length > 20 ? "..." : "");
            usePCLabel.Font = new Font("Meiryo UI", 9F);
            usePCLabel.ForeColor = Color.FromArgb(100, 100, 100);
            usePCLabel.Location = new Point(2, 28);
            usePCLabel.Size = new Size((int)(parent.Width * 0.535714), 20);
            usePCLabel.TextAlign = ContentAlignment.MiddleLeft;
            
            Label dateLabel = new Label();
            dateLabel.Text = date.ToString();
            dateLabel.Font = new Font("Meiryo UI", 9F);
            dateLabel.ForeColor = Color.FromArgb(100, 100, 100);
            dateLabel.Location = new Point((int)(parent.Width * 0.535714), 28);
            dateLabel.Size = new Size((int)(parent.Width * (1 - 0.535714)), 20);
            dateLabel.TextAlign = ContentAlignment.MiddleRight;

            PictureBox borderBottom = new PictureBox();
            borderBottom.Size = new Size(parent.Width, 1);
            borderBottom.Location = new Point(0, parent.Height-1);
            borderBottom.BackColor = Color.FromArgb(180, 180, 180);

            parent.Controls.Add(nameLabel);
            parent.Controls.Add(usePCLabel);
            parent.Controls.Add(dateLabel);
            parent.Controls.Add(borderBottom);
            panel1.Controls.Add(parent);

            if(historyPanel.Count-1 == 50)
            {
                historyPanel[49].Hide();
                panel1.Controls.Remove(historyPanel[49]);
            }

            historyPanel.Insert(0, parent);

            if (mode)
            {
                for (int i=1; i<50; ++i)
                {
                    if (i >= historyPanel.Count) break;
                    historyPanel[i].Top = i * 50;
                }
                Update();
                panel1.Update();
            }
        }


        private FelicaInfo readFelica(Felica felicaObj)
        {
            FelicaInfo returnData = new FelicaInfo();

            try
            {
                felicaObj.Polling((int)SystemCode.Any);

                returnData.felicaId = Convert.ToBase64String(felicaObj.IDm());
                returnData.felicaPm = Convert.ToBase64String(felicaObj.PMm());
                returnData.felicaUid = SHA256(returnData.felicaId + "COMB" + returnData.felicaPm);

                felicaObj.Dispose();
            }
            catch
            {
                prevUid = null;
            }
            return returnData;
        }

        private string SHA256(string originalText)
        {
            byte[] byteValue = Encoding.UTF8.GetBytes(originalText);

            SHA256 crypto = new SHA256CryptoServiceProvider();
            byte[] hashValue = crypto.ComputeHash(byteValue);

            StringBuilder hashedText = new StringBuilder();
            for (int i = 0; i < hashValue.Length; i++)
            {
                hashedText.AppendFormat("{0:X2}", hashValue[i]);
            }

            return hashedText.ToString();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string showMessage = "1. テキストボックスに名前を入力" + "\r\n" +
                "2. Enterまたは登録ボタンをクリック" + "\r\n" +
                "3. 交通電子マネーをタッチ";

            MessageBox.Show(showMessage, "登録手順", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string showMessage = "1. テキストボックスに使用したパソコンの番号を入力" + "\r\n" +
                "2. Enterまたは登録ボタンをクリック" + "\r\n" +
                "3. 交通電子マネーをタッチ";

            MessageBox.Show(showMessage, "登録手順", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.FileName = "名称未設定.csv";
            sfd.Filter = "CSVファイル[Excelで開ける](*.csv)|*.csv";
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
                bool saveSuccess = excelOutput.excel_OutPutEx(writeObject, sfd.FileName);

                if (saveSuccess)
                    MessageBox.Show("保存しました。", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("保存に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            groupBox1.Enabled = !groupBox1.Enabled;
            groupBox2.Enabled = !groupBox2.Enabled;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if ((groupBox1.Enabled && textBox1.Text == "") || (groupBox2.Enabled && textBox2.Text == ""))
            {
                MessageBox.Show("入力してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            gone = true;
            panel3.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            p3Hide();
            gone = false;
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
                button3_Click(null, null);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (!f2.Visible)
            {
                string[] files = Directory.GetFiles("logs", "*.cfsd", SearchOption.AllDirectories);

                f2 = new Form2();
                f2.Owner = this;
                f2.Show();
                f2.listUpdate(files);
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) button5_Click(null, null);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (!f3.Visible)
            {
                string[] files = Directory.GetFiles("logs", "*.cfsd", SearchOption.AllDirectories);

                f3 = new Form3();
                f3.Owner = this;
                f3.Show();
                f3.listUpdate(history.nameAndId, files);
            }
        }

        public void updateName(string updateText, string selectedUid)
        {
            Control.ControlCollection updateItems = panel1.Controls;
            foreach (Control controlOne in updateItems)
            {
                if (controlOne.Tag.ToString() == selectedUid)
                {
                    controlOne.Controls.Find("nameLabel", false)[0].Text = updateText;
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {

        }
    }
}
