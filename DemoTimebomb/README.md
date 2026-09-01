# Xsaya-Rsa 데모 자동 정리 런처

지정한 **만료일**이 지난 뒤 실행하면, 자신이 들어있는 **데모 폴더의 내용물
(하위 폴더·파일 전부)을 관리자 권한 없이 비워** 버리는 런처입니다. 폴더 자체는
남고 그 안이 빈 상태가 됩니다. 만료 전에는 하위 `Game` 폴더의 실제 게임을
대신 실행해 줍니다.

- 현재 만료일: **2026-09-09 23:59:59** (PC 로컬 시간)
- 산출물: `dist/Xsaya-Rsa.exe` (self-contained 단일 파일, 대상 PC에 .NET 설치 불필요)

---

## 1. 배포 폴더 구성

유니티를 **평소대로 빌드**한 뒤, 아래 구조로 재배치해서 압축·제출하세요.
핵심은 **실제 유니티 빌드를 `Game/` 하위 폴더로 넣고, 런처를 최상단에 두는 것**입니다.

```
Xsaya-Rsa_Demo/              ← 이 폴더 전체가 만료 시 삭제됨 (심사자에게 주는 폴더)
├─ Xsaya-Rsa.exe             ← 이 런처 (dist/Xsaya-Rsa.exe 를 복사)
└─ Game/                     ← 유니티 빌드 산출물을 통째로 이동
   ├─ Xsaya-Rsa.exe          (진짜 게임)
   ├─ UnityPlayer.dll
   ├─ Xsaya-Rsa_Data/
   └─ ...
```

심사자는 최상단 `Xsaya-Rsa.exe`(런처)를 실행합니다.
- 만료 전 → `Game/Xsaya-Rsa.exe` 실행 (평범하게 게임 구동)
- 만료 후 → 안내창 표시 후 `Xsaya-Rsa_Demo/` 폴더의 **내용물을 전부 비움**
  (폴더 자체는 남고, 런처 exe 포함 내부 항목이 모두 삭제됨)

## 2. 만료일 변경 / 재빌드

날짜를 바꾸려면 `src/Program.cs` 상단의 `Expiry` 값을 수정한 뒤 다시 빌드합니다.

```csharp
static readonly DateTime Expiry =
    new DateTime(2026, 9, 9, 23, 59, 59, DateTimeKind.Local);
```

빌드:

```
powershell -ExecutionPolicy Bypass -File build.ps1
```

결과물은 `dist/Xsaya-Rsa.exe` 에 생성됩니다.
게임 실행 파일 이름이 `Xsaya-Rsa.exe` 가 아니거나 하위 폴더명을 바꾸고 싶으면
`Program.cs` 의 `GameExe` / `GameSubdir` 상수도 함께 수정하세요.

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
│  ├─ Program.cs           런처 + 자동 정리 로직 (설정 상수 상단에 있음)
│  └─ DemoLauncher.csproj  self-contained 단일 exe 빌드 설정
├─ build.ps1               빌드 스크립트
├─ dist/Xsaya-Rsa.exe      빌드 산출물 (배포용)
└─ README.md               이 문서
```
