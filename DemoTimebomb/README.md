# Xsaya-Rsa 데모 자동 정리 런처

지정한 **만료일**이 지난 뒤 실행하면, 자신이 들어있는 **데모 폴더의 내용물
(하위 폴더·파일 전부)을 관리자 권한 없이 비워** 버리는 런처입니다. 폴더 자체는
남고 그 안이 빈 상태가 됩니다. 만료 전에는 하위 `Game` 폴더의 실제 게임을
대신 실행해 줍니다.

- 만료일은 **유니티 빌드 때 "빌드일 + 14일"로 자동 계산되어 exe 안에 컴파일**됩니다.
  (텍스트로 열어 고칠 수 없음. 일수는 `Assets/Editor/DemoTimebombPostBuild.cs` 의
  `ExpiryDays` 상수로 조정)
- 런처는 self-contained 단일 파일이라 대상 PC에 .NET 설치가 필요 없습니다.

---

## 1. 사용법 — 그냥 평소처럼 유니티에서 Windows 빌드하면 끝

`Assets/Editor/DemoTimebombPostBuild.cs` 후처리 훅이 빌드 완료 직후 자동으로:

1. 런처를 **빌드일 + 14일** 만료일로 컴파일(`dotnet publish`)하고
2. 유니티 산출물을 `Game/` 하위로 옮긴 뒤, 런처를 빌드 최상단에 배치

즉 별도 조작 없이 아래 구조가 자동으로 만들어집니다:

```
build/                       ← 만료 시 이 폴더의 "내용물"이 비워짐 (폴더는 남음)
├─ Xsaya-Rsa.exe             ← 런처 (심사자가 실행) — 만료일이 exe에 박혀 있음
└─ Game/                     ← 유니티 빌드 산출물 전체
   ├─ Xsaya-Rsa.exe          (진짜 게임)
   ├─ UnityPlayer.dll
   ├─ Xsaya-Rsa_Data/
   └─ ...
```

심사자는 최상단 `Xsaya-Rsa.exe`(런처)를 실행합니다.
- 만료 전 → `Game/Xsaya-Rsa.exe` 실행 (평범하게 게임 구동)
- 만료 후 → 안내창 표시 후 이 폴더의 **내용물을 전부 비움**
  (폴더 자체는 남고, 런처 exe 포함 내부 항목이 모두 삭제됨)

빌드 로그(Console)에 `[Timebomb] 자동 삭제 설치 완료. 만료일: ...` 이 찍히면 성공입니다.
`dotnet` 이 없거나 런처 빌드가 실패하면 에러만 찍고 **보호 없는 일반 빌드로 남깁니다**
(빌드 자체는 깨지지 않음).

> 대상: **Windows 스탠드얼론**(StandaloneWindows64/Windows) 빌드에만 적용됩니다.
> `dotnet` SDK 가 PATH 또는 `C:\Program Files\dotnet\` 에 있어야 합니다.

## 2. 만료 일수 변경

`Assets/Editor/DemoTimebombPostBuild.cs` 의 상수만 바꾸면 됩니다.

```csharp
const int ExpiryDays = 14;   // 빌드일 + N일
```

만료일은 이 값으로 매 빌드마다 자동 재계산되므로, 소스에 날짜를 손으로 박을 필요가
없습니다. 게임 실행 파일 이름/하위 폴더명을 바꾸려면 `src/Program.cs` 의
`GameExe` / `GameSubdir` 상수를 함께 수정하세요.

### 수동 빌드 (선택)

후처리 없이 런처만 따로 만들려면:

```
powershell -ExecutionPolicy Bypass -File build.ps1        # 기본: 만료 안 함(2099)
dotnet publish src/DemoLauncher.csproj -c Release -o dist -p:ExpiryLocal=2026-09-25T23:59:59
```

`ExpiryLocal` 을 주지 않으면 안전을 위해 **먼 미래(2099-01-01)** 로 컴파일되어
아무것도 지우지 않습니다.

## 3. 안전장치

런처는 **자기 자신이 놓인 폴더의 내용물**만 비우며, 그 경로가 아래에 해당하면
비우기를 거부합니다(안내창만 뜨고 아무것도 지우지 않음).

- 드라이브 루트(`C:\` 등)
- 드라이브 바로 아래 1단계 폴더
- 사용자 폴더 / 바탕화면 / 문서·사진·음악·동영상 / Windows / Program Files 등
  주요 폴더, 또는 그 상위(조상) 폴더

즉 압축을 풀 때 반드시 **자체 폴더(`Xsaya-Rsa_Demo/`) 안에** 두어야 하며,
바탕화면 최상단 등에 파일을 흩뿌리면 안전장치가 작동해 삭제되지 않습니다.

## 4. 한계 (반드시 인지)

로컬 시한장치의 본질적 한계로, 다음 방법으로 우회가 가능합니다.

1. PC 시계를 만료일 이전으로 되돌리기
2. 만료 전에 폴더를 다른 곳으로 복사해 두기
3. 런처를 지우고 `Game/` 안의 게임을 직접 실행하기

따라서 "정직한 심사자 대상, 평가 기간 이후 자동 정리" 용도로는 충분하지만,
강력한 복제 방지(DRM)는 아닙니다.

또한 서명되지 않은 실행 파일이라 **Windows SmartScreen / 백신**이 경고하거나
자동 삭제 동작을 의심스럽게 탐지할 수 있습니다. 제출 시 심사자에게
"평가 기간 후 자동 정리되는 데모"임을 미리 안내하면 오해를 줄일 수 있습니다.

## 5. 구성 파일

```
DemoTimebomb/
├─ src/
│  ├─ Program.cs           런처 + 자동 정리 로직 (만료일은 exe에 컴파일됨)
│  └─ DemoLauncher.csproj  self-contained 단일 exe 빌드 설정 (ExpiryLocal 주입)
├─ build.ps1               수동 빌드 스크립트
├─ dist/Xsaya-Rsa.exe      수동 빌드 산출물 (기본 만료 없음)
└─ README.md               이 문서

Assets/Editor/DemoTimebombPostBuild.cs   유니티 빌드 후처리 훅 (자동 설치)
```
