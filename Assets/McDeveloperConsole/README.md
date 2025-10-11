# Mc Developer Console

[![Unity Version](https://img.shields.io/badge/Unity-2022.3%20LTS-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/badge/License-BSD--2--Clause-blue)](LICENSE)
[![Package Version](https://img.shields.io/badge/Version-1.0.1-orange.svg)](package.json)

A comprehensive in-game developer console for Unity with UI Toolkit support, auto-completion, command system, and customizable UI.

Unity UI Toolkit을 사용한 포괄적인 게임 내 개발자 콘솔입니다.

## ✨ Features

- 🎮 **In-Game Console**: Runtime에서 사용 가능한 개발자 콘솔
- ⌨️ **Auto-completion**: 명령어 자동 완성 기능
- 🔧 **Command System**: 리플렉션 기반의 명령어 시스템
- 📝 **Log Integration**: Unity 로그 시스템과 통합
- 🖱️ **Drag & Resize**: 드래그 및 크기 조절 기능

## 📦 Installation

### Package Manager (Recommended)

1. Unity Package Manager를 열어주세요
2. `+` 버튼을 클릭하고 `Add package from git URL`을 선택
3. 다음 URL을 입력: `https://github.com/machamy/McDeveloperConsole.git`

### Manual Installation

1. 이 저장소를 클론하거나 다운로드
2. `Assets/McDeveloperConsole` 폴더를 프로젝트에 복사
3. Unity에서 자동으로 패키지가 인식됩니다

## 🚀 Quick Start

### 1. 기본 설정

1. **ConsoleUI Prefab 추가**:
   ```
   Assets/McDeveloperConsole/Runtime/Prefabs/ConsoleUI.prefab
   ```
   를 씬에 드래그 앤 드롭

2. **Input Action 설정** (선택사항):
   - ConsoleUI 컴포넌트의 `Toggle Console Action`에 Input Action 할당
   - 기본값: ` 키로 콘솔 토글

### 2. 기본 사용법

```csharp
using Machamy.DeveloperConsole;

public class Example : MonoBehaviour
{
    void Start()
    {
        // 콘솔에 메시지 출력
        McConsole.MessageInfo("Hello, Console!");
        McConsole.MessageWarning("This is a warning");
        McConsole.MessageError("This is an error");
        
        // 로그 출력
        McConsole.Print("Debug message");
        McConsole.Print(LogType.Warning, "Warning message");
    }
}
```

### 3. 커스텀 명령어 생성

```csharp
using Machamy.DeveloperConsole;

public class MyCommands
{
    [ConsoleCommand("setHealth", "플레이어 체력을 설정합니다", "setHealth <value>")]
    public static void SetHealth(float health)
    {
        // 플레이어 체력 설정 로직
        McConsole.MessageInfo($"Health set to: {health}");
    }
    
    [ConsoleCommand("spawn", "오브젝트를 생성합니다", "spawn <objectName> <count>")]
    public static void SpawnObject(string objectName, int count)
    {
        // 오브젝트 생성 로직
        McConsole.MessageSuccess($"Spawned {count} {objectName}(s)");
    }
}
```

## 🎮 Controls

| 키 | 기능 |
|---|---|
| ` (백틱) | 콘솔 토글 |
| `Tab` | 자동 완성 |
| `↑/↓` | 명령어 히스토리 |
| `Enter` | 명령어 실행 |
| `Escape` | 콘솔 닫기 |

## 📚 API Reference

### McConsole 클래스

#### 정적 메서드

```csharp
// 메시지 출력
McConsole.Message(string message);
McConsole.Message(MessageType type, string message);
McConsole.MessageInfo(string message);
McConsole.MessageWarning(string message);
McConsole.MessageError(string message);
McConsole.MessageSuccess(string message);

// 로그 출력
McConsole.Print(string message);
McConsole.Print(LogType type, string message);

// 명령어 실행
McConsole.ExecuteCommand(string command);
```

#### 인스턴스 속성

```csharp
McConsole.Instance.LogPrintLevel; // 로그 출력 레벨 설정
McConsole.Instance.IsWindowOpen;  // 콘솔 창이 열려있는지 확인
```

### ConsoleUI 클래스

#### 주요 메서드

```csharp
ConsoleUI.Instance.Open();           // 콘솔 열기
ConsoleUI.Instance.Close();          // 콘솔 닫기
ConsoleUI.Instance.Toggle();         // 콘솔 토글
ConsoleUI.Instance.ClearHistory();   // 히스토리 지우기
ConsoleUI.Instance.SetOpacity(float); // 투명도 설정
```

### MessageType 열거형

```csharp
MessageType.Default   // 기본 메시지
MessageType.Info      // 정보 메시지
MessageType.Warning   // 경고 메시지
MessageType.Error     // 오류 메시지
MessageType.Debug     // 디버그 메시지
MessageType.Success   // 성공 메시지
MessageType.Gray      // 회색 메시지
MessageType.White     // 흰색 메시지
MessageType.Cyan      // 청록색 메시지
```

## 🎨 Customization

### UI 테마 변경

1. `Assets/McDeveloperConsole/Runtime/UI Toolkit/DebugConsole/console.uss` 파일 수정
2. CSS 스타일을 원하는 대로 변경

### 콘솔 설정

```csharp
// ConsoleUI 컴포넌트에서 설정 가능한 옵션들
public bool useAutoComplete = true;                    // 자동 완성 사용
public bool autoScrollToBottomOnNewMessage = true;     // 새 메시지 시 자동 스크롤
public bool autoScrollToBottomOnNewPrint = true;       // 새 프린트 시 자동 스크롤
public Vector2 minSize = new Vector2(360, 200);        // 최소 크기
public Vector2 maxSize = new Vector2(1920, 1200);      // 최대 크기
```

## 🔧 Built-in Commands

패키지에 포함된 기본 명령어들:

- `help` - 사용 가능한 명령어 목록 표시
- `help2` - RawCommand 버전의 도움말
- `ping` - 콘솔 연결 확인
- `echo <message>` - 메시지 출력
- `clear` - 콘솔 히스토리 지우기
- `autoScroll` - 자동 스크롤 토글
- `setOpacity <value>` - 콘솔 투명도 설정 (0.0-1.0)
- `setLogLevel <level>` - 로그 출력 레벨 설정 (0-4)
- `printAllLogTypes` - 모든 로그 타입 테스트

## 🎯 Advanced Usage

### 커스텀 명령어 클래스

```csharp
public class CustomCommand : IConsoleCommand
{
    public string Command => "custom";
    public string Description => "커스텀 명령어입니다";
    public string Signature => "custom <arg1> <arg2>";
    
    public void Execute(string[] args)
    {
        // 명령어 실행 로직
        McConsole.MessageInfo($"Custom command executed with args: {string.Join(", ", args)}");
    }
    
    public void AutoComplete(Span<string> args, ref List<string> suggestions)
    {
        // 자동 완성 로직
        if (args.Length == 1)
        {
            suggestions.Add("option1");
            suggestions.Add("option2");
        }
    }
}

// 명령어 등록
CommandLibrary.RegisterCommand(new CustomCommand());
```

### 로그 레벨 설정

```csharp
// 로그 출력 레벨 설정 (낮은 숫자일수록 더 많은 로그 출력)
McConsole.SetLogLevel(LogLevel.Info);     // 모든 로그 출력
McConsole.SetLogLevel(LogLevel.Warning);  // 경고 이상만 출력
McConsole.SetLogLevel(LogLevel.Error);    // 오류만 출력
McConsole.SetLogLevel(LogLevel.None);     // 로그 출력 안함
```

## 🐛 Troubleshooting

### 자주 묻는 질문

**Q: 콘솔이 나타나지 않아요**
A: ConsoleUI prefab이 씬에 있는지 확인하고, ` (백틱) 키를 눌러보세요.

**Q: 자동 완성이 작동하지 않아요**
A: ConsoleUI 컴포넌트의 `Use Auto Complete` 옵션이 활성화되어 있는지 확인하세요.

**Q: Input System 오류가 발생해요**
A: Unity Input System 패키지가 설치되어 있는지 확인하세요.

**Q: 커스텀 명령어가 등록되지 않아요**
A: 명령어 메서드가 `static`이고 `[ConsoleCommand]` 어트리뷰트가 있는지 확인하세요.

## 📋 Requirements

- Unity 2022.3 LTS 이상
- Unity Input System 1.6.0 이상
- Unity UI Toolkit 지원

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Made with ❤️ for the Unity community**
