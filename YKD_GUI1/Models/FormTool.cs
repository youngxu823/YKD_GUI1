using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YKD_GUI1.Models
{
    public static class FormTool
    {
        /// <summary>
        /// 弹出数值输入弹窗，返回输入结果
        /// </summary>
        /// <param name="owner">父窗口</param>
        /// <param name="initValue">输入框默认值</param>
        /// <returns>(是否确认, 输入的数值)</returns>
        public static (bool IsConfirm, int InputNum) ShowNumInputPopup(Form owner, int initValue)
        {
            using (Form popupForm = new Form())
            {
                InputPopup inputPopup = new InputPopup();

                popupForm.Owner = owner;
                popupForm.Size = inputPopup.Size;
                popupForm.StartPosition = FormStartPosition.CenterParent;
                popupForm.FormBorderStyle = FormBorderStyle.None;
                popupForm.MaximizeBox = false;
                popupForm.MinimizeBox = false;

                inputPopup.Dock = DockStyle.Fill;
                popupForm.Controls.Add(inputPopup);
                inputPopup.SetValue = initValue;

                popupForm.ShowDialog();

                return (inputPopup.InputDone, inputPopup.SetValue);
            }
        }
    }
}
