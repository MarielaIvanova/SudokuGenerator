using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;

namespace SUD
{
    
    public partial class MainWindow : Window
    {
        #region Sudoku
        readonly int MAX_TRIES = 30;
        readonly int NUMBEROFROWS = 9;

        int h = 0, m = 0, s = 0;
        int complexity = 45;

        Stack<KeyValuePair<TextBox, string>> history = new Stack<KeyValuePair<TextBox, string>>();
        Stack<KeyValuePair<TextBox, string>> redo = new Stack<KeyValuePair<TextBox, string>>();

        int posX, posY;
        int[,] emptyTable = new int[9, 9] { { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 } };
        int[,] sudokuTable = new int[9, 9] { { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 } };
        int[,] filledTable = new int[9, 9] { { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 } };

        bool gameStarted = false;
        bool LOADED = false;
        bool complexityEasy = true, complexityMed = false, complexityDiff = false;
        Random rnd = new Random();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Sudoko Table Create
        private void ShowOnTheBoard()
        {
            int i = 0, j = 0;
            int r = 0, c = 0;
            foreach (var item in grSudokoTable.Children)
            {
                j = 0;
                foreach (var box in (item as Grid).Children)
                {
                    // having i and j for each box we calculate which i and j is which row and column index of the small grids
                    r = (((int)Math.Floor((decimal)i / 3)) * 3) + (int)Math.Floor((decimal)j / 3);//1,2,3 са на 0ви ред, 3 4 5 на 1ви...
                    c = (((int)Math.Floor((decimal)i % 3)) * 3) + j % 3;

                    if (sudokuTable[r, c] == 0) (box as TextBox).Text = "";
                    else (box as TextBox).Text = "" + sudokuTable[r, c];
                    j++;
                }
                i++;
            }
        }

        private void FillTheTable()
        {
            FillDiagonal();

            while (!TryFillTheWholeTable())
            {
                sudokuTable = new int[9, 9];
                FillDiagonal();
            }
            for (int i = 0; i < NUMBEROFROWS; i++)
            {
                for (int j = 0; j < NUMBEROFROWS; j++)
                {
                    filledTable[i, j] = sudokuTable[i, j];
                }
            }

        }

        private void FillDiagonal()
        {
            FillBox(0, 0);
            FillBox(3, 3);
            FillBox(6, 6);
        }

        private void FillBox(int row, int col)
        {
            int num;
            int leftUpBoxRow, leftUpBoxCol;
            leftUpBoxRow = row - (row % 3);
            leftUpBoxCol = col - (col % 3);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //if (sudokuTable[leftUpBoxRow + i, leftUpBoxCol + j] != 0)
                    //{
                    do
                    {
                        num = rnd.Next(1, NUMBEROFROWS + 1);
                    }
                    while (!IsPossibleInSQ(leftUpBoxRow, leftUpBoxCol, num));
                    sudokuTable[leftUpBoxRow + i, leftUpBoxCol + j] = num;
                    // }
                }
            }
        }

        private bool IsPossibleInSQ(int r, int c, int currNum)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int deb = sudokuTable[r + i, c + j];
                    if (sudokuTable[r + i, c + j] == currNum) return false;
                }
            }
            return true;
        }

        private bool IsPossibleInRow(int row, int currN)
        {
            for (int i = 0; i < NUMBEROFROWS; i++)
            {
                if (sudokuTable[row, i] == currN) return false;
            }
            return true;
        }
        private bool IsPossibleInColumn(int col, int currN)
        {
            for (int j = 0; j < NUMBEROFROWS; j++)
            {
                if (sudokuTable[j, col] == currN) return false;
            }
            return true;
        }

        private bool TryFillTheWholeTable()
        {
            int curr;

            for (int i = 0; i < NUMBEROFROWS; i++)
            {
                for (int j = 0; j < NUMBEROFROWS; j++)
                {
                    int tries = 0;
                    if (sudokuTable[i, j] == 0)
                    {
                        do
                        {
                            tries++;
                            curr = rnd.Next(1, NUMBEROFROWS + 1);
                            if (tries == MAX_TRIES) return false;
                        }
                        while (!(IsPossibleInRow(i, curr) && IsPossibleInColumn(j, curr) && IsPossibleInSQ(i - (i % 3), j - (j % 3), curr)));

                        sudokuTable[i, j] = curr;
                    }
                }
            }
            return true;
        }

        private void RemoveDigits()
        {
            int numOfDigits = complexity;
            while (numOfDigits != 0)
            {
                int col = rnd.Next(0, NUMBEROFROWS);
                int row = rnd.Next(0, NUMBEROFROWS);
                if (sudokuTable[row, col] != 0)
                {
                    numOfDigits--;
                    sudokuTable[row, col] = 0;
                }
            }
        }
        #endregion

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LOADED)
            {
                int curr;
                TextBox txtBox = (TextBox)sender;
                if (txtBox.Text == "")
                {
                    txtBox.Background = Brushes.White;
                }
                else if (int.TryParse(txtBox.Text, out curr))
                {
                    string[] name = txtBox.Name.Split('_');
                    try
                    {
                        posX = Int32.Parse(name[1]);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine($"Unable to parse {name[1]}");
                    }
                    try
                    {
                        posY = Int32.Parse(name[2]);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine($"Unable to parse {name[2]}");
                    }

                    if (CheckIfCorrect(posX, posY, curr))
                    {
                        txtBox.Background = Brushes.Green;
                        txtBox.IsEnabled = false;
                        lblMessage.Content = "";
                        sudokuTable[posX, posY] = curr;
                        history.Push(new KeyValuePair<TextBox, string>(sender as TextBox, txtBox.Text));
                    }
                    else
                    {
                        txtBox.Background = Brushes.Red;
                        lblMessage.Content = "Wrong Number: " + curr;
                        history.Push(new KeyValuePair<TextBox, string>(sender as TextBox, txtBox.Text));
                    }
                }
                else
                {
                    lblMessage.Content = "INCORECT INPUT";
                }
            }
            else
            {
                lblMessage.Content = "GAME NOT LOADED";
            }
        }
        private bool CheckIfCorrect(int posX, int posY, int currN)
        {
            return filledTable[posX, posY] == currN;
        }

        #region buttonsClickMenu
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            if (LOADED)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "Text|*.txt";
                saveFileDialog1.Title = "Save a sudoku table";
                saveFileDialog1.ShowDialog();

                if (saveFileDialog1.FileName != "")
                {
                    StringBuilder values = new StringBuilder();
                    foreach (int candidate in sudokuTable)
                        values.Append(candidate);
                    foreach (int candidate in filledTable)
                        values.Append(candidate);
                    string res = values.ToString();
                    Stream fileStream = saveFileDialog1.OpenFile();
                    StreamWriter sw = new StreamWriter(fileStream);
                    sw.Write(res);
                    sw.Close();
                    fileStream.Close();

                }
            }
        }  
        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            if (LOADED)
            {
                Solve();
                dispatcherTimer.Stop();
            }
            
        }
        private void Solve()
        {
            for (int i = 0; i < NUMBEROFROWS; i++)
            {
                for (int j = 0; j < NUMBEROFROWS; j++)
                {
                    sudokuTable[i, j] = filledTable[i, j];
                }
            }
            ShowOnTheBoard();
        }
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            Load();
        }
        private void Load()
        {
            restartTheGame();

            LOADED = true;

            FillTheTable();

            RemoveDigits();

            ShowOnTheBoard();

        }
        private void restartTheGame()
        {
            for (int i = 0; i < NUMBEROFROWS; i++)
            {
                for (int j = 0; j < NUMBEROFROWS; j++)
                {
                    sudokuTable[i, j] = emptyTable[i, j];
                    filledTable[i, j] = emptyTable[i, j];
                }
            }
            h = 0;
            m = 0;
            s = 0;
            complexity = 45;

            Stack<KeyValuePair<TextBox, string>> history = new Stack<KeyValuePair<TextBox, string>>();
            Stack<KeyValuePair<TextBox, string>> redo = new Stack<KeyValuePair<TextBox, string>>();

            dispatcherTimer.Stop();
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            gameStarted = false;
            LOADED = false;
            complexityEasy = true;
            complexityMed = false;
            complexityDiff = false;

        }
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (LOADED && gameStarted)
            {
                if (history.Count != 0)
                {
                    KeyValuePair<TextBox, string> last = history.Pop();
                    redo.Push(new KeyValuePair<TextBox, string>(last.Key, last.Value));
                    if (history.Count == 0)//ако първото въведено число е правилно но искаме да направим undo
                    {
                        last.Key.IsEnabled = true;
                        last.Key.Text = "";
                        last.Key.Background = Brushes.White;

                    }
                    else if (last.Key == history.Peek().Key)
                    {
                        history.Peek().Key.Text = history.Peek().Value;
                        if (history.Peek().Key.Background == Brushes.Red || history.Peek().Key.Background == Brushes.White) last.Key.IsEnabled = true;
                    }
                    else
                    {
                        last.Key.Text = "";
                    }
                }
                else lblMessage.Content = "Can not undo anything more!";
            }
            else
            {
                lblMessage.Content = "Game not loaded";
            }
        }

        private void btnRedo_Click(object sender, RoutedEventArgs e)
        {
            if (LOADED)
            {
                if (redo.Count != 0)
                {
                    var last = redo.Pop();
                    (last.Key as TextBox).Text = last.Value;
                    history.Push(new KeyValuePair<TextBox, string>(last.Key, last.Value));
                }
                else lblMessage.Content = "Can not redo anything more!";
            }
            else
            {
                lblMessage.Content = "Game not loaded";
            }

        }

        private void btnEasy_Click(object sender, RoutedEventArgs e)
        {
            if (LOADED)
            {
                if (complexityMed == true)
                {
                    for (int i = 0; i < 13; i++) Help();
                }
                else if (complexityDiff == true)
                {
                    for (int i = 0; i < 25; i++) Help();
                }
                else
                {
                    complexity = 45;
                    for (int i = 0; i < NUMBEROFROWS; i++)
                    {
                        for (int j = 0; j < NUMBEROFROWS; j++)
                        {
                            sudokuTable[i, j] = filledTable[i, j];
                        }
                    }
                    RemoveDigits();
                }

                ShowOnTheBoard();

                MarkButton(btnEasy);

                complexityEasy = true;
                complexityMed = false;
                complexityDiff = false;
            }
            complexity = 45;
        }

        private void btnMedium_Click(object sender, RoutedEventArgs e)
        {
            if (LOADED)
            {
                if (complexityEasy == true)
                {
                    for (int i = 0; i < 13; i++) Remove();
                }
                else if (complexityDiff == true)
                {
                    for (int i = 0; i < 12; i++) Help();
                }
                else
                {
                    complexity = 58;
                    for (int i = 0; i < NUMBEROFROWS; i++)
                    {
                        for (int j = 0; j < NUMBEROFROWS; j++)
                        {
                            sudokuTable[i, j] = filledTable[i, j];
                        }
                    }
                    RemoveDigits();
                }

                MarkButton(btnMedium);

                ShowOnTheBoard();
                complexityEasy = false;
                complexityMed = true;
                complexityDiff = false;
            }
            complexity = 58;
        }

        private void btnHard_Click(object sender, RoutedEventArgs e)
        {
            if (LOADED)
            {
                if (complexityEasy == true)
                {
                    for (int i = 0; i < 25; i++) Remove();
                }
                else if (complexityMed == true)
                {
                    for (int i = 0; i < 12; i++) Remove();
                }
                else
                {
                    complexity = 70;
                    for (int i = 0; i < NUMBEROFROWS; i++)
                    {
                        for (int j = 0; j < NUMBEROFROWS; j++)
                        {
                            sudokuTable[i, j] = filledTable[i, j];
                        }
                    }
                    RemoveDigits();
                }

                MarkButton(btnHard);
                ShowOnTheBoard();
                complexityEasy = false;
                complexityMed = false;
                complexityDiff = true;
            }
            complexity = 70;
        }

        private void MarkButton(Button button)
        {
            btnEasy.FontWeight = FontWeights.Normal;
            btnMedium.FontWeight = FontWeights.Normal;
            btnHard.FontWeight = FontWeights.Normal;
            button.FontWeight = FontWeights.Bold;
        }
        private void btnHint_Click(object sender, RoutedEventArgs e)
        {
            if (LOADED)
            {
                Help();
                ShowOnTheBoard();
            }
            else
            {
                lblMessage.Content = "Game not loaded";
            }
        }
        private void Help()
        {
            int i, j;
            do
            {
                i = rnd.Next(0, NUMBEROFROWS);
                j = rnd.Next(0, NUMBEROFROWS);
            } while (sudokuTable[i, j] != 0);
            sudokuTable[i, j] = filledTable[i, j];
        }

        private void Remove()
        {
            int i, j;
            do
            {
                i = rnd.Next(0, NUMBEROFROWS);
                j = rnd.Next(0, NUMBEROFROWS);
            } while (sudokuTable[i, j] == 0);
            sudokuTable[i, j] = 0;
        }
        
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            //LOAD FROM FILE
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.ShowDialog();
                string strFileName = openFileDialog.FileName;
                string text = System.IO.File.ReadAllText(strFileName);
                for (int i = 0; i < NUMBEROFROWS; i++)
                {
                    for (int j = 0; j < NUMBEROFROWS; j++)
                    {
                        int position = i * 9 + j;
                        char currNumber = text[position];
                        sudokuTable[i, j] = (int)currNumber - 48;
                    }
                }
                for (int i = 0; i < NUMBEROFROWS; i++)
                {
                    for (int j = 0; j < NUMBEROFROWS; j++)
                    {
                        filledTable[i, j] = (int)text[(i + 9) * 9 + j]-48;
                    }
                }
            LOADED = true;

            ShowOnTheBoard();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (LOADED && !gameStarted)
            {
                gameStarted = true;
                foreach (var item in grSudokoTable.Children)
                {
                    foreach (var box in (item as Grid).Children)
                    {
                        if ((box as TextBox).Background == Brushes.White || (box as TextBox).Background == Brushes.Red) (box as TextBox).IsEnabled = true;
                    }
                }

                var timer = new DispatcherTimer(); // creating a new timer
                TimerTest();

            }
            else
            {
                lblMessage.Content = "The game is not loaded";
            }


        }
    
        private void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            if (LOADED && gameStarted)
            {
                dispatcherTimer.Stop();
                foreach (var item in grSudokoTable.Children)
                {
                    foreach (var box in (item as Grid).Children)
                    {
                        if ((box as TextBox).Background != Brushes.Green)
                        {
                            var result = MessageBox.Show("The sudoku is not solved. Do you want to finish the game?", "Finish Game?", MessageBoxButton.YesNo);
                            if (result == MessageBoxResult.Yes) Solve();
                            else
                            {
                                lblMessage.Content = "GOOD JOB";
                                return;
                            }
                        }
                    }
                }
                gameStarted = false;
                
            }
            else
            {
                lblMessage.Content = "The game is not started or loaded";
            }
        }
        #endregion

        #region Timer
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        private void TimerTest()
        {

            dispatcherTimer.Tick += new EventHandler((sender, e) => dispatcherTimer_Tick(sender, e, ref s, ref m, ref h));
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e, ref int s, ref int m, ref int h)
        {
            if (s + 1 == 60)
            {
                s = 0;
                m += 1;
            }
            else s += 1;
            if (m == 60)
            {
                m = 0;
                h += 1;
            }
            lblTime.Content = $"{h}:{m}:{s}";
        }
        #endregion


    }
}