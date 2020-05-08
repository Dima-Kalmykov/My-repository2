﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dangl.Calculator;

namespace WindowsFormsApp1
{
    public partial class GetLengthEdgeForm : Form
    {
        // Флаг, который показывает, нажали мы кнопку отмены, или нет.
        internal bool cancel;

        // Итоговое число.
        internal double number;

        internal GetLengthEdgeForm()
        {
            InitializeComponent();
        }

        private static bool IsNotSuitableNumber(double numb) =>
                   numb < 0.0001 ||
                   numb > Math.Pow(10, 4) ||
                   double.IsInfinity(numb) ||
                   double.IsNaN(numb);

        /// <summary>
        /// Обработка введённого текста,
        /// и получение итогового числа number.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            try
            {
                number = Calculator.Calculate(GetNumberTextBox.Text).Result;

                if (IsNotSuitableNumber(number))
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Number must be in range [0.0001 - 10^4]");
                GetNumberTextBox.Text = "";
                GetNumberTextBox.Focus();
                return;
            }

            cancel = false;

            GetNumberTextBox.Text = string.Empty;
            GetNumberTextBox.Focus();
            Close();
        }

        /// <summary>
        /// Устанавливаем нужные параметры флага,
        /// и перед выходом  очищаем TextBox и
        /// оставляем на нём фокус для последующего ввода.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            cancel = true;

            GetNumberTextBox.Text = "";
            GetNumberTextBox.Focus();
            Close();
        }

        /// <summary>
        /// Убираем верхнюю панель.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetLengthEdgeForm_Load(object sender, EventArgs e) =>
            ControlBox = false;
    }
}