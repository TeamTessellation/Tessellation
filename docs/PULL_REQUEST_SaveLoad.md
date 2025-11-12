---
<!-- 
   양식 : 'PR타입 : 제목'
   타입은 대문자로 적어야한다. 예) FEAT/FIX/REFACTOR
-->
# 🚀 FEAT : 세이브/로드 시스템 구현

## 📑 개요
게임 데이터를 저장하고 불러오는 세이브/로드 시스템과 히스토리 관리 시스템을 구현했습니다.
- `ISaveTarget` 인터페이스를 통한 저장 가능 객체 관리
- `GameData` 구조체를 통한 통합 데이터 관리
- `VariableContainer`를 통한 유연한 변수 저장
- `PlayerPrefs` 기반 간단한 저장/불러오기 기능
- 선형적 저장 히스토리 관리 시스템
- Unity Editor에서 히스토리 관리 기능 제공
- 박싱 회피를 위한 최적화된 변수 저장 구조

---

## ✏️ 변경(추가) 사항

### 1) VariableContainer (변수 컨테이너)
- 다양한 타입(int, float, string)의 값을 저장할 수 있는 컨테이너 클래스
- `Variable` 내부 클래스로 박싱을 피하는 최적화된 구조
- `SerializableDictionary`를 사용한 Unity 직렬화 지원
- `SetString/SetInteger/SetFloat`: 키-값 쌍으로 변수 저장
- `GetVariable/TryGetInteger/TryGetFloat/TryGetString`: 변수 조회
- `HasVariable`: 변수 존재 여부 확인
- `Clone()`: 깊은 복사를 통한 변수 컨테이너 복제

### 2) GameData (게임 데이터 구조체)
- 게임의 모든 저장 가능한 데이터를 담는 중앙 집중식 데이터 구조
- **내장 변수**: 
  - `CurrentStage`: 현재 월드 및 스테이지 인덱스
  - `TurnCount`: 턴 횟수
  - `TotalScore`: 누적 총점
  - `CurrentScore`: 현재 턴 점수
  - `PlayerStatus`: 플레이어 상태 정보
- **VariableContainer**: 추가적인 동적 변수 저장 (내장 변수 우선 사용 권장)
- `SaveVariable/GetVariable/GetIntVariable/GetFloatVariable/GetStringVariable`: 변수 접근 메서드
- `ContainsKey`: 변수 존재 여부 확인
- `Clone()`: 데이터 복제 기능

### 3) ISaveTarget 인터페이스
- 저장/불러오기가 가능한 객체를 위한 인터페이스
- `Guid Guid { get; init; }`: 고유 식별자
- `LoadData(GameData data)`: 데이터 불러오기
- `SaveData(ref GameData data)`: 데이터 저장

### 4) SaveLoadManager (세이브/로드 관리자)
- 싱글톤 패턴으로 구현된 중앙 집중식 저장/불러오기 관리자
- `RegisterSaveTarget(ISaveTarget)`: 저장 대상 객체 등록
- `UnregisterSaveTarget(ISaveTarget)`: 저장 대상 객체 등록 해제
- `RegisterPendingSavable(ISaveTarget)`: 인스턴스 생성 전 등록 대기
- `CreateCurrentSaveData()`: 모든 등록된 객체의 데이터를 수집하여 GameData 생성
- `LoadSaveData(GameData)`: GameData를 모든 등록된 객체에 전달
- **간단한 저장/불러오기**:
  - `SimpleSave()`: PlayerPrefs를 사용한 빠른 저장
  - `SimpleLoad(string, Action, Action)`: PlayerPrefs를 사용한 빠른 불러오기
  - `HasSimpleSave()`: 저장 데이터 존재 여부 확인
  - `HasSaveSimpleReliable()`: 저장 데이터 유효성 확인
- `DefaultExecutionOrder(-1000)`: 다른 스크립트보다 먼저 초기화

### 5) SaveHistory (저장 히스토리)
- `List<GameData>`를 래핑한 선형 저장 히스토리 컨테이너
- `IReadOnlyList<GameData>` 인터페이스 구현
- `Add(GameData)`: 새 저장 데이터 추가 (자동 복제)
- `GetLastSave()`: 마지막 저장 데이터 조회
- `GetSaveAt(int)`: 특정 인덱스의 저장 데이터 조회
- `PopLastSave()`: 마지막 저장 데이터 제거 및 반환
- `Clear()`: 모든 히스토리 삭제

### 6) HistoryManager (히스토리 관리자)
- 싱글톤 패턴으로 구현된 저장 히스토리 관리자
- `SaveCurrentState()`: 현재 게임 상태를 히스토리에 저장
- `LoadLastSave()`: 마지막 저장 상태로 복원
- `LoadAndPopLastSave()`: 마지막 저장 상태로 복원하고 히스토리에서 제거 (Undo 기능)
- `ClearHistory()`: 모든 히스토리 삭제
- **Unity Editor 통합**:
  - Inspector에서 히스토리 관리 버튼 제공
  - 메뉴바에서 `SaveLoad/Clear History` 명령 제공

---

## 📖사용 방법

### 기본 세이브/로드 사용법

```csharp
// 1. ISaveTarget 인터페이스 구현
public class PlayerController : MonoBehaviour, ISaveTarget
{
    public Guid Guid { get; init; } = Guid.NewGuid();
    
    private Vector3 playerPosition;
    private int playerHealth;
    
    private void OnEnable()
    {
        SaveLoadManager.RegisterPendingSavable(this);
    }
    
    private void OnDisable()
    {
        SaveLoadManager.Instance.UnregisterSaveTarget(this);
    }
    
    // 데이터 저장
    public void SaveData(ref GameData data)
    {
        data.SaveVariable("PlayerPosX", playerPosition.x);
        data.SaveVariable("PlayerPosY", playerPosition.y);
        data.SaveVariable("PlayerPosZ", playerPosition.z);
        data.SaveVariable("PlayerHealth", playerHealth);
    }
    
    // 데이터 불러오기
    public void LoadData(GameData data)
    {
        float x = data.GetFloatVariable("PlayerPosX");
        float y = data.GetFloatVariable("PlayerPosY");
        float z = data.GetFloatVariable("PlayerPosZ");
        playerPosition = new Vector3(x, y, z);
        
        playerHealth = data.GetIntVariable("PlayerHealth", 100);
    }
}

// 2. 게임 저장
public void SaveGame()
{
    SaveLoadManager.Instance.SimpleSave();
    Debug.Log("Game Saved!");
}

// 3. 게임 불러오기
public void LoadGame()
{
    bool success = SaveLoadManager.Instance.SimpleLoad(
        onComplete: () => Debug.Log("Game Loaded!"),
        onFail: () => Debug.LogError("Failed to load game!")
    );
}

// 4. 저장 데이터 존재 여부 확인
public bool CanContinue()
{
    return SaveLoadManager.Instance.HasSimpleSave();
}
```

### GameData 내장 변수 사용

```csharp
public class StageManager : MonoBehaviour, ISaveTarget
{
    public Guid Guid { get; init; } = Guid.NewGuid();
    
    public void SaveData(ref GameData data)
    {
        // 내장 변수 사용 (권장)
        data.CurrentStage = new int[] { currentWorld, currentStage };
        data.TurnCount = turnCount;
        data.TotalScore = totalScore;
        data.CurrentScore = currentScore;
    }
    
    public void LoadData(GameData data)
    {
        currentWorld = data.CurrentStage[0];
        currentStage = data.CurrentStage[1];
        turnCount = data.TurnCount;
        totalScore = data.TotalScore;
        currentScore = data.CurrentScore;
    }
}
```

### 히스토리 시스템 사용법

```csharp
// 1. 현재 상태 저장 (체크포인트)
public void CreateCheckpoint()
{
    HistoryManager.Instance.SaveCurrentState();
    Debug.Log("Checkpoint created!");
}

// 2. 마지막 저장 상태로 복원 (히스토리 유지)
public void RestoreCheckpoint()
{
    HistoryManager.Instance.LoadLastSave();
    Debug.Log("Restored to last checkpoint!");
}

// 3. 마지막 저장 상태로 복원 및 제거 (Undo)
public void UndoLastAction()
{
    var previousState = HistoryManager.Instance.LoadAndPopLastSave();
    if (previousState != null)
    {
        Debug.Log("Undone!");
    }
}

// 4. 히스토리 확인
public int GetCheckpointCount()
{
    return HistoryManager.Instance.SaveHistory.Count;
}

// 5. 특정 시점으로 복원
public void RestoreToCheckpoint(int index)
{
    var saveHistory = HistoryManager.Instance.SaveHistory;
    if (index >= 0 && index < saveHistory.Count)
    {
        SaveLoadManager.Instance.LoadSaveData(saveHistory[index]);
    }
}
```

### VariableContainer 직접 사용

```csharp
// VariableContainer를 직접 사용하는 경우
public void SaveCustomData(GameData data)
{
    // 다양한 타입 저장
    data.Variables.SetInteger("Level", 5);
    data.Variables.SetFloat("ExperienceMultiplier", 1.5f);
    data.Variables.SetString("PlayerName", "Hero");
}

public void LoadCustomData(GameData data)
{
    // TryGet 메서드 사용
    if (data.Variables.TryGetInteger("Level", out int level))
    {
        Debug.Log($"Level: {level}");
    }
    
    // 인덱서 사용
    var variable = data.Variables["ExperienceMultiplier"];
    if (variable != null)
    {
        float multiplier = variable.FloatValue;
        Debug.Log($"Multiplier: {multiplier}");
    }
}
```

---

## ⭐특징 및 주의사항

### 장점
- **중앙 집중식 관리**: SaveLoadManager를 통한 통합 저장/불러오기 관리
- **유연한 데이터 구조**: 내장 변수와 VariableContainer를 통한 확장 가능한 데이터 저장
- **박싱 회피**: Variable 클래스를 통한 성능 최적화
- **히스토리 시스템**: 선형적 저장 히스토리를 통한 체크포인트 및 Undo 기능
- **자동 등록**: RegisterPendingSavable을 통한 초기화 순서 문제 해결
- **Unity 직렬화**: SerializableDictionary와 [Serializable] 속성을 통한 Inspector 표시
- **Editor 통합**: Unity Editor에서 히스토리 관리 및 디버깅 기능 제공
- **안전한 불러오기**: 예외 처리와 콜백을 통한 안전한 데이터 로딩

### 주의사항
- **내장 변수 우선 사용**: GameData의 내장 변수를 우선적으로 사용하고, 동적 데이터만 VariableContainer 사용
- **Guid 관리**: ISaveTarget 구현 시 고유한 Guid가 필요 (현재는 사용되지 않지만 추후 확장 가능)
- **등록/해제 필수**: OnEnable/OnDisable에서 반드시 SaveTarget을 등록/해제해야 함
- **히스토리 메모리**: SaveHistory는 GameData를 복제하여 저장하므로 메모리 사용량 고려 필요
- **PlayerPrefs 한계**: SimpleSave는 PlayerPrefs를 사용하므로 대용량 데이터에는 부적합
- **ref 키워드**: SaveData 메서드는 `ref GameData`를 사용하여 구조체 복사 비용 감소
- **DefaultExecutionOrder**: SaveLoadManager는 -1000 순서로 다른 스크립트보다 먼저 초기화됨

### 확장 가능성
- 현재는 PlayerPrefs를 사용하지만, 파일 시스템이나 클라우드 저장으로 쉽게 확장 가능
- Guid를 활용한 개별 객체 저장/불러오기 구현 가능
- SaveHistory에 메타데이터(저장 시간, 스테이지 정보 등) 추가 가능
- 암호화나 압축 기능 추가 가능

---

## ⚠️알려진 문제 (Known Issues)

- `ISaveData` 인터페이스가 정의되어 있지만 현재 사용되지 않음 (추후 확장용)
- `Guid`가 ISaveTarget에 정의되어 있지만 현재 SaveLoadManager에서 활용되지 않음
- PlayerPrefs의 저장 크기 제한으로 인해 대용량 데이터 저장 시 문제 발생 가능
- SaveHistory가 무제한 증가할 수 있어 메모리 관리 필요 (최대 개수 제한 고려)

--- 

## ✅ 체크리스트
- [x] Namespace 규칙 확인 (SaveLoad)
- [x] public 함수의 경우 주석 확인
- [x] 박싱 회피를 위한 최적화 적용
- [x] Unity 직렬화 지원
- [x] Editor 통합 기능 제공

---

## 연관 PR

(없음)

---

## 연관 이슈

(없음)

