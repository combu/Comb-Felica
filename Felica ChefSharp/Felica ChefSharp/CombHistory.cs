using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace Felica_ChefSharp
{
    public class CombHistory
    {
        public struct HistoryObject
        {
            public DateTime loginTime;
            public string Name;
            public string uid;
            public string usePC;
        }
        public List<HistoryObject> history = new List<HistoryObject>();
        private Dictionary<string, string> nameAndId = new Dictionary<string, string>();

        public const char nameSplit = (char)3;
        public const char lineSplit = (char)4;
        public const char historySplit = (char)5;
        private string saveFilePath = "";
        private const string IdFilePath = "id.cfsd";
        private readonly Encoding textEncoding = Encoding.UTF8;

        public CombHistory()
        {
            saveFilePath = "logs\\" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_save.cfsd";

            if (!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs");
            }

            if (File.Exists(IdFilePath))
            {
                FileStream fs = new FileStream(IdFilePath, FileMode.Open, FileAccess.Read);
                byte[] saveData = new byte[fs.Length];
                fs.Read(saveData, 0, saveData.Length);
                fs.Close();

                try
                {
                    string alldata = CombEncrypt.Decrypt(saveData);
                    Console.WriteLine(alldata);

                    string[] lines = alldata.Split(lineSplit);
                    foreach (string line in lines)
                    {
                        string[] oneObj = line.Split(nameSplit);
                        if (oneObj.Length == 2) nameAndId.Add(oneObj[0], oneObj[1]);
                    }
                }
                catch { }

            }
            else {
                using (FileStream hStream = File.Create(IdFilePath))
                    if (hStream != null)
                        hStream.Close();
            }

            if (File.Exists(saveFilePath))
            {

            }else {
                using (FileStream hStream = File.Create(saveFilePath))
                    if (hStream != null)
                        hStream.Close();
            }
        }

        public string getName(string uid)
        {
            if (nameAndId.ContainsKey(uid)) return nameAndId[uid];
            else throw new Exception("登録されていないIDです。");
        }

        public void addHisotry(string uid, string usePC)
        {
            HistoryObject addObj = new HistoryObject();
            addObj.uid = uid;
            addObj.loginTime = DateTime.UtcNow;
            addObj.Name = getName(uid);
            addObj.usePC = usePC;

            history.Insert(0, addObj);

            saveHistory();
        }

        public List<HistoryObject> getHistorys()
        {
            history = new List<HistoryObject>();
            loadHistory();
            return history;
        }

        public void loadHistory()
        {
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

            string[] lines = alldata.Split(lineSplit);
            foreach (string line in lines)
            {
                string[] oneObj = line.Split(historySplit);
                if (oneObj.Length == 3)
                {
                    HistoryObject addObj = new HistoryObject();
                    addObj.uid = oneObj[1];
                    addObj.loginTime = FromUnixTime(long.Parse(oneObj[0]));
                    addObj.Name = getName(oneObj[1]);
                    addObj.usePC = oneObj[2];

                    history.Add(addObj);
                }
            }
        }

        public void saveHistory()
        {
            byte[] saveByte = CombEncrypt.Encrypt(parseHistory());

            FileStream fs = new FileStream(saveFilePath,FileMode.Create,FileAccess.Write);
            fs.Write(saveByte, 0, saveByte.Length);
            fs.Close();
        }

        public string parseHistory()
        {
            string returnString = "";

            foreach (HistoryObject historyOne in history)
            {
                if (returnString != "") returnString += lineSplit;
                returnString += ToUnixTime(historyOne.loginTime).ToString()
                    + historySplit + historyOne.uid
                    + historySplit + historyOne.usePC;
            }

            return returnString;
        }

        public bool addMember(string uid, string name)
        {
            if (nameAndId.ContainsKey(uid)) return false;
            else {
                nameAndId.Add(uid, name);

                saveMember();
                return true;
            }
        }

        public void saveMember()
        {
            byte[] saveByte = CombEncrypt.Encrypt(parseMember());

            FileStream fs = new FileStream(IdFilePath, FileMode.Create, FileAccess.Write);
            fs.Write(saveByte, 0, saveByte.Length);
            fs.Close();
        }

        public string parseMember()
        {
            string returnString = "";

            foreach (string key in nameAndId.Keys)
            {
                if (returnString != "") returnString += lineSplit;
                returnString += key + nameSplit
                    + nameAndId[key];
            }

            return returnString;
        }



        public readonly static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // DateTimeをUNIX時間に変換するメソッド
        public static long ToUnixTime(DateTime dateTime)
        {
            // 時刻をUTCに変換
            dateTime = dateTime.ToUniversalTime();

            // unix epochからの経過秒数を求める
            return (long)dateTime.Subtract(UnixEpoch).TotalSeconds;
        }

        // UNIX時間からDateTimeに変換するメソッド
        public static DateTime FromUnixTime(long unixTime)
        {
            // unix epochからunixTime秒だけ経過した時刻を求める
            return UnixEpoch.AddSeconds(unixTime);
        }
    }
}
