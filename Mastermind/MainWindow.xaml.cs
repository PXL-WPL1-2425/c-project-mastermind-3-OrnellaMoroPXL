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

        private ComboBox[] _comboBoxes = new ComboBox[4];
        private string _currentPlayer;

        //close message enkel mid-game met dit
        private bool _gameIsOngoing = true;

        private Border[,] _guessHistory = new Border[10, 4];
        private HintWindow _hintWindow;
        private Label[] _labels = new Label[4];
        private string[] _playerGuess = new string[4];
        private List<string> _playerNames = new List<string>();
        private int _score = 100;

        // zogenaamde debug
        private bool _showSolution = true;

        public MainWindow()
        {
            InitializeComponent();

            _labels[0] = colorLabel1;
            _labels[1] = colorLabel2;
            _labels[2] = colorLabel3;
            _labels[3] = colorLabel4;

            _comboBoxes[0] = comboBox1;
            _comboBoxes[1] = comboBox2;
            _comboBoxes[2] = comboBox3;
            _comboBoxes[3] = comboBox4;

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

        private void ChooseCurrentPlayer()
        {
            _currentPlayer = NextPlayer();
            currentPlayerLabel.Content = _currentPlayer;
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

        private void HintButton_Click(object sender, RoutedEventArgs e)
        {
            _hintWindow = new HintWindow()
            {
                Owner = this
            };

            _hintWindow.Show();

            _hintWindow.correctColourButton.Click += HintCorrectColourButton_Click;
            _hintWindow.correctColourAndPositionButton.Click += HintCorrectColourAndPositionButton_Click;
        }

        private void HintCorrectColourAndPositionButton_Click(object sender, RoutedEventArgs e)
        {
            int hintIndex = -1;
            for (int i = 0; i < 4; i++)
            {
                if (hintIndex >= 0)
                {
                    continue;
                }

                if (_playerGuess[i] != _code[i])
                {
                    hintIndex = i;
                }
            }

            if (hintIndex >= 0)
            {
                _playerGuess[hintIndex] = _code[hintIndex];
                _comboBoxes[hintIndex].SelectedItem = _code[hintIndex]; // doet niks for some reason

                _labels[hintIndex].BorderBrush = Brushes.Black;
                _labels[hintIndex].BorderThickness = new Thickness(3);
                _labels[hintIndex].Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_code[hintIndex])); // label verandert eindelijk, maar combobox niet
                _labels[hintIndex].ToolTip = "Nice move! Or should we call it an investment?";

                _score -= 25;
                UpdateScore();
            }

            _hintWindow.Close();
        }

        private void HintCorrectColourButton_Click(object sender, RoutedEventArgs e)
        {
            string hint = null;
            for (int i = 0; i < 4; i++)
            {
                if (hint != null)
                {
                    continue;
                }

                if (!_playerGuess.Contains(_code[i]))
                {
                    hint = _code[i];
                }
            }

            if (hint != null)
            {
                MessageBox.Show(hint, "Hint");

                _score -= 15;
                UpdateScore();
            }

            _hintWindow.Close();
        }

        private void NewGame()
        {
            ChooseCurrentPlayer();

            if (string.IsNullOrEmpty(_currentPlayer))
            {
                // TODO: Include winner?
                MessageBoxResult answer = MessageBox.Show(
                    "The game has nothing left to give you... except for another round. Replay?",
                    "Game over!",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (answer == MessageBoxResult.Yes)
                {
                    RestartGame();
                }
                else
                {
                    _gameIsOngoing = false;

                    Close();
                }

                return;
            }

            for (int i = 0; i < 4; i++)
            {
                _labels[i].Background = Brushes.Transparent;
                _labels[i].BorderBrush = Brushes.LightGray;
                _labels[i].BorderThickness = new Thickness(1);

                _comboBoxes[i].SelectedItem = null;
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

        private string NextPlayer()
        {
            if (_currentPlayer == null)
            {
                return _playerNames.First();
            }

            int nextPlayerIndex = _playerNames.IndexOf(_currentPlayer) + 1;
            return _playerNames.ElementAtOrDefault(nextPlayerIndex); // out of bounds -> default
        }

        private void RestartGame()
        {
            // Clear players
            _playerNames.Clear();

            StartGame();
        }

        private void StartGame()
        {
            bool addNewPlayer = true;

            while (addNewPlayer)
            {
                string playerName = Interaction.InputBox("Player name:", "Add player");

                while (string.IsNullOrWhiteSpace(playerName))
                {
                    MessageBox.Show("Nice try, but we’re looking for a name, not a riddle.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    _labels[i].ToolTip = "This is what perfection looks like!";
                }
                else if (_code.Contains(_playerGuess[i]))
                {
                    _labels[i].BorderBrush = Brushes.Wheat;
                    _labels[i].BorderThickness = new Thickness(3);
                    //kleur op de foute plaats = -1 punt
                    _labels[i].ToolTip = "This colour is correct... somewhere else.";
                    _score = _score - 1;
                    answerIsGuessed = false;
                }
                else // als m'n kleur totaal niet aanwezig is, -2 punten
                {
                    _score = _score - 2;
                    answerIsGuessed = false;
                    _labels[i].ToolTip = "Wrong colour, but a stylish one!";
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

            if (answerIsGuessed || _attempts == 10)
            {
                string message = $"You did it in {_attempts} tries! The code never stood a chance!";

                if (_attempts == 10)
                {
                    message = $"Close, but no cigar! The answer was: {string.Join(", ", _code)}.";
                }

                string nextPlayer = NextPlayer();

                if (nextPlayer != null)
                {
                    message += $"\r\nNext player is: {nextPlayer}";
                }

                MessageBox.Show(
                    message,
                    _currentPlayer, // message titel moet blijkbaar, anders error
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                NewGame();
            }
        }
    }
}