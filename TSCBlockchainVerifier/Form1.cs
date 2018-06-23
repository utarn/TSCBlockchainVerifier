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
        private string Key1, newKey1;
        private string Key2, newKey2;
        private bool IsValid1 = false;
        private bool IsValid2 = false;
        private bool IsKeyValid = false;
        private bool IsTimeValid = false;

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://digiexplorer.info");
        }

        private DateTime BlockTime, BlockTime2;

        private async void button2_Click(object sender, EventArgs e)
        {
            string resultString = "";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri("https://digiexplorer.info/api/");

            try
            {
                var response = await client.GetAsync($"tx/{TX1TextBox.Text}");
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<RawTransaction>(responseBody);
                if (result.vout.First(v => v.scriptPubKey.asm.Contains("OP_RETURN")).scriptPubKey.hex.Contains(finalString))
                {
                    IsValid1 = true;
                    Key1 = result.vin.First().addr;
                    Key2 = result.vout.First(v => v.scriptPubKey.asm.Contains("OP_EQUALVERIFY")).scriptPubKey.addresses.First();
                    BlockTime = new DateTime(1970, 1, 1).AddSeconds(result.blocktime);
                }
            }
            catch (Exception)
            {
                resultString = "";
            }

            try
            {
                var response2 = await client.GetAsync($"tx/{TX2TextBox.Text}");
                response2.EnsureSuccessStatusCode();

                string responseBody = await response2.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<RawTransaction>(responseBody);
                if (result.vout.First(v => v.scriptPubKey.asm.Contains("OP_RETURN")).scriptPubKey.hex.Contains(finalString))
                {
                    IsValid2 = true;
                    newKey1 = result.vin.First().addr;
                    newKey2 = result.vout.First(v => v.scriptPubKey.asm.Contains("OP_EQUALVERIFY")).scriptPubKey.addresses.First();
                    BlockTime2 = new DateTime(1970, 1, 1).AddSeconds(result.blocktime);

                }
            }
            catch (Exception)
            {
                resultString = "";
            }

            if (BlockTime == BlockTime2)
            {
                IsTimeValid = true;
            }
            if (Key1 == newKey2 && Key2 == newKey1)
            {
                IsKeyValid = true;
            }
            if (!IsValid1 || !IsValid2)
            {
                resultString += "รหัสธุรกรรมอย่างน้อย 1 ธุรกรรมไม่ตรงกับการเซ็น\r\n";
            }
            else
            {
                if (IsTimeValid)
                {
                    resultString += $"ข้อมูลอยู่ในบล็อกเชนเวลา {BlockTime.ToString("d MMMM yyyy HH:mm:ssน.", new System.Globalization.CultureInfo("th-TH"))}\r\n";
                }

                if (IsKeyValid)
                {
                    if (Key1 == ServerKey || Key2 == ServerKey)
                    {
                        resultString += "ธุรกรรมทำกับระบบไทยสมาร์ทคอนแทรค\r\n";
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
                else
                {
                    resultString += "ธุรกรรมไม่ได้กระทำโดยผู้เซ็นคนเดียวกัน\r\n";
                }

            }

            ResultTextBox.Text = resultString;
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
