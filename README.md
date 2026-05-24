# Advanced Calculator

A beautifully designed, feature-rich desktop calculator built with C# and WPF. Dark glass theme, scientific functions, and a comprehensive age calculator — all in one sleek app.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)

## ✨ Features

### 🧮 Standard Calculator
- Basic arithmetic: addition, subtraction, multiplication, division
- Percentage, square, square root, reciprocal
- Memory functions: MC, MR, M+, M−, MS
- Backspace, clear entry, clear all
- Full keyboard support

### 🔬 Scientific Calculator
- **Trigonometry:** sin, cos, tan
- **Logarithms:** log (base 10), ln (natural)
- **Powers:** x², xʸ, √, ∛
- **Constants:** π, e
- **Factorial:** x!
- **Other:** 1/x, %, ±, parentheses
- **DEG/RAD** toggle for angle modes
- Expression-based evaluation

### 📅 Age Calculator
- Select birthdate → get your exact age
- **Detailed breakdown:** years, months, weeks, days, hours, minutes, seconds
- **Heartbeats lived** (based on average 72 bpm)
- **Next birthday** countdown with day of week
- **Zodiac sign** (Western + Chinese)
- **Upcoming milestones** (1M, 10M, 100M seconds heartbeats)

### 🎨 Design
- Beautiful dark glass/neumorphism theme
- Smooth button hover and click animations
- History panel with past calculations
- Status bar with real-time feedback
- Draggable window with custom title bar

## 📥 Installation

### Option 1: Download (Recommended)
1. Go to the [Releases](https://github.com/abd-farooqi/AdvancedCalculator/releases) page
2. Download `AdvancedCalculator.exe`
3. Double-click to run — no installation needed!

### Option 2: Build from Source

**Requirements:**
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10/11

```bash
# Clone the repository
git clone https://github.com/abd-farooqi/AdvancedCalculator.git
cd AdvancedCalculator

# Build
dotnet build -c Release

# Run
dotnet run

# Publish standalone .exe
dotnet publish -c Release -o Release --self-contained false
```

## 🖥️ Screenshots

> *Coming soon — add screenshots once the app is running*

## 📁 Project Structure

```
AdvancedCalculator/
├── App.xaml                  # App entry point + global styles
├── App.xaml.cs
├── MainWindow.xaml           # Main UI layout
├── MainWindow.xaml.cs        # Calculator logic + age calculator
├── AdvancedCalculator.csproj # Project file
└── .gitignore
```

## ⌨️ Keyboard Shortcuts

| Key | Action |
|---|---|
| `0-9` | Input numbers |
| `+ - * /` | Operators |
| `Enter` | Equals |
| `Backspace` | Delete last digit |
| `Escape` | Clear all |
| `.` | Decimal point |

## 🛠️ Tech Stack

- **Language:** C#
- **Framework:** .NET 8.0 WPF
- **Styling:** XAML with custom resource dictionaries
- **No external dependencies** — pure .NET

## 📄 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

## 🙏 Contributing

Contributions are welcome! Feel free to:
- Report bugs
- Suggest features
- Submit pull requests

## 📧 Contact

- GitHub: [@abd-farooqi](https://github.com/abd-farooqi)
