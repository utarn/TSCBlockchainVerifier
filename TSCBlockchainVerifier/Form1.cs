using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TSCBlockchainVerifier
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private byte[] dataHash;
        private string finalString;
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            if (!string.IsNullOrEmpty(openFileDialog1.FileName))
            {
                FileLabel.Text = openFileDialog1.FileName;
                UpdateHash();
            }


        }

        private void UpdateHash()
        {
            try
            {
                var data = File.ReadAllBytes(openFileDialog1.FileName);
                dataHash = CalculateHash(data);
                dataHash = CalculateHash(data);
                dataHash = CalculateHash(data);
                if (SignTextBox.Text.Trim().Length > 0)
                {
                    var noteByte = Encoding.UTF8.GetBytes(SignTextBox.Text);
                    var noteHash = CalculateHash(noteByte);
                    var dataConcat = noteHash.Concat(dataHash).ToArray();
                    dataHash = CalculateHash(dataConcat);
                }
                else
                {
                    dataHash = CalculateHash(data);
                }
                finalString = BitConverter.ToString(dataHash).Replace("-", "").ToLower();
                SignatureTextBox.Text = finalString;
            }
            catch (Exception)
            {
                return;
            }

        }

        public byte[] CalculateHash(byte[] data)
        {
            var cloner = new byte[data.Length];
            Array.Copy(data, cloner, data.Length);
            var hasher = new KeccakDigest(256);
            hasher.BlockUpdate(cloner, 0, cloner.Length);
            var output = new byte[hasher.GetDigestSize()];
            hasher.DoFinal(output, 0);
            return output;
        }

        private void SignTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateHash();
        }

        private string ServerKey = "DCzmzkMBqEz2tLn47W9YuNAV9cFzuWCydW";


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://digiexplorer.info");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start($"https://digiexplorer.info/tx/{TX1TextBox.Text.Trim()}");
        }
        private void button4_Click(object sender, EventArgs e)
        {
            Process.Start($"https://digiexplorer.info/tx/{TX2TextBox.Text.Trim()}");
        }
        private void TX1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TX1TextBox.Text) && TX1TextBox.Text.Length == 64 && OnlyHexInString(TX1TextBox.Text))
            {
                button3.Enabled = true;
            }
            else
            {
                button3.Enabled = false;
            }
        }

        private void TX2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TX2TextBox.Text) && TX2TextBox.Text.Length == 64 && OnlyHexInString(TX2TextBox.Text))
            {
                button4.Enabled = true;
            }
            else
            {
                button4.Enabled = false;
            }
        }

        private DateTime BlockTime, BlockTime2;

        private async void button2_Click(object sender, EventArgs e)
        {
            string Key1 = "a", newKey1 = "b";
            string Key2 = "c", newKey2 = "d";
            bool IsValid1 = false;
            bool IsValid2 = false;
            string role = "g";
            string userId = "h";
            int? signId = null;

            string resultString = "";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri("https://digiexplorer.info/api/");

            string blockvalue1 = "e", blockvalue2 = "f";
            try
            {
                var response = await client.GetAsync($"tx/{TX1TextBox.Text}");
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<RawTransaction>(responseBody);
                var matchResult = result.vout.FirstOrDefault(v => v.scriptPubKey.asm.Contains("OP_RETURN"));
                string blockvalue = "";
                if (matchResult != null)
                {
                    blockvalue = matchResult.scriptPubKey.hex;
                    blockvalue1 = blockvalue;
                }

                if (blockvalue.Contains(finalString))
                {
                    IsValid1 = true;
                    Key1 = result.vin.First().addr;
                    Key2 = result.vout.First(v => v.scriptPubKey.asm.Contains("OP_EQUALVERIFY")).scriptPubKey.addresses.First();
                    BlockTime = new DateTime(1970, 1, 1).AddSeconds(result.blocktime);


                    blockvalue = blockvalue.Replace(finalString, "");
                    role = blockvalue.Substring(blockvalue.Length - 2, 2);
                    userId = blockvalue.Substring(blockvalue.Length - 34, 32);
                    signId = int.Parse(blockvalue.Substring(blockvalue.Length - 36, 2), System.Globalization.NumberStyles.HexNumber); 
                }
            }
            catch (Exception)
            {
            }

            try
            {
                var response2 = await client.GetAsync($"tx/{TX2TextBox.Text}");
                response2.EnsureSuccessStatusCode();

                string responseBody = await response2.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<RawTransaction>(responseBody);
                var matchResult = result.vout.FirstOrDefault(v => v.scriptPubKey.asm.Contains("OP_RETURN"));
                string blockvalue = "";
                if (matchResult != null)
                {
                    blockvalue = matchResult.scriptPubKey.hex;
                    blockvalue2 = blockvalue;
                }

                if (blockvalue.Contains(finalString))
                {
                    IsValid2 = true;
                    newKey1 = result.vin.First().addr;
                    newKey2 = result.vout.First(v => v.scriptPubKey.asm.Contains("OP_EQUALVERIFY")).scriptPubKey.addresses.First();
                    BlockTime2 = new DateTime(1970, 1, 1).AddSeconds(result.blocktime);

                }
            }
            catch (Exception)
            {
            }

            if (blockvalue1 == blockvalue2)
            {
                resultString += "รหัสธุรกรรมที่กรอกเป็นรหัสธุรกรรมคู่กัน";
            }
            else
            {
                resultString += "รหัสธุรกรรมที่กรอกไม่ได้เป็นรหัสธุรกรรมคู่กัน";
                goto Show;
            }
            if (!IsValid1 || !IsValid2)
            {
                resultString += "รหัสธุรกรรมอย่างน้อย 1 ธุรกรรมไม่ตรงกับการเซ็น\r\n";
                goto Show;
            }

            if (Key1 == newKey2 && Key2 == newKey1)
            {
                resultString += $"ข้อมูลอยู่ในบล็อกเชนเวลา {BlockTime.ToString("d MMMM yyyy HH:mm:ssน.", new System.Globalization.CultureInfo("th-TH"))}\r\n";
                if (Key1 == ServerKey || Key2 == ServerKey)
                {
                    resultString += "ธุรกรรมทำกับระบบไทยสมาร์ทคอนแทรค\r\n";
                }
                if (signId.HasValue)
                {
                    resultString += $"รหัสการเซ็นที่ {signId.Value}\r\n";
                    resultString += $"รหัสบัญชีผู้ใช้ {userId}\r\n";
                }
                if (Key1 == ServerKey)
                {
                    resultString += $"กุญแจสาธารณะของผู้เซ็นคือ {Key2}\r\n";
                }
                else
                {
                    resultString += $"กุญแจสาธารณะของผู้เซ็นคือ {Key1}\r\n";
                }
            }


            Show:
            ResultTextBox.Text = resultString;
        }

        public bool OnlyHexInString(string test)
        {
            // For C-style hex notation (0xFF) you can use @"\A\b(0[xX])?[0-9a-fA-F]+\b\Z"
            return System.Text.RegularExpressions.Regex.IsMatch(test, @"\A\b[0-9a-fA-F]+\b\Z");
        }

    }

    public class RawTransaction
    {
        public string hex { get; set; }
        public string txid { get; set; }
        public string hash { get; set; }
        public int size { get; set; }
        public int vsize { get; set; }
        public int version { get; set; }
        public int locktime { get; set; }
        public List<Vin> vin { get; set; }
        public List<Vout> vout { get; set; }
        public string blockhash { get; set; }
        public long confirmations { get; set; }
        public long time { get; set; }
        public long blocktime { get; set; }
        public decimal valueOut { get; set; }
        public decimal valueIn { get; set; }
        public decimal fees { get; set; }
    }

    public class Vin
    {
        public string coinbase { get; set; }
        public List<string> txinwitness { get; set; }
        public string txid { get; set; }
        public int vout { get; set; }
        public ScriptSig scriptSig { get; set; }
        public long sequence { get; set; }
        public int n { get; set; }
        public string addr { get; set; }
        public long valueSat { get; set; }
        public decimal value { get; set; }
        public object doubleSpentTxID { get; set; }
    }

    public class Vout
    {
        public decimal value { get; set; }
        public int n { get; set; }
        public ScriptPubKey scriptPubKey { get; set; }
    }

    public class ScriptSig
    {
        public string asm { get; set; }
        public string hex { get; set; }
    }

    public class ScriptPubKey
    {
        public string asm { get; set; }
        public string hex { get; set; }
        public int reqSigs { get; set; }
        public string type { get; set; }
        public List<string> addresses { get; set; }
    }
}
