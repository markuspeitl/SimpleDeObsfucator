using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StringReverser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String splitChars;

        public MainWindow()
        {
            InitializeComponent();

            splitChars = "(\\.|\\*|\\+|\\-|\\^|\\{|\\}|\\(|\\)|\\ |\\,|\\;)";

            ConvertButton.Click += OnConvertButtonClicked;

            AddKeyButton.Click += OnAddKeyClicked;

        }

        private void OnAddKeyClicked(object sender, RoutedEventArgs e)
        {
            String key = KeyTextBox.Text;
            String value = ValueTextBox.Text;

            if (key != "" && value != "")
            {
                String text = OutputStringBox.Text;

                text = ReplaceOnlyFullString(text,key, value);

                OutputStringBox.Text = text;

                KeyTextBox.Text = "";
                ValueTextBox.Text = "";
            }
        }

        private void OnConvertButtonClicked(object sender, RoutedEventArgs e)
        {
            if (InputStringBox != null)
            {
                String input = InputStringBox.Text;
                String output = ConvertStrings(input);
                output = InsertMethodParameterCounts(output);

                OutputStringBox.Text = output;
            }
        }

        public String ReplaceOnlyFullString(String sourceString,String fullString, String replaceString)
        {
            String[] splitStrings = Regex.Split(sourceString, splitChars);
            String result = "";

            for (int i = 0; i < splitStrings.Length; i++)
            {
                if (splitStrings[i] == fullString)
                {
                    splitStrings[i] = replaceString;
                }

                result += splitStrings[i];
            }

            return result;
        }

        int thresholdChars = 30;
        public String ConvertStrings(String inputString)
        {
            String[] splitStrings = Regex.Split(inputString, splitChars);
            String result = "";

            for (int i = 0; i < splitStrings.Length; i++)
            {
                if (splitStrings[i].Length >= thresholdChars)
                {
                    String hexString = ConvertToHexString(splitStrings[i]);
                    hexString = RemoveSequences(hexString);
                    splitStrings[i] = hexString;
                }

                result += splitStrings[i];
            }

            return result;
        }

        //Some Obsfuctors use the same Addresses for Methods and overload them at runtime so we insert the number of parameters into the Hex - Method String
        public String InsertMethodParameterCounts(String input)
        {
            String output = "";

            String methodParameters = "";

            bool counting = false;
            int parameterCount = 1;

            int stringLenght = input.Length;

            for (int i = 0; i < stringLenght; i++)
            {
                if (input[i].Equals('('))
                {
                    if (i > 0)
                    {
                        int test;
                        if (Int32.TryParse(""+input[i - 1],out test))
                        {

                            counting = true;

                            if (stringLenght > i + 1)
                            {
                                if (input[i + 1].Equals(')'))
                                {
                                    parameterCount = 0;
                                }
                            }
                        }
                    }

                }
                if (counting)
                {
                    methodParameters += input[i];
                    if (input[i].Equals(','))
                    {
                        parameterCount++;
                    }

                }
                else
                {
                    output += input[i];
                }
                if (input[i].Equals(')') && counting)
                {
                    counting = false;
                    String countstring = "" + parameterCount;
                    output += "" + countstring + methodParameters;
                    //i += countstring.Length;
                    //stringLenght += countstring.Length;
                    parameterCount = 1;
                    methodParameters = "";
                }
                
            }

            return output;
        }

        public String ConvertToHexString(String normalString)
        {
            byte[] ba = Encoding.Default.GetBytes(normalString);
            return BitConverter.ToString(ba).Replace("-","");
        }

        public String RemoveSequences(String input)
        {
            List<int> sequenceCounter = new List<int>();
            List<string> sequenceChars = new List<string>();
            int sequencecounter = 0;

            bool sequenceStarted = false;
            for (int i = 0; i < input.Length - 3; i = i + 2)
            {
                if (input.ElementAt(i).Equals(input.ElementAt(i + 2)) && input.ElementAt(i + 1).Equals(input.ElementAt(i + 3)))
                {
                    if (!sequenceStarted)
                    {
                        sequencecounter = 1;
                        sequenceChars.Add("" + input.ElementAt(i) + input.ElementAt(i + 1));
                        sequenceStarted = true;
                    }
                    else
                    {
                        sequencecounter++;
                    }
                }
                else
                {
                    if (sequenceStarted)
                    {
                        sequenceCounter.Add(sequencecounter);
                        sequencecounter = 0;
                    }

                    sequenceStarted = false;
                }
            }

            if (sequencecounter != 0)
            {
                sequenceCounter.Add(sequencecounter);
            }

            int biggestSequenceIndex = -1;

            for (int i = 0; i < sequenceCounter.Count; i++)
            {
                if (sequenceCounter[i] >= 3)
                {
                    if (biggestSequenceIndex == -1)
                        biggestSequenceIndex = 0;

                    if (sequenceCounter[biggestSequenceIndex] < sequenceCounter[i])
                    {
                        biggestSequenceIndex = i;
                    }
                }
            }

            if (biggestSequenceIndex == -1)
                return input;

            String replaceSequence = "";
            for (int i = 0; i < sequenceCounter[biggestSequenceIndex]; i++)
            {
                replaceSequence += sequenceChars[biggestSequenceIndex];
            }

            return input.Replace(replaceSequence,"");
        }

    }
}
