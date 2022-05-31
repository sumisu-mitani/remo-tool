using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using USB_IR_Library;
using System.Runtime.InteropServices;


namespace USB_IR_sample
{
    public partial class Form1 : Form
    {

        byte[] ir_code = new byte[9600];

        public static readonly Boolean SET = true;
        public static readonly Boolean CLR = false;
        byte r_remocon_no = 0;

        byte[] code_dammy = new byte[8] { 0x02, 0x20, 0xE0, 0x04, 0x00, 0x00, 0x00, 0x06 };
        byte[] code_Stype = new byte[8] { 0x02, 0x20, 0xE0, 0x04, 0x80, 0x00, 0x10, 0x00 };

        byte[] code_1packet = new byte[19] { 0x02, 0x20, 0xE0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };    // 赤外線コード 4byte * 8bit = 32bit
        byte[] code_2packet = new byte[19] { 0x02, 0x20, 0xE0, 0x00, 0x41, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };    // 赤外線コード 4byte * 8bit = 32bit

        byte[] list_code_1packet = new byte[19];
        byte[] list_code_2packet = new byte[19];

        string remocon_no_str = "0";

        byte[] rcv_data = new byte[153];

        byte rmdt_temp = 0x20;
        byte rmdt_humid = 50;
        byte rmdt_mode = 0x00;
        byte rmdt_onoff = 0x00;
        byte rmdt_AI = 0x00;
        byte rmdt_fan = 0xA0;
        byte rmdt_flap_lr = 0x0F;
        byte rmdt_flap_ud = 0x0;
        byte rmdt_vent_onoff = 0x00;
        byte f_buzzer = 0x00;
        byte rmdt_SW = 0x40;
        byte rmdt_push_sw = 0x00;
        byte f_rmdt_motto = 0x00;
        byte f_rmdt_powerfull = 0x00;
        byte f_rmdt_long = 0x00;
        byte f_rmdt_quite = 0x00;

        public Form1()
        {

            InitializeComponent();
            comboBox_flap_ud.SelectedIndex = 0;
            comboBox_flap_lr.SelectedIndex = 0;
            comboBox_fan.SelectedIndex = 0;
            comboBox_nanoe.SelectedIndex = 0;
            comboBox_humidify.SelectedIndex = 0;
            comboBox_vent.SelectedIndex = 0;
            comboBox_AI.SelectedIndex = 0;
            comboBox_packet.SelectedIndex = 0;
            
            comboBox_mode.Text = "停止";
            comboBox_ope_sw.Text = "停止";


        }

        private void ntype_code_send(byte rem_No)
        {
            SafeFileHandle handle_usb_device = null;    // USB DEVICEハンドル
            byte[] code_dammy = new byte[8] { 0x02, 0x20, 0xE0, 0x07, 0x00, 0x00, 0x00, 0x09 };
            int i_ret = 0;

            try
            {
                // USB DEVICEオープン
                handle_usb_device = USBIR.openUSBIR(this.Handle);
                if (handle_usb_device != null)
                {
                    // USB DEVICEへ送信 パラメータ[USB DEVICEハンドル、周波数、リーダーコード、Bit0、Bit1、ストップコード、送信赤外線コード、赤外線コードのビット長]
                    // リーダーコード、Bit0、Bit1、ストップコード の 上位16bitはON時間　下位16bitはOFF時間
                    i_ret = USBIR.writeUSBIRCode(handle_usb_device, 38000, 0x00800060, 0x00100010, 0x00100035, 0x00100190, code_dammy, 64);
                    Thread.Sleep(150);
                    if (comboBox_packet.Text == "1+2パケット")
                    {
                        i_ret = USBIR.writeUSBIRCode(handle_usb_device, 38000, 0x00800060, 0x00100010, 0x00100035, 0x00100190, code_1packet, 152);
                        Thread.Sleep(300);
                        i_ret = USBIR.writeUSBIRCode(handle_usb_device, 38000, 0x00800060, 0x00100010, 0x00100035, 0x00100190, code_2packet, 152);
                    }
                    else if (comboBox_packet.Text == "1パケット")
                    {
                        i_ret = USBIR.writeUSBIRCode(handle_usb_device, 38000, 0x00800060, 0x00100010, 0x00100035, 0x00100190, code_1packet, 152);
                    }
                    else
                    {
                        i_ret = USBIR.writeUSBIRCode(handle_usb_device, 38000, 0x00800060, 0x00100010, 0x00100035, 0x00100190, code_2packet, 152);
                    }
               }

            }
            catch
            {
            }
            finally
            {
                if (handle_usb_device != null)
                {
                    // USB DEVICEクローズ
                    i_ret = USBIR.closeUSBIR(handle_usb_device);
                }
            }
        }

        private void comboBox_remNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox_remNo.Text)
            {
                case "A":
                    r_remocon_no = 0x00;
                    remocon_no_str = "0";
                    break;
                case "B":
                    r_remocon_no = 0x01;
                    remocon_no_str = "1";
                    break;
                case "C":
                    r_remocon_no = 0x02;
                    remocon_no_str = "2";
                    break;
                case "D":
                    r_remocon_no = 0x03;
                    remocon_no_str = "3";
                    break;
                default:
                    remocon_no_str = "0";
                    break;
           }

        }


        private void set_ntype_data()
        {
            code_1packet[3] = (byte)(0x04 + r_remocon_no);
            code_1packet[4] = 0x00;

            code_1packet[5] = (byte)((rmdt_mode << 4) + rmdt_onoff);
            code_1packet[6] = (byte)((byte)(rmdt_temp / 2) * 2);
            code_1packet[7] = (byte)(rmdt_humid);

            code_1packet[8] = (byte)(rmdt_fan + rmdt_flap_ud);   //風量：上下風向

            code_1packet[9] = (byte)(rmdt_vent_onoff + rmdt_flap_lr);   //換気：左右風向
            // 10-12 timer
            code_1packet[10] = (byte)(0x00);   //換気：左右風向
            code_1packet[11] = (byte)(0x0E);   //換気：左右風向
            code_1packet[12] = (byte)(0xE0);   //換気：左右風向

            code_1packet[14] = (byte)(((rmdt_temp % 2) << 7) + rmdt_SW + (rmdt_AI << 4));

            code_1packet[15] = 0x8D;

            code_1packet[16] = (byte)(0x00 + f_rmdt_motto);
            code_1packet[17] = 0x00;

            code_1packet[18] = 0;

            code_2packet[3] = (byte)(0x04 + r_remocon_no);

            code_2packet[5] = (byte)(0x08); //拡張08(2packet目)
            code_2packet[6] = (byte)(0x01);

            code_2packet[17] = (byte)(rmdt_push_sw);
            code_2packet[18] = 0;

            for (int i = 0; i < 18; i++)
            {
                code_1packet[18] = (byte)(code_1packet[18] + code_1packet[i]);
                code_2packet[18] = (byte)(code_2packet[18] + code_2packet[i]);
            }


        }
        private void update_disp_data()
        {
                set_ntype_data();
                label_1pac_0.Text = Convert.ToString((code_1packet[0]), 2).PadLeft(8, '0');
                label_1pac_1.Text = Convert.ToString((code_1packet[1]), 2).PadLeft(8, '0');
                label_1pac_2.Text = Convert.ToString((code_1packet[2]), 2).PadLeft(8, '0');
                label_1pac_3.Text = Convert.ToString((code_1packet[3]), 2).PadLeft(8, '0');
                label_1pac_4.Text = Convert.ToString((code_1packet[4]), 2).PadLeft(8, '0');
                label_1pac_5.Text = Convert.ToString((code_1packet[5]), 2).PadLeft(8, '0');
                label_1pac_6.Text = Convert.ToString((code_1packet[6]), 2).PadLeft(8, '0');
                label_1pac_7.Text = Convert.ToString((code_1packet[7]), 2).PadLeft(8, '0');
                label_1pac_8.Text = Convert.ToString((code_1packet[8]), 2).PadLeft(8, '0');
                label_1pac_9.Text = Convert.ToString((code_1packet[9]), 2).PadLeft(8, '0');
                label_1pac_10.Text = Convert.ToString((code_1packet[10]), 2).PadLeft(8, '0');
                label_1pac_11.Text = Convert.ToString((code_1packet[11]), 2).PadLeft(8, '0');
                label_1pac_12.Text = Convert.ToString((code_1packet[12]), 2).PadLeft(8, '0');
                label_1pac_13.Text = Convert.ToString((code_1packet[13]), 2).PadLeft(8, '0');
                label_1pac_14.Text = Convert.ToString((code_1packet[14]), 2).PadLeft(8, '0');
                label_1pac_15.Text = Convert.ToString((code_1packet[15]), 2).PadLeft(8, '0');
                label_1pac_16.Text = Convert.ToString((code_1packet[16]), 2).PadLeft(8, '0');
                label_1pac_17.Text = Convert.ToString((code_1packet[17]), 2).PadLeft(8, '0');
                label_1pac_18.Text = Convert.ToString((code_1packet[18]), 2).PadLeft(8, '0');

                label_2pac_0.Text = Convert.ToString((code_2packet[0]), 2).PadLeft(8, '0');
                label_2pac_1.Text = Convert.ToString((code_2packet[1]), 2).PadLeft(8, '0');
                label_2pac_2.Text = Convert.ToString((code_2packet[2]), 2).PadLeft(8, '0');
                label_2pac_3.Text = Convert.ToString((code_2packet[3]), 2).PadLeft(8, '0');
                label_2pac_4.Text = Convert.ToString((code_2packet[4]), 2).PadLeft(8, '0');
                label_2pac_5.Text = Convert.ToString((code_2packet[5]), 2).PadLeft(8, '0');
                label_2pac_6.Text = Convert.ToString((code_2packet[6]), 2).PadLeft(8, '0');
                label_2pac_7.Text = Convert.ToString((code_2packet[7]), 2).PadLeft(8, '0');
                label_2pac_8.Text = Convert.ToString((code_2packet[8]), 2).PadLeft(8, '0');
                label_2pac_9.Text = Convert.ToString((code_2packet[9]), 2).PadLeft(8, '0');
                label_2pac_10.Text = Convert.ToString((code_2packet[10]), 2).PadLeft(8, '0');
                label_2pac_11.Text = Convert.ToString((code_2packet[11]), 2).PadLeft(8, '0');
                label_2pac_12.Text = Convert.ToString((code_2packet[12]), 2).PadLeft(8, '0');
                label_2pac_13.Text = Convert.ToString((code_2packet[13]), 2).PadLeft(8, '0');
                label_2pac_14.Text = Convert.ToString((code_2packet[14]), 2).PadLeft(8, '0');
                label_2pac_15.Text = Convert.ToString((code_2packet[15]), 2).PadLeft(8, '0');
                label_2pac_16.Text = Convert.ToString((code_2packet[16]), 2).PadLeft(8, '0');
                label_2pac_17.Text = Convert.ToString((code_2packet[17]), 2).PadLeft(8, '0');
                label_2pac_18.Text = Convert.ToString((code_2packet[18]), 2).PadLeft(8, '0');

        }

        private void button92_Click(object sender, EventArgs e)
        {
            try
            {
                    set_ntype_data();
                    ntype_code_send(r_remocon_no);
            }
            catch
            {

            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Closed(object sender, FormClosedEventArgs e)
        {

        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
        }
        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void set_mode(String wk_mode, String wk_onoff)
        {
            switch (wk_mode)
            {
                case "暖房":
                    rmdt_push_sw = 0x03;
                    rmdt_mode = 0x04;
                    rmdt_onoff = 0x01;
                    break;
                case "快適おまかせ":
                    rmdt_push_sw = 0x05;
                    rmdt_mode = 0x00;
                    rmdt_onoff = 0x01;
                    break;
                case "冷房":
                    rmdt_mode = 0x03;
                    rmdt_onoff = 0x01;
                    rmdt_push_sw = 0x01;
                    break;
                case "除湿":
                    rmdt_mode = 0x02;
                    rmdt_push_sw = 0x02;
                    rmdt_onoff = 0x01;
                    break;
                case "冷房除湿":
                    rmdt_mode = 0x02;
                    rmdt_push_sw = 0x02;
                    rmdt_onoff = 0x01;
                    break;
                case "衣類乾燥":
                    rmdt_mode = 0x02;
                    rmdt_push_sw = 0x02;
                    rmdt_onoff = 0x01;
                    break;
                case "送風":
                    rmdt_push_sw = 0x07;
                    rmdt_mode = 0x06;
                    rmdt_onoff = 0x01;
                    break;
                case "停止":
                    rmdt_push_sw = 0x08;
                    rmdt_onoff = 0x00;
                    break;
                case "キープ暖房":
                    rmdt_push_sw = 0x04;
                    rmdt_onoff = 0x01;
                    rmdt_mode = 0x04;
                    rmdt_temp = 0xEF;
                    break;
                default:
                    rmdt_push_sw = 0x00;
                    rmdt_onoff = 0x00;
                    break;
            }
        }


        private void comboBox_mode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((comboBox_mode.Text == "暖房")
             || (comboBox_mode.Text == "冷房")
             || (comboBox_mode.Text == "冷房除湿")
             || (comboBox_mode.Text == "快適おまかせ"))
            {
                numericUpDown_temp.Text = numericUpDown_temp.Value.ToString();
                numericUpDown_temp.Enabled = true;
                numericUpDown_humid.Text = null;
                numericUpDown_humid.Enabled = false;
            }
            else if ((comboBox_mode.Text == "送風")
            || (comboBox_mode.Text == "衣類乾燥")
            || (comboBox_mode.Text == "キープ暖房"))
            {
                numericUpDown_temp.Text = null;
                numericUpDown_temp.Enabled = false;
                numericUpDown_humid.Text = null;
                numericUpDown_humid.Enabled = false;
            }
            else if (comboBox_mode.Text == "除湿")
            {
                numericUpDown_temp.Text = null;
                numericUpDown_temp.Enabled = false;
                numericUpDown_humid.Text = numericUpDown_humid.Value.ToString();
                numericUpDown_humid.Enabled = true;
            }
            else
            {
                numericUpDown_temp.Text = null;
                numericUpDown_temp.Enabled = false;
                numericUpDown_humid.Text = null;
                numericUpDown_humid.Enabled = false;
            }
            comboBox_ope_sw.Text = comboBox_mode.Text;
            if (comboBox_mode.Text == "停止")
            {
                set_mode(comboBox_mode.Text, "OFF");
            }
            else
            {
                set_mode(comboBox_mode.Text, "ON");
            }
            update_disp_data();
        }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e) /* 温度設定 */
        {
            rmdt_temp = (byte)(numericUpDown_temp.Value * 2);
            update_disp_data();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e) /* しつど設定 */
        {
            rmdt_humid = (byte)(numericUpDown_temp.Value);
            update_disp_data();
        }

        private void set_rmdt_flap_ud(string wk_flap_ud)
        {
            switch (wk_flap_ud)
            {
                case "上下1":
                    rmdt_flap_ud = 0x01;
                    break;
                case "上下2":
                    rmdt_flap_ud = 0x02;
                    break;
                case "上下3":
                    rmdt_flap_ud = 0x03;
                    break;
                case "上下4":
                    rmdt_flap_ud = 0x04;
                    break;
                case "上下5":
                    rmdt_flap_ud = 0x05;
                    break;
                case "上下スイング":
                    rmdt_flap_ud = 0x0E;
                    break;
                case "上下自動":
                    rmdt_flap_ud = 0x0F;
                    break;
                default:
                    break;
            }
        }

        private void set_rmdt_flap_lr(string wk_flap_lr)
        {
            switch (wk_flap_lr)
            {
                case "左右1":
                    rmdt_flap_lr = 0x06;
                    break;
                case "左右2":
                    rmdt_flap_lr = 0x07;
                    break;
                case "左右3":
                    rmdt_flap_lr = 0x08;
                    break;
                case "左右4":
                    rmdt_flap_lr = 0x09;
                    break;
                case "左右5":
                    rmdt_flap_lr = 0x0A;
                    break;
                case "左右6":
                    rmdt_flap_lr = 0x0B;
                    break;
                case "左右7":
                    rmdt_flap_lr = 0x0C;
                    break;
                case "左右スイング":
                    rmdt_flap_lr = 0x0D;
                    break;
                case "左右自動":
                    rmdt_flap_lr = 0x0E;
                    break;
                default:
                    break;
            }
        }

        private void set_rmdt_fan(string wk_fan)
        {
            f_rmdt_quite = 0x00;
            f_rmdt_powerfull = 0x00;
            f_rmdt_long = 0x00;
            switch (wk_fan)
            {
                case "風量1":
                    rmdt_fan = 0x30;
                    break;
                case "風量2":
                    rmdt_fan = 0x40;
                    break;
                case "風量3":
                    rmdt_fan = 0x50;
                    break;
                case "風量4":
                    rmdt_fan = 0x60;
                    break;
                case "風量5":
                    rmdt_fan = 0x70;
                    break;
                case "風量自動":
                    rmdt_fan = 0xA0;
                    break;
                case "しずか":
                    rmdt_fan = 0xA0;
                    f_rmdt_quite = 0x01;
                    break;
                case "パワフル":
                    rmdt_fan = 0xA0;
                    f_rmdt_powerfull = 0x01;
                    break;
                case "ロング":
                    rmdt_fan = 0xA0;
                    f_rmdt_long = 0x01;
                    break;
                default:
                    break;
            }
        }

        private void comboBox_flap_lr_SelectedIndexChanged(object sender, EventArgs e) /* 左右風向選択 */
        {
            set_rmdt_flap_lr(comboBox_flap_lr.Text);
            rmdt_push_sw = 0x0B;
            comboBox_ope_sw.Text = "左右風向";
            update_disp_data();
        }

        private void comboBox_fan_SelectedIndexChanged(object sender, EventArgs e) /* 風量選択 */
        {
            rmdt_push_sw = 0x0C;
            comboBox_ope_sw.Text = "風量";
            set_rmdt_fan(comboBox_fan.Text);
            update_disp_data();
        }
 
        private void comboBox_flap_ud_SelectedIndexChanged(object sender, EventArgs e) /* 上下風向選択 */
        {
            set_rmdt_flap_ud(comboBox_flap_ud.Text);
            rmdt_push_sw = 0x0A;
            comboBox_ope_sw.Text = "上下風向";
            update_disp_data();
        }

        private void comboBox_packet_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox_ope_sw_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
