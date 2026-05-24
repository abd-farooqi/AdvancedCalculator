using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AdvancedCalculator
{
    public partial class MainWindow : Window
    {
        // Standard Calculator State
        private string _standardCurrentInput = "0";
        private string _standardExpression = "";
        private double _standardValue = 0;
        private string _standardOperator = "";
        private bool _standardNewInput = true;
        
        // Scientific Calculator State
        private string _scientificExpression = "";
        private string _scientificDisplay = "0";
        private bool _scientificNewInput = true;
        private bool _useDegrees = true;
        
        // Memory
        private double _memory = 0;
        private bool _hasMemory = false;
        
        // History
        private bool _historyVisible = false;
        private List<HistoryItem> _history = new();
        
        public MainWindow()
        {
            InitializeComponent();
            KeyDown += MainWindow_KeyDown;
            UpdateStandardDisplay();
            UpdateScientificDisplay();
            UpdateMemoryIndicator();
        }

        // Window Controls - proper drag without stealing button clicks
        private bool _isDragging = false;
        private Point _dragStart;

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isDragging = true;
                _dragStart = e.GetPosition(this);
                CaptureMouse();
            }
        }

        private void TitleBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
            }
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPos = e.GetPosition(this);
                var offset = currentPos - _dragStart;
                Left += offset.X;
                Top += offset.Y;
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        // Keyboard Support
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (MainTabs.SelectedIndex == 0)
            {
                HandleStandardKey(e);
            }
            else if (MainTabs.SelectedIndex == 1)
            {
                HandleScientificKey(e);
            }
        }

        private void HandleStandardKey(KeyEventArgs e)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9) Number_Click(FindButton(e.Key.ToString().Replace("D", "")), null);
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) Number_Click(FindButton(e.Key.ToString().Replace("NumPad", "")), null);
            else if (e.Key == Key.Add) Operator_Click(FindButton("+"), null);
            else if (e.Key == Key.Subtract) Operator_Click(FindButton("−"), null);
            else if (e.Key == Key.Multiply) Operator_Click(FindButton("×"), null);
            else if (e.Key == Key.Divide) Operator_Click(FindButton("÷"), null);
            else if (e.Key == Key.Enter || e.Key == Key.Return) Equals_Click(FindButton("="), null);
            else if (e.Key == Key.Back) Backspace_Click(FindButton("⌫"), null);
            else if (e.Key == Key.Escape) ClearAll_Click(FindButton("C"), null);
            else if (e.Key == Key.OemPeriod || e.Key == Key.Decimal) Decimal_Click(FindButton("."), null);
        }

        private void HandleScientificKey(KeyEventArgs e)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9) SciNumber_Click(FindButton(e.Key.ToString().Replace("D", "")), null);
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) SciNumber_Click(FindButton(e.Key.ToString().Replace("NumPad", "")), null);
            else if (e.Key == Key.Add) SciOperator_Click(FindButton("+"), null);
            else if (e.Key == Key.Subtract) SciOperator_Click(FindButton("−"), null);
            else if (e.Key == Key.Multiply) SciOperator_Click(FindButton("×"), null);
            else if (e.Key == Key.Divide) SciOperator_Click(FindButton("÷"), null);
            else if (e.Key == Key.Enter || e.Key == Key.Return) SciEquals_Click(FindButton("="), null);
            else if (e.Key == Key.Back) SciBackspace();
            else if (e.Key == Key.Escape) SciClearAll_Click(FindButton("C"), null);
            else if (e.Key == Key.OemPeriod || e.Key == Key.Decimal) SciDecimal_Click(FindButton("."), null);
        }

        private Button FindButton(string content) => this.FindName("dummy") as Button;

        // Standard Calculator
        private void UpdateStandardDisplay()
        {
            StandardDisplay.Text = _standardCurrentInput;
            StandardExpression.Text = _standardExpression;
        }

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (_standardNewInput)
                {
                    _standardCurrentInput = btn.Content.ToString();
                    _standardNewInput = false;
                }
                else
                {
                    _standardCurrentInput += btn.Content.ToString();
                }
                UpdateStandardDisplay();
            }
        }

        private void Decimal_Click(object sender, RoutedEventArgs e)
        {
            if (_standardNewInput)
            {
                _standardCurrentInput = "0.";
                _standardNewInput = false;
            }
            else if (!_standardCurrentInput.Contains("."))
            {
                _standardCurrentInput += ".";
            }
            UpdateStandardDisplay();
        }

        private void Operator_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (!_standardNewInput)
                {
                    CalculateStandard();
                }
                _standardValue = double.Parse(_standardCurrentInput);
                _standardOperator = btn.Content.ToString();
                _standardExpression = $"{_standardCurrentInput} {_standardOperator}";
                _standardNewInput = true;
                UpdateStandardDisplay();
            }
        }

        private void Equals_Click(object sender, RoutedEventArgs e)
        {
            CalculateStandard();
            _standardExpression = "";
            _standardNewInput = true;
            UpdateStandardDisplay();
        }

        private void CalculateStandard()
        {
            if (string.IsNullOrEmpty(_standardOperator)) return;
            
            double second = double.Parse(_standardCurrentInput);
            double result = 0;
            bool valid = true;
            
            switch (_standardOperator)
            {
                case "+": result = _standardValue + second; break;
                case "−": result = _standardValue - second; break;
                case "×": result = _standardValue * second; break;
                case "÷":
                    if (second == 0) { valid = false; _standardCurrentInput = "Error"; }
                    else result = _standardValue / second;
                    break;
            }
            
            if (valid)
            {
                string expr = $"{_standardValue} {_standardOperator} {second}";
                _standardCurrentInput = FormatResult(result);
                AddToHistory(expr, _standardCurrentInput);
            }
            
            _standardValue = result;
            _standardOperator = "";
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            _standardCurrentInput = "0";
            _standardExpression = "";
            _standardValue = 0;
            _standardOperator = "";
            _standardNewInput = true;
            UpdateStandardDisplay();
        }

        private void ClearEntry_Click(object sender, RoutedEventArgs e)
        {
            _standardCurrentInput = "0";
            _standardNewInput = true;
            UpdateStandardDisplay();
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (_standardCurrentInput.Length > 1)
                _standardCurrentInput = _standardCurrentInput.Substring(0, _standardCurrentInput.Length - 1);
            else
                _standardCurrentInput = "0";
            UpdateStandardDisplay();
        }

        private void Percent_Click(object sender, RoutedEventArgs e)
        {
            double val = double.Parse(_standardCurrentInput);
            _standardCurrentInput = FormatResult(val / 100);
            UpdateStandardDisplay();
        }

        private void Square_Click(object sender, RoutedEventArgs e)
        {
            double val = double.Parse(_standardCurrentInput);
            string expr = $"sqr({_standardCurrentInput})";
            _standardCurrentInput = FormatResult(val * val);
            AddToHistory(expr, _standardCurrentInput);
            UpdateStandardDisplay();
        }

        private void SquareRoot_Click(object sender, RoutedEventArgs e)
        {
            double val = double.Parse(_standardCurrentInput);
            if (val < 0) { _standardCurrentInput = "Error"; }
            else
            {
                string expr = $"√({_standardCurrentInput})";
                _standardCurrentInput = FormatResult(Math.Sqrt(val));
                AddToHistory(expr, _standardCurrentInput);
            }
            UpdateStandardDisplay();
        }

        private void Reciprocal_Click(object sender, RoutedEventArgs e)
        {
            double val = double.Parse(_standardCurrentInput);
            if (val == 0) { _standardCurrentInput = "Error"; }
            else
            {
                string expr = $"1/({_standardCurrentInput})";
                _standardCurrentInput = FormatResult(1.0 / val);
                AddToHistory(expr, _standardCurrentInput);
            }
            UpdateStandardDisplay();
        }

        private void Negate_Click(object sender, RoutedEventArgs e)
        {
            if (_standardCurrentInput != "0" && _standardCurrentInput != "Error")
            {
                if (_standardCurrentInput.StartsWith("-"))
                    _standardCurrentInput = _standardCurrentInput.Substring(1);
                else
                    _standardCurrentInput = "-" + _standardCurrentInput;
            }
            UpdateStandardDisplay();
        }

        // Scientific Calculator
        private void UpdateScientificDisplay()
        {
            ScientificDisplay.Text = _scientificDisplay;
            ScientificExpression.Text = _scientificExpression;
        }

        private void SciNumber_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (_scientificNewInput)
                {
                    _scientificDisplay = btn.Content.ToString();
                    _scientificNewInput = false;
                }
                else
                {
                    _scientificDisplay += btn.Content.ToString();
                }
                UpdateScientificDisplay();
            }
        }

        private void SciDecimal_Click(object sender, RoutedEventArgs e)
        {
            if (_scientificNewInput)
            {
                _scientificDisplay = "0.";
                _scientificNewInput = false;
            }
            else if (!_scientificDisplay.Contains("."))
            {
                _scientificDisplay += ".";
            }
            UpdateScientificDisplay();
        }

        private void SciOperator_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                _scientificExpression += _scientificDisplay + " " + btn.Content.ToString() + " ";
                _scientificNewInput = true;
                UpdateScientificDisplay();
            }
        }

        private void SciEquals_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string expr = _scientificExpression + _scientificDisplay;
                string cleanExpr = expr.Replace("×", "*").Replace("÷", "/").Replace("−", "-").Replace("+", "+");
                double result = EvaluateExpression(cleanExpr);
                string formatted = FormatResult(result);
                AddToHistory(expr, formatted);
                _scientificExpression = "";
                _scientificDisplay = formatted;
                _scientificNewInput = true;
            }
            catch
            {
                _scientificDisplay = "Error";
                _scientificNewInput = true;
            }
            UpdateScientificDisplay();
        }

        private void SciClearAll_Click(object sender, RoutedEventArgs e)
        {
            _scientificDisplay = "0";
            _scientificExpression = "";
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciClearEntry_Click(object sender, RoutedEventArgs e)
        {
            _scientificDisplay = "0";
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciBackspace()
        {
            if (_scientificDisplay.Length > 1)
                _scientificDisplay = _scientificDisplay.Substring(0, _scientificDisplay.Length - 1);
            else
                _scientificDisplay = "0";
            UpdateScientificDisplay();
        }

        private void SciNegate_Click(object sender, RoutedEventArgs e)
        {
            if (_scientificDisplay != "0" && _scientificDisplay != "Error")
            {
                if (_scientificDisplay.StartsWith("-"))
                    _scientificDisplay = _scientificDisplay.Substring(1);
                else
                    _scientificDisplay = "-" + _scientificDisplay;
            }
            UpdateScientificDisplay();
        }

        private void SciSquare_Click(object sender, RoutedEventArgs e)
        {
            double val = double.Parse(_scientificDisplay);
            string expr = $"({_scientificDisplay})²";
            _scientificDisplay = FormatResult(val * val);
            AddToHistory(expr, _scientificDisplay);
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciPower_Click(object sender, RoutedEventArgs e)
        {
            _scientificExpression += _scientificDisplay + " ^ ";
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciFactorial_Click(object sender, RoutedEventArgs e)
        {
            double val = double.Parse(_scientificDisplay);
            if (val < 0 || val > 170 || val != Math.Floor(val))
            {
                _scientificDisplay = "Error";
            }
            else
            {
                string expr = $"{_scientificDisplay}!";
                _scientificDisplay = FormatResult(Factorial((int)val));
                AddToHistory(expr, _scientificDisplay);
            }
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciPi_Click(object sender, RoutedEventArgs e)
        {
            _scientificDisplay = Math.PI.ToString(CultureInfo.InvariantCulture);
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciE_Click(object sender, RoutedEventArgs e)
        {
            _scientificDisplay = Math.E.ToString(CultureInfo.InvariantCulture);
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciSin_Click(object sender, RoutedEventArgs e)
        {
            double val = double.Parse(_scientificDisplay);
            double angle = _useDegrees ? val * Math.PI / 180 : val;
            string expr = $"sin({_scientificDisplay})";
            _scientificDisplay = FormatResult(Math.Sin(angle));
            AddToHistory(expr, _scientificDisplay);
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciCos_Click(object sender, RoutedEventArgs e)
        {
            double val = double.Parse(_scientificDisplay);
            double angle = _useDegrees ? val * Math.PI / 180 : val;
            string expr = $"cos({_scientificDisplay})";
            _scientificDisplay = FormatResult(Math.Cos(angle));
            AddToHistory(expr, _scientificDisplay);
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciTan_Click(object sender, RoutedEventArgs e)
        {
            double val = double.Parse(_scientificDisplay);
            double angle = _useDegrees ? val * Math.PI / 180 : val;
            string expr = $"tan({_scientificDisplay})";
            _scientificDisplay = FormatResult(Math.Tan(angle));
            AddToHistory(expr, _scientificDisplay);
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciLog_Click(object sender, RoutedEventArgs e)
        {
            double val = double.Parse(_scientificDisplay);
            if (val <= 0) { _scientificDisplay = "Error"; }
            else
            {
                string expr = $"log({_scientificDisplay})";
                _scientificDisplay = FormatResult(Math.Log10(val));
                AddToHistory(expr, _scientificDisplay);
            }
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciLn_Click(object sender, RoutedEventArgs e)
        {
            double val = double.Parse(_scientificDisplay);
            if (val <= 0) { _scientificDisplay = "Error"; }
            else
            {
                string expr = $"ln({_scientificDisplay})";
                _scientificDisplay = FormatResult(Math.Log(val));
                AddToHistory(expr, _scientificDisplay);
            }
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciSqrt_Click(object sender, RoutedEventArgs e)
        {
            double val = double.Parse(_scientificDisplay);
            if (val < 0) { _scientificDisplay = "Error"; }
            else
            {
                string expr = $"√({_scientificDisplay})";
                _scientificDisplay = FormatResult(Math.Sqrt(val));
                AddToHistory(expr, _scientificDisplay);
            }
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciCbrt_Click(object sender, RoutedEventArgs e)
        {
            double val = double.Parse(_scientificDisplay);
            string expr = $"∛({_scientificDisplay})";
            _scientificDisplay = FormatResult(Math.Cbrt(val));
            AddToHistory(expr, _scientificDisplay);
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciPercent_Click(object sender, RoutedEventArgs e)
        {
            double val = double.Parse(_scientificDisplay);
            _scientificDisplay = FormatResult(val / 100);
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciParenOpen_Click(object sender, RoutedEventArgs e)
        {
            _scientificExpression += "( ";
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciParenClose_Click(object sender, RoutedEventArgs e)
        {
            _scientificExpression += _scientificDisplay + " ) ";
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void SciReciprocal_Click(object sender, RoutedEventArgs e)
        {
            double val = double.Parse(_scientificDisplay);
            if (val == 0) { _scientificDisplay = "Error"; }
            else
            {
                string expr = $"1/({_scientificDisplay})";
                _scientificDisplay = FormatResult(1.0 / val);
                AddToHistory(expr, _scientificDisplay);
            }
            _scientificNewInput = true;
            UpdateScientificDisplay();
        }

        private void DegRadToggle_Checked(object sender, RoutedEventArgs e)
        {
            _useDegrees = true;
            DegRadToggle.Content = "DEG";
            StatusBar.Text = "Mode: Degrees";
        }

        private void DegRadToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            _useDegrees = false;
            DegRadToggle.Content = "RAD";
            StatusBar.Text = "Mode: Radians";
        }

        // Expression Evaluator
        private double EvaluateExpression(string expr)
        {
            // Handle parentheses
            int openParen = expr.LastIndexOf('(');
            if (openParen >= 0)
            {
                int closeParen = expr.IndexOf(')', openParen);
                if (closeParen >= 0)
                {
                    string inner = expr.Substring(openParen + 1, closeParen - openParen - 1);
                    double innerResult = EvaluateExpression(inner);
                    expr = expr.Substring(0, openParen) + innerResult.ToString(CultureInfo.InvariantCulture) + expr.Substring(closeParen + 1);
                }
            }
            
            // Handle power
            int powerIdx = expr.IndexOf('^');
            if (powerIdx >= 0)
            {
                string left = ExtractNumberLeft(expr, powerIdx);
                string right = ExtractNumberRight(expr, powerIdx);
                double baseVal = double.Parse(left, CultureInfo.InvariantCulture);
                double expVal = double.Parse(right, CultureInfo.InvariantCulture);
                double result = Math.Pow(baseVal, expVal);
                expr = expr.Substring(0, powerIdx - left.Length) + result.ToString(CultureInfo.InvariantCulture) + expr.Substring(powerIdx + right.Length);
            }
            
            // Handle multiplication and division
            int mulIdx = expr.IndexOf('*');
            int divIdx = expr.IndexOf('/');
            int opIdx = -1;
            
            if (mulIdx >= 0 && (divIdx < 0 || mulIdx < divIdx)) opIdx = mulIdx;
            else if (divIdx >= 0) opIdx = divIdx;
            
            if (opIdx >= 0)
            {
                string left = ExtractNumberLeft(expr, opIdx);
                string right = ExtractNumberRight(expr, opIdx);
                double leftVal = double.Parse(left, CultureInfo.InvariantCulture);
                double rightVal = double.Parse(right, CultureInfo.InvariantCulture);
                double result = expr[opIdx] == '*' ? leftVal * rightVal : leftVal / rightVal;
                expr = expr.Substring(0, opIdx - left.Length) + result.ToString(CultureInfo.InvariantCulture) + expr.Substring(opIdx + right.Length);
            }
            
            // Handle addition and subtraction
            int addIdx = expr.IndexOf('+', 1);
            int subIdx = expr.IndexOf('-', 1);
            opIdx = -1;
            
            if (addIdx >= 0 && (subIdx < 0 || addIdx < subIdx)) opIdx = addIdx;
            else if (subIdx >= 0) opIdx = subIdx;
            
            if (opIdx >= 0)
            {
                string left = expr.Substring(0, opIdx).Trim();
                string right = expr.Substring(opIdx + 1).Trim();
                double leftVal = double.Parse(left, CultureInfo.InvariantCulture);
                double rightVal = double.Parse(right, CultureInfo.InvariantCulture);
                double result = expr[opIdx] == '+' ? leftVal + rightVal : leftVal - rightVal;
                return result;
            }
            
            return double.Parse(expr.Trim(), CultureInfo.InvariantCulture);
        }

        private string ExtractNumberLeft(string expr, int opIdx)
        {
            int i = opIdx - 1;
            while (i >= 0 && (char.IsDigit(expr[i]) || expr[i] == '.' || expr[i] == '-'))
            {
                if (expr[i] == '-' && i > 0 && !char.IsDigit(expr[i - 1]) && expr[i - 1] != ')') break;
                i--;
            }
            return expr.Substring(i + 1, opIdx - i - 1).Trim();
        }

        private string ExtractNumberRight(string expr, int opIdx)
        {
            int i = opIdx + 1;
            while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.')) i++;
            return expr.Substring(opIdx + 1, i - opIdx - 1).Trim();
        }

        // Memory Functions
        private void UpdateMemoryIndicator()
        {
            MemoryIndicator.Text = _hasMemory ? "M" : "";
        }

        private void MemoryClear_Click(object sender, RoutedEventArgs e)
        {
            _memory = 0;
            _hasMemory = false;
            UpdateMemoryIndicator();
            StatusBar.Text = "Memory cleared";
        }

        private void MemoryRecall_Click(object sender, RoutedEventArgs e)
        {
            if (_hasMemory)
            {
                _standardCurrentInput = FormatResult(_memory);
                _scientificDisplay = FormatResult(_memory);
                _standardNewInput = true;
                _scientificNewInput = true;
                UpdateStandardDisplay();
                UpdateScientificDisplay();
                StatusBar.Text = "Memory recalled: " + _memory;
            }
        }

        private void MemoryAdd_Click(object sender, RoutedEventArgs e)
        {
            double val = MainTabs.SelectedIndex == 0 ? double.Parse(_standardCurrentInput) : double.Parse(_scientificDisplay);
            _memory += val;
            _hasMemory = true;
            UpdateMemoryIndicator();
            StatusBar.Text = $"Memory: {_memory} (added {val})";
        }

        private void MemorySubtract_Click(object sender, RoutedEventArgs e)
        {
            double val = MainTabs.SelectedIndex == 0 ? double.Parse(_standardCurrentInput) : double.Parse(_scientificDisplay);
            _memory -= val;
            _hasMemory = true;
            UpdateMemoryIndicator();
            StatusBar.Text = $"Memory: {_memory} (subtracted {val})";
        }

        private void MemoryStore_Click(object sender, RoutedEventArgs e)
        {
            double val = MainTabs.SelectedIndex == 0 ? double.Parse(_standardCurrentInput) : double.Parse(_scientificDisplay);
            _memory = val;
            _hasMemory = true;
            UpdateMemoryIndicator();
            StatusBar.Text = $"Memory stored: {_memory}";
        }

        // History
        private void HistoryToggle_Click(object sender, RoutedEventArgs e)
        {
            _historyVisible = !_historyVisible;
            HistoryCol.Width = _historyVisible ? new GridLength(180) : new GridLength(0);
            HistoryList.ItemsSource = _history;
        }

        private void AddToHistory(string expression, string result)
        {
            _history.Add(new HistoryItem { Expression = expression, Result = result });
            HistoryPreview.Text = $"{expression} = {result}";
            StatusBar.Text = $"Calculated: {expression} = {result}";
        }

        // Age Calculator
        private void BirthDatePicker_Opened(object sender, RoutedEventArgs e)
        {
            // Default max to today
            if (BirthDatePicker.DisplayDate > DateTime.Today)
                BirthDatePicker.DisplayDate = DateTime.Today;
        }

        private void CalculateAge_Click(object sender, RoutedEventArgs e)
        {
            if (!BirthDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select your birth date", "No Date Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            DateTime birthDate = BirthDatePicker.SelectedDate.Value;
            DateTime today = DateTime.Today;
            
            if (birthDate > today)
            {
                MessageBox.Show("Birth date cannot be in the future", "Invalid Date", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Calculate age
            int years = today.Year - birthDate.Year;
            if (today.Month < birthDate.Month || (today.Month == birthDate.Month && today.Day < birthDate.Day))
                years--;
            
            int months = ((today.Month - birthDate.Month) + 12) % 12;
            if (today.Day < birthDate.Day) months--;
            if (months < 0) months += 12;
            
            TimeSpan ageSpan = today - birthDate;
            int totalDays = (int)ageSpan.TotalDays;
            int totalWeeks = totalDays / 7;
            int totalHours = (int)ageSpan.TotalHours;
            int totalMinutes = (int)ageSpan.TotalMinutes;
            int totalSeconds = (int)ageSpan.TotalSeconds;
            long heartbeats = (long)(totalMinutes * 72); // Average 72 bpm
            
            // Next birthday
            DateTime nextBirthday = new DateTime(today.Year, birthDate.Month, birthDate.Day);
            if (nextBirthday < today)
                nextBirthday = nextBirthday.AddYears(1);
            
            int daysUntilBirthday = (nextBirthday - today).Days;
            string dayOfWeek = nextBirthday.ToString("dddd");
            string monthsUntilBirthday = (nextBirthday - today).TotalDays >= 365 ? "1 year" : $"{(nextBirthday.Month - today.Month + 12) % 12} months";
            
            // Zodiac
            string zodiac = GetZodiacSign(birthDate.Month, birthDate.Day);
            string chineseZodiac = GetChineseZodiac(birthDate.Year);
            
            // Update UI
            AgeResults.Visibility = Visibility.Visible;
            
            AgeMainText.Text = $"{years} year{(years != 1 ? "s" : "")}, {months} month{(months != 1 ? "s" : "")}, {totalDays - (years * 365 + months * 30)} days";
            
            AgeYears.Text = years.ToString("N0");
            AgeMonths.Text = (years * 12 + months).ToString("N0");
            AgeWeeks.Text = totalWeeks.ToString("N0");
            AgeDays.Text = totalDays.ToString("N0");
            AgeHours.Text = totalHours.ToString("N0");
            AgeMinutes.Text = totalMinutes.ToString("N0");
            AgeSeconds.Text = totalSeconds.ToString("N0");
            AgeHeartbeats.Text = FormatLargeNumber(heartbeats);
            
            if (daysUntilBirthday == 0)
                NextBirthdayText.Text = "🎉 Today is your birthday! Happy Birthday! 🎂";
            else
                NextBirthdayText.Text = $"{daysUntilBirthday} days away";
            
            NextBirthdayDate.Text = nextBirthday.ToString("MMMM dd, yyyy");
            NextBirthdayDay.Text = $"({dayOfWeek}) - Turning {years + 1}";
            
            BornDayOfWeek.Text = birthDate.ToString("dddd, MMMM dd, yyyy");
            ZodiacSign.Text = zodiac;
            ChineseZodiac.Text = chineseZodiac;
            
            // Milestones
            UpdateMilestones(birthDate, today);
            
            StatusBar.Text = $"Age calculated for {birthDate:MMMM dd, yyyy}";
        }

        private void UpdateMilestones(DateTime birthDate, DateTime today)
        {
            MilestonesList.Children.Clear();
            
            long[] milestones = { 1_000_000, 10_000_000, 50_000_000, 100_000_000, 200_000_000, 500_000_000, 1_000_000_000 };
            string[] labels = { "1 Million", "10 Million", "50 Million", "100 Million", "200 Million", "500 Million", "1 Billion" };
            
            long currentSeconds = (long)(today - birthDate).TotalSeconds;
            
            for (int i = 0; i < milestones.Length; i++)
            {
                if (milestones[i] > currentSeconds)
                {
                    DateTime milestoneDate = birthDate.AddSeconds(milestones[i]);
                    int daysUntil = (milestoneDate - today).Days;
                    
                    var border = new Border
                    {
                        Background = (System.Windows.Media.Brush)FindResource("BgTertiary"),
                        CornerRadius = new CornerRadius(6),
                        Padding = new Thickness(10),
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    
                    var stack = new StackPanel();
                    stack.Children.Add(new TextBlock
                    {
                        Text = $"{labels[i]} Seconds Heartbeat",
                        Foreground = (System.Windows.Media.Brush)FindResource("Accent"),
                        FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                        FontSize = 14,
                        FontWeight = System.Windows.FontWeights.SemiBold
                    });
                    stack.Children.Add(new TextBlock
                    {
                        Text = milestoneDate.ToString("MMMM dd, yyyy"),
                        Foreground = (System.Windows.Media.Brush)FindResource("TextSecondary"),
                        FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                        FontSize = 12,
                        Margin = new Thickness(0, 3, 0, 0)
                    });
                    
                    border.Child = stack;
                    MilestonesList.Children.Add(border);
                    
                    if (MilestonesList.Children.Count >= 3) break;
                }
            }
            
            if (MilestonesList.Children.Count == 0)
            {
                var text = new TextBlock
                {
                    Text = "All milestones reached! 🎉",
                    Foreground = (System.Windows.Media.Brush)FindResource("Success"),
                    FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                    FontSize = 14,
                    TextAlignment = TextAlignment.Center
                };
                MilestonesList.Children.Add(text);
            }
        }

        private string GetZodiacSign(int month, int day)
        {
            return (month, day) switch
            {
                (>= 3 and <= 4, _) => month == 3 ? (day >= 21 ? "♈ Aries" : "♓ Pisces") : (day >= 20 ? "♉ Taurus" : "♈ Aries"),
                (5, _) => day >= 21 ? "♊ Gemini" : "♉ Taurus",
                (6, _) => day >= 21 ? "♋ Cancer" : "♊ Gemini",
                (7, _) => day >= 23 ? "♌ Leo" : "♋ Cancer",
                (8, _) => day >= 23 ? "♍ Virgo" : "♌ Leo",
                (9, _) => day >= 23 ? "♎ Libra" : "♍ Virgo",
                (10, _) => day >= 23 ? "♏ Scorpio" : "♎ Libra",
                (11, _) => day >= 22 ? "♐ Sagittarius" : "♏ Scorpio",
                (12, _) => day >= 22 ? "♑ Capricorn" : "♐ Sagittarius",
                (1, _) => day >= 20 ? "♒ Aquarius" : "♑ Capricorn",
                (2, _) => day >= 19 ? "♓ Pisces" : "♒ Aquarius",
                _ => "Unknown"
            };
        }

        private string GetChineseZodiac(int year)
        {
            string[] animals = { "🐒 Monkey", "🐓 Rooster", "🐕 Dog", "🐖 Pig", "🐀 Rat", "🐂 Ox", "🐅 Tiger", "🐇 Rabbit", "🐉 Dragon", "🐍 Snake", "🐎 Horse", "🐐 Goat" };
            return animals[year % 12];
        }

        // Utility
        private string FormatResult(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
                return "Error";
            
            if (value == Math.Floor(value) && Math.Abs(value) < 1e15)
                return value.ToString("N0", CultureInfo.InvariantCulture);
            
            string formatted = value.ToString("G15", CultureInfo.InvariantCulture);
            if (formatted.Contains("E"))
                return value.ToString("E6", CultureInfo.InvariantCulture);
            
            return formatted;
        }

        private string FormatLargeNumber(long value)
        {
            if (value >= 1_000_000_000)
                return $"{value / 1_000_000_000.0:F2}B";
            if (value >= 1_000_000)
                return $"{value / 1_000_000.0:F2}M";
            if (value >= 1_000)
                return $"{value / 1_000.0:F2}K";
            return value.ToString("N0");
        }

        private double Factorial(int n)
        {
            if (n <= 1) return 1;
            double result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;
            return result;
        }
    }

    public class HistoryItem
    {
        public string Expression { get; set; } = "";
        public string Result { get; set; } = "";
    }
}
