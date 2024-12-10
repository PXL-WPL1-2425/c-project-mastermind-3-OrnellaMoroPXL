using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mastermind
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _attempts = 0;
        private string[] _code = new string[4];

        private string[] _colors = new string[6]
        {
            "Red", // 0
            "Yellow", // 1
            "Orange", // 2
            "White", // 3
            "Green", // 4
            "Blue" // 5
        };

        private string _currentPlayer;

        //close message enkel mid-game met dit
        private bool _gameIsOngoing = true;

        private Border[,] _guessHistory = new Border[10, 4];
        private Label[] _labels = new Label[4];
        private string[] _playerGuess = new string[4];
        private List<string> _playerNames = new List<string>();
        private int _score = 100;

        // zogenaamde debug
        private bool _showSolution = false;

        public MainWindow()
        {
            InitializeComponent();

            _labels[0] = colorLabel1;
            _labels[1] = colorLabel2;
            _labels[2] = colorLabel3;
            _labels[3] = colorLabel4;

            StartGame();
        }

        // override > space > onClosING!! kiezen uit de dropdown
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (_gameIsOngoing)
            {
                MessageBoxResult exitMessage = MessageBox.Show(
                    $"Closing up? All unsolved codes will remain a mystery!", $"Attempt {_attempts}/10",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (exitMessage == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            ComboBoxItem selectedItem = comboBox.SelectedItem as ComboBoxItem;

            if (selectedItem != null)
            {
                string selectedColor = selectedItem.Content.ToString();

                if (comboBox == comboBox1)
                {
                    _playerGuess[0] = selectedColor;
                    colorLabel1.BorderBrush = Brushes.LightGray;
                    colorLabel1.BorderThickness = new Thickness(3);
                    colorLabel1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(selectedColor));
                }
                else if (comboBox == comboBox2)
                {
                    _playerGuess[1] = selectedColor;
                    colorLabel2.BorderBrush = Brushes.LightGray;
                    colorLabel2.BorderThickness = new Thickness(3);
                    colorLabel2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(selectedColor));
                }
                else if (comboBox == comboBox3)
                {
                    _playerGuess[2] = selectedColor;
                    colorLabel3.BorderBrush = Brushes.LightGray;
                    colorLabel3.BorderThickness = new Thickness(3);
                    colorLabel3.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(selectedColor));
                }
                else if (comboBox == comboBox4)
                {
                    _playerGuess[3] = selectedColor;
                    colorLabel4.BorderBrush = Brushes.LightGray;
                    colorLabel4.BorderThickness = new Thickness(3);
                    colorLabel4.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(selectedColor));
                }
            }
        }

        private void GenerateColorCode()
        {
            Random random = new Random();

            for (int i = 0; i < 4; i++)
            {
                int randomColorIndex = random.Next(6);
                _code[i] = _colors[randomColorIndex];
            }

            UpdateTitle();
        }

        private void NewGame()
        {
            // TODO: save player score & attempts

            for (int i = 0; i < _labels.Length; i++)
            {
                _labels[i].Background = Brushes.Transparent;
                _labels[i].BorderBrush = Brushes.LightGray;
                _labels[i].BorderThickness = new Thickness(1);
            }

            _attempts = 0;
            _score = 100;
            _guessHistory = new Border[10, 4];
            userGuessHistory.Children.Clear();

            UpdateAttempt();
            UpdateScore();
            UpdateHistory();

            GenerateColorCode();
        }

        private void StartGame()
        {
            bool addNewPlayer = true;

            while (addNewPlayer)
            {
                string playerName = Interaction.InputBox("Player name:", "Add player");

                while (string.IsNullOrWhiteSpace(playerName))
                {
                    MessageBox.Show("Please input a valid name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    playerName = Interaction.InputBox("Player name:", "Add player");
                }

                _playerNames.Add(playerName);

                MessageBoxResult addPlayerResult = MessageBox.Show("Would you like to add another player?", "Add player", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (addPlayerResult == MessageBoxResult.No)
                {
                    addNewPlayer = false;
                }
            }

            NewGame();
        }

        private void UpdateAttempt()
        {
            attemptLabel.Content = $"{_attempts}/10";
        }

        private void UpdateHistory()
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Border border = (Border)_guessHistory.GetValue(i, j);

                    if (border == null)
                    {
                        continue;
                    }

                    border.Height = 40;
                    border.Width = 40;
                    border.HorizontalAlignment = HorizontalAlignment.Center;
                    border.VerticalAlignment = VerticalAlignment.Center;
                    border.Margin = new Thickness(6);

                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);

                    if (!userGuessHistory.Children.Contains(border))
                    {
                        userGuessHistory.Children.Add(border);
                    }
                }
            }
        }

        private void UpdateScore()
        {
            scoreLabel.Content = _score;
        }

        private void UpdateTitle()
        {
            Title = $"Mastermind";
            if (_showSolution)
            {
                Title += $" ({string.Join(", ", _code)})";
            }
        }

        private void ValidateAnswers_Click(object sender, RoutedEventArgs e)
        {
            bool answerIsGuessed = true;

            for (int i = 0; i < 4; i++)
            {
                if (_code[i] == _playerGuess[i])
                {
                    _labels[i].BorderBrush = Brushes.Black;
                    _labels[i].BorderThickness = new Thickness(3);
                }
                else if (_code.Contains(_playerGuess[i]))
                {
                    _labels[i].BorderBrush = Brushes.Wheat;
                    _labels[i].BorderThickness = new Thickness(3);
                    //kleur op de foute plaats = -1 punt
                    _score = _score - 1;
                    answerIsGuessed = false;
                }
                else // als m'n kleur totaal niet aanwezig is, -2 punten
                {
                    _score = _score - 2;
                    answerIsGuessed = false;
                }
            }

            for (int i = 0; i < _labels.Length; i++)
            {
                _guessHistory[_attempts, i] = new Border()
                {
                    Width = 40,
                    Height = 40,
                    Background = _labels[i].Background,
                    BorderBrush = _labels[i].BorderBrush.Clone(),
                    BorderThickness = _labels[i].BorderThickness,
                    CornerRadius = new CornerRadius(20),
                    Margin = new Thickness(5)
                };
            }

            _attempts++;
            UpdateAttempt();
            UpdateScore();
            UpdateHistory();

            // als antwoord juist is -> "wil je verderspelen?"
            if (answerIsGuessed)
            {
                MessageBoxResult winMessage = MessageBox.Show(
                   $"You did it in {_attempts} tries! The code never stood a chance!\r\nUp for another round?", "WINNER", //<-message titel moet blijkbaar, anders error
                   MessageBoxButton.YesNo,
                   MessageBoxImage.Information);
                _gameIsOngoing = false;

                if (winMessage == MessageBoxResult.No)
                {
                    Close();
                }
                else
                {
                    NewGame();
                }
            }
            else if (_attempts == 10)
            {
                MessageBoxResult loseMessage = MessageBox.Show(
                    $"Close, but no cigar! The answer was: {string.Join(", ", _code)}.\r\nTry again?", "FAILED",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                _gameIsOngoing = false;

                if (loseMessage == MessageBoxResult.No)
                {
                    Close();
                }
                else
                {
                    NewGame();
                }
            }
        }
    }
}