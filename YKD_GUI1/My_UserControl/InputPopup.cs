using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YKD_GUI1
{
    public partial class InputPopup : UserControl
    {

        public bool InputDone = false;
        public int SetValue = 0;

        public InputPopup()
        {
            InitializeComponent();
        }
        private void InputPopup_Load(object sender, EventArgs e)
        {
            button0.Click += ButtonInput_Click;
            button1.Click += ButtonInput_Click;
            button2.Click += ButtonInput_Click;
            button3.Click += ButtonInput_Click;
            button4.Click += ButtonInput_Click;
            button5.Click += ButtonInput_Click;
            button6.Click += ButtonInput_Click;
            button7.Click += ButtonInput_Click;
            button8.Click += ButtonInput_Click;
            button9.Click += ButtonInput_Click;

            button_BS.Click += Button_BS_Click;
            button_CLR.Click += Button_CLR_Click;
            button_ESC.Click += Button_ESC_Click;


            numericUpDown1.Value = SetValue;
        }

        private void Button_ESC_Click(object sender, EventArgs e)
        {
            Form win = this.FindForm();
            win?.Close();
        }

        private void Button_CLR_Click(object sender, EventArgs e)
        {
            numericUpDown1.Value = 0;
        }

        private void Button_BS_Click(object sender, EventArgs e)
        {
            int num = (int)numericUpDown1.Value;
            numericUpDown1.Value = num / 10;
        }

        private void ButtonInput_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            string num = numericUpDown1.Value.ToString();
            num = num + button.Tag.ToString();
            int.TryParse(num, out int number);
            if (number <= numericUpDown1.Maximum)
            {
                numericUpDown1.Value = number;
            }
            else
            {
                numericUpDown1.Value = numericUpDown1.Maximum;
            }

            

        }

        private void btn_ok_Click(object sender, EventArgs e)
        {

            SetValue = (int)numericUpDown1.Value;
            InputDone = true;
            Form win = this.FindForm();
            win?.Close();
        }

      
    }
}
