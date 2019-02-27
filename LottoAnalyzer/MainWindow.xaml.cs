using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Windows.Visibility;

namespace LottoAnalyzer
{
    //=================================//
    //Prefix E marks enums
    //Names of CONSTANTS are written in CAPITALS
    //=================================//

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            inputTextBox.Text = FetchFileFromUrl();
        }

        private string FetchFileFromUrl()   
        {
            string fileContent = null;
            try
            {
                var webRequest = WebRequest.Create(@"https://raw.githubusercontent.com/NetanelElimelech/LottoAnalyzer/master/LottoAnalyzer/allDraws.txt");

                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                using (var reader = new StreamReader(content))
                {
                    fileContent = reader.ReadToEnd();
                    reader.Close();
                }
            }

            catch
            {
                CustomMessageBox customMessageBox = new CustomMessageBox();
                customMessageBox.Show();
                customMessageBox.Title = "File not found";
            }

            return fileContent;
        }

        private void DisplayFileContent()
        {
            inputTextBox.Clear();
            outputTextBox.Clear();
            inputTextBox.Text = FetchFileFromUrl();
        }

        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayFileContent();
        }

        private void PrepareGUIforTextBoxes()
        {
            dataGridView.Visibility = Hidden;
            outputTextBox.Visibility = Visible;
            outputTextBox.Clear();
        }

        private void PrepareGUIforTableView()
        {
            outputTextBox.Visibility = Hidden;
            dataGridView.Visibility = Visible;
            outputTextBox.Clear();
        }

        private void CalculateRateButton_Click(object sender, RoutedEventArgs e)
        {
            PrepareGUIforTableView();
            DisplayFileContent();
            string fileContent = FetchFileFromUrl();
            int maxNumber = GetMaxNumber();

            dataGridView.ItemsSource = new RatingCombiner(fileContent, maxNumber).GetView();
        }

        

        private void ShowDrawsButton_Click(object sender, RoutedEventArgs e)
        {
            PrepareGUIforTextBoxes();
            DisplayFileContent();

            string fileContent = FetchFileFromUrl();
            int maxNumber = GetMaxNumber();

            string[] drawsArrayToBeDisplayed = new DrawsCombiner(fileContent, maxNumber).GetDrawsStringArray();

            for (int i = 0; i < drawsArrayToBeDisplayed.Length - 1; i++)
            {
                outputTextBox.AppendText($"{drawsArrayToBeDisplayed[i]}\n");
            }
        }

        private void LastAppearanceButton_Click(object sender, RoutedEventArgs e)
        {
            PrepareGUIforTableView();
            DisplayFileContent();

            string fileContent = FetchFileFromUrl();
            int maxNumber = GetMaxNumber();

            dataGridView.ItemsSource = new LastAppearanceCombiner(fileContent, maxNumber).GetView();

        }

        private int GetMaxNumber()
        {
            bool maxNumberIsInt = int.TryParse(maxNumberTextBox.Text, out int maxNumber);

            if (!maxNumberIsInt)
            {
                const string message = "Please provide the maximal possible number";
                const string caption = "No max. number provided";
                var result = MessageBox.Show(message, caption, MessageBoxButton.OK);
                maxNumberTextBox.Focus();
                return 0;
            }

            else
            {
                return maxNumber;
            }
        }

        private void CheckCombinationButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayFileContent();
            string fileContent = FetchFileFromUrl();
            string[] drawsArray = CustomArray.SeparateToLines(fileContent);

            string sequenceToCheck = "";
            bool sequenceAlreadyWon = false;
            bool valueIsInteger = false;
            int maxNumber = GetMaxNumber();

            GetMaxNumber();

            Dictionary<TextBox, string> textBoxes = new Dictionary<TextBox, string>()
            {
                { num1TextBox, num1TextBox.Text },
                { num2TextBox, num2TextBox.Text },
                { num3TextBox, num3TextBox.Text },
                { num4TextBox, num4TextBox.Text },
                { num5TextBox, num5TextBox.Text },
                { num6TextBox, num6TextBox.Text },
            };

            foreach (KeyValuePair<TextBox, string> pair in textBoxes)
            {
                valueIsInteger = int.TryParse(pair.Value, out int parsedNumber);

                if (!valueIsInteger || parsedNumber <= 0 || (parsedNumber > maxNumber && maxNumber != 0))
                {
                    const string message = "Some entries are invalid";
                    const string caption = "Invalid number";
                    var result = MessageBox.Show(message, caption, MessageBoxButton.OK);
                    pair.Key.Focus();
                    break;
                }

                else if (pair.Key != num6TextBox)
                {
                    sequenceToCheck += (pair.Value + "\t");
                }

                else
                {
                    sequenceToCheck += (pair.Value);
                }
            }

            if (valueIsInteger)
            {
                foreach (string item in drawsArray)
                {
                    sequenceAlreadyWon = item.Contains(sequenceToCheck);

                    if (sequenceAlreadyWon)
                    {
                        const string message = "This sequence of numbers already won";
                        const string caption = "Combination already won in the past";
                        var result = MessageBox.Show(message, caption, MessageBoxButton.OK);
                        break;
                    }
                }

                if (!sequenceAlreadyWon)
                {
                    const string messageNothingFound = "Nothing found";
                    const string captionNothingFound = "Nothing found";
                    var resultNothingFound = MessageBox.Show(messageNothingFound, captionNothingFound, MessageBoxButton.OK);
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<TextBox, string> textBoxes = new Dictionary<TextBox, string>()
            {
                { num1TextBox, "" },
                { num2TextBox, "" },
                { num3TextBox, "" },
                { num4TextBox, "" },
                { num5TextBox, "" },
                { num6TextBox, "" },
            };

            foreach (var pair in textBoxes)
            {
                pair.Key.Text = pair.Value;
            }
        }

        private int GetComboBoxValue(int value)
        {
            int comboBoxValue;
            switch (value)
            {
                case 0:
                    comboBoxValue = 2;
                    break;
                case 1:
                    comboBoxValue = 3;
                    break;
                case 2:
                    comboBoxValue = 4;
                    break;
                case 3:
                    comboBoxValue = 5;
                    break;
                case 4:
                    comboBoxValue = 6;
                    break;
                default:
                    comboBoxValue = 4;
                    break;
            }
            return comboBoxValue;
        }

        private void CheckConsequentCombinationsButton_Click(object sender, RoutedEventArgs e)
        {
            PrepareGUIforTextBoxes();
            drawLabel.Visibility = Hidden;

            string fileContent = FetchFileFromUrl();
            CombCombiner combCombiner = new CombCombiner(GetMaxNumber(), GetComboBoxValue(ConsequentComboBox.SelectedIndex), fileContent);

            inputTextBox.Text = combCombiner.GetCombinationsToBeCheckedAsString();
            outputTextBox.Text = combCombiner.GetComparedCombinationsAsString();
        }
    }
}