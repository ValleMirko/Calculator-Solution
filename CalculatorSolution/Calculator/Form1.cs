﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Calculator
{

    public partial class FormMain : Form
    {

        RichTextBox resultBox;
        int resultBoxTextSize = 24;

        private const char ASCIIZERO = '\0';
        private double operand1, operand2, result;
        private char lastOperator = ASCIIZERO;
        private ButtonStruct lastButtonClicked;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);


        public struct ButtonStruct
        {
            public char Content;
            public bool IsBold;
            public bool IsNumber;
            public bool IsDecimalSeparator;

            public bool IsPlusMinusSign;
            public bool IsOperator;
            public bool IsEqualSign;
            public bool IsSpecialOperator;
            public ButtonStruct(char content, bool isBold,
                bool isNumber = false, bool isDecimalSeparator = false,
                bool isPlusMinusSign = false, bool isOperator = false,
                bool isEqualSign = false, bool isSpecialOperator = false)
            {
                this.Content = content;
                this.IsBold = isBold;
                this.IsNumber = isNumber;
                this.IsDecimalSeparator = isDecimalSeparator;

                this.IsPlusMinusSign = isPlusMinusSign;
                this.IsOperator = isOperator;
                this.IsEqualSign = isEqualSign;
                this.IsSpecialOperator = isSpecialOperator;
            }
            public override string ToString()
            {
                return Content.ToString();
            }
        }

    private ButtonStruct[,] buttons =
    {
        { new ButtonStruct('%', false, false, false, false, true, false, true), new ButtonStruct('Œ', false), new ButtonStruct('C', false), new ButtonStruct('⌫', false) },
        { new ButtonStruct('⅟', false, false, false, false, true, false, true), new ButtonStruct('²', false, false, false, false, true, false, true), new ButtonStruct('⎷', false, false, false, false, true, false, true), new ButtonStruct('÷', false, false, false, false, true) },
        { new ButtonStruct('7', true, true), new ButtonStruct('8', true, true), new ButtonStruct('9', true, true), new ButtonStruct('×', false, false, false, false, true) },
        { new ButtonStruct('4', true, true), new ButtonStruct('5', true, true), new ButtonStruct('6', true, true), new ButtonStruct('-', false, false, false, false, true) },
        { new ButtonStruct('1', true, true), new ButtonStruct('2', true, true), new ButtonStruct('3', true, true), new ButtonStruct('+', false, false, false, false, true) },
        { new ButtonStruct('±', false, false, false, true), new ButtonStruct('0', true, true), new ButtonStruct(',', false, false, true), new ButtonStruct('=', false, false, false, false, false, true) }
    };

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            MakeResultBox();
            MakeButtons(buttons);
        }

        private void MakeResultBox()
        {
            resultBox = new RichTextBox();
            resultBox.ReadOnly = true;
            resultBox.SelectionAlignment = HorizontalAlignment.Right;
            resultBox.Font = new Font("Segoe UI", resultBoxTextSize, FontStyle.Regular);
            resultBox.Width = this.Width - 16;
            resultBox.Height = 120;
            resultBox.Text = "0";
            resultBox.TabStop = false;
            resultBox.GotFocus += ResultBox_HideCaretHandler;
            resultBox.MouseDown += ResultBox_HideCaretHandler;
            resultBox.SelectionChanged += ResultBox_HideCaretHandler;
            resultBox.TextChanged += ResultBox_TextChanged;
            this.Controls.Add(resultBox);
        }

        private void ResultBox_TextChanged(object sender, EventArgs e)
        {
            HideCaret(resultBox.Handle);
            if (lastButtonClicked.IsNumber)
            {
                string[] parts = resultBox.Text.Split(',');
                string leftOutput = "";
                for (int i = parts[0].Length - 1; i >= 0; i--)
                {
                    double intPart = double.Parse(parts[0]);
                    leftOutput = intPart.ToString("N0");
                }
                resultBox.Text = leftOutput;
                if (parts.Length > 1)
                {
                    resultBox.Text += "," + parts[1];
                }
            }
            int newSize = resultBoxTextSize - resultBox.Text.Length + 12;
            if (newSize > 8 && newSize < resultBoxTextSize + 1)
            {
                resultBox.Font = new Font("Segoe UI", newSize, FontStyle.Regular);
            }
            else if (newSize > 8)
            {
                resultBox.Font = new Font("Segoe UI", resultBoxTextSize, FontStyle.Regular);
            }
        }

        private string getFormattedNumber(double number)
        {
            //string stOut = number.ToString("N16").TrimEnd('0');
            //if (lastButtonClicked.Content == '0') stOut += '0';
            //if (!lastButtonClicked.IsDecimalSeparator) stOut = stOut.TrimEnd(',');
            string stOut = number.ToString("N16").TrimEnd('0').TrimEnd(',');
            return stOut;
        }

        private void ResultBox_HideCaretHandler(object sender, EventArgs e)
        {
            HideCaret(resultBox.Handle);
        }

        private void MakeButtons(ButtonStruct[,] buttons)
        {
            int buttonWidth = 80;
            int buttonHeight = 60;
            int posX = 0;
            int posY = 110;
            for (int i = 0; i < buttons.GetLength(0); i++)
            {
                for (int j = 0; j < buttons.GetLength(1); j++)
                {
                    ButtonStruct myButtonStruct = buttons[i, j];
                    Button myButton = new Button();
                    FontStyle myButtonStyle = myButtonStruct.IsBold ? FontStyle.Bold : FontStyle.Regular;
                    myButton.Font = new Font("Segoe UI", 16, myButtonStyle);
                    myButton.Text = myButtonStruct.ToString();
                    myButton.Width = buttonWidth;
                    myButton.Height = buttonHeight;
                    myButton.Left = posX;
                    myButton.Top = posY;
                    myButton.Tag = myButtonStruct;
                    myButton.Click += Button_Click;
                    this.Controls.Add(myButton);
                    posX += buttonWidth;
                }
                posX = 0;
                posY += buttonHeight;
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            ButtonStruct clickedButtonStructure = (ButtonStruct)clickedButton.Tag;
            if (clickedButtonStructure.IsNumber)
            {
                if (lastButtonClicked.IsEqualSign)
                {
                    clearAll();
                }
                if (resultBox.Text == "0" || lastButtonClicked.IsOperator)
                {
                    resultBox.Text = "";
                }
                lastButtonClicked = clickedButtonStructure;
                resultBox.Text += clickedButton.Text;
            }
            else
            {
                if (clickedButtonStructure.IsDecimalSeparator)
                {
                    if (!resultBox.Text.Contains(clickedButtonStructure.Content))
                    {
                        lastButtonClicked = clickedButtonStructure;
                        resultBox.Text += clickedButton.Text;
                    }
                }
                if (clickedButtonStructure.IsPlusMinusSign && resultBox.Text != "0")
                {
                    resultBox.Text = resultBox.Text.Contains("-") ? resultBox.Text.Replace("-", "") : "-" + resultBox.Text;
                }
                else
                {
                    switch (clickedButtonStructure.Content)
                    {
                        case 'Œ':
                            if (lastButtonClicked.IsEqualSign) clearAll();
                            else resultBox.Text = "0";
                            break;
                        case 'C':
                            clearAll();
                            break;
                        case '⌫':
                            resultBox.Text = resultBox.Text.Remove(resultBox.Text.Length - 1);
                            if (resultBox.Text.Length == 0 || resultBox.Text == "-" || resultBox.Text == "-0")
                            {
                                resultBox.Text = "0";
                            }
                            break;
                        default:
                            if (clickedButtonStructure.IsOperator || clickedButtonStructure.IsEqualSign) manageOperators(clickedButtonStructure);
                            break;
                    }
                }
            }
            lastButtonClicked = clickedButtonStructure;
        }

        private void clearAll()
        {
            operand1 = 0;
            operand2 = 0;
            result = 0;
            lastOperator = ASCIIZERO;
            resultBox.Text = "0";
        }

            private void manageOperators(ButtonStruct clickedButtonStructure)
            {
                if (clickedButtonStructure.IsSpecialOperator)
                {
                    double specialOperatorResult = 0;
                    switch (clickedButtonStructure.Content)
                    {
                        case '%':
                            specialOperatorResult = operand1 * double.Parse(resultBox.Text) / 100;
                            break;
                        case '⅟':
                            specialOperatorResult = 1 / double.Parse(resultBox.Text);
                            break;
                        case '²':
                            specialOperatorResult = Math.Pow(double.Parse(resultBox.Text), 2);
                            break;
                        case '⎷':
                            specialOperatorResult = Math.Sqrt(double.Parse(resultBox.Text));
                            break;
                    }
                    if (lastOperator == ASCIIZERO)
                    {
                        result = specialOperatorResult;
                        resultBox.Text = getFormattedNumber(result);
                    }
                    else
                    {
                        operand2 = specialOperatorResult;
                        resultBox.Text = getFormattedNumber(operand2);
                    }
                }
                else
                {
                    if (lastOperator == ASCIIZERO)
                    {
                        operand1 = double.Parse(resultBox.Text);
                        if (!clickedButtonStructure.IsEqualSign) lastOperator = clickedButtonStructure.Content;
                    }
                    else
                    {
                        if (!lastButtonClicked.IsEqualSign) operand2 = double.Parse(resultBox.Text);
                        if (lastButtonClicked.IsEqualSign && clickedButtonStructure.IsOperator) lastOperator = ASCIIZERO;
                        switch (lastOperator)
                        {
                            case '+':
                                result = operand1 + operand2;
                                break;
                            case '-':
                                result = operand1 - operand2;
                                break;
                            case '×':
                                result = operand1 * operand2;
                                break;
                            case '÷':
                                result = operand1 / operand2;
                                break;
                            default:
                                break;
                        }
                        operand1 = result;
                        if (!clickedButtonStructure.IsEqualSign)
                        {
                            lastOperator = clickedButtonStructure.Content;
                            operand2 = 0;
                        }
                        resultBox.Text = getFormattedNumber(result);
                    }
                }
            }

        private void FormMain_Activated(object sender, EventArgs e)
        {
            resultBox.TabStop = true;
        }
    }
}