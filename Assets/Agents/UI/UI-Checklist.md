# UI 작업 체크리스트

UI 씬·프리팹 작업 기록.
씬은 프리팹 인스턴스 한 개짜리 껍데기이므로, 실제 수정은 대부분 프리팹 쪽에서 일어난다.

**UI는 씬 2개 / 프리팹 2개다. 한쪽만 고치면 반드시 어긋난다.**

| 씬 | 쓰는 프리팹 | GUID |
|---|---|---|
| `Assets/Scenes/Parts/UI - Start.unity` | `Assets/Prefabs/UI/UI Canvas.prefab` | `905dc11e…` |
| `Assets/Scenes/Parts/UI - General.unity` | `Assets/Scenes/Parts/UI Canvas.prefab` | `cc78a5a6…` |

두 프리팹은 **variant가 아니라 독립 복사본**이다. 상속이 없으므로 한쪽 수정이 다른 쪽에
전파되지 않는다. 반면 스크립트는 공유하므로, 직렬화 필드를 바꾸면 **양쪽 다** 재배선해야 한다.
General 쪽에는 `Instructor`가 없고 Transform이 하나 더 많다(41 vs 40).

최종 갱신: 2026-08-23

---

## 완료

- [x] **Instructor의 죽은 UnityEvent 항목 제거** (2026-08-22)
  `OnScreenMessage`의 `Instructor._event`에 `m_Target`이 null인 `GameObject.SetActive` 항목이 4개 있었다.
  프리팹은 씬 오브젝트를 참조할 수 없어 저장 시 대상이 날아간 흔적. 런타임에 조용히 무시되던 것들이다.
  9개 → 5개 (Header / Esc / X / Z / Arrow Keys 의 `FadeIn`만 남음).

- [x] **`Main/Footer`의 고아 CanvasRenderer 제거** (2026-08-22)
  같은 오브젝트에 `Graphic`이 없어 아무것도 렌더하지 않던 잔재.

- [x] **씬의 무의미한 프리팹 오버라이드 제거** (2026-08-22)
  `Esc Menu`(fileID 4289678155353987793)의 `m_IsActive: 1` 오버라이드.
  프리팹 원본도 활성이라 값이 동일했다.
  당시 파일명은 `UI.unity`였고, 이후 `UI - Start.unity`로 이름이 바뀌었다.

  ※ 위 3건은 `905dc11e…`(Start) 프리팹에만 적용됐다. General 쪽 복사본은
    그 시점에 아직 없었거나 따로 만들어진 것이라 죽은 항목이 남아 있을 수 있다
    (실제로 `onSetBook`/`onDialogueNull`에 NULL 항목이 하나씩 있다 — 아래 할 일 참고).

- [x] **`OnScreenMessage`의 소유권을 `MessageUI`로 단일화 (SSOT)** (2026-08-23)
  표시 여부를 셋이 나눠 갖던 것을 `MessageUI` 하나로 모았다.
  - `MessageUI`가 `UIGroupFader _fader`(= `Central`)를 들고, `SetActive` 직접 호출을 없앴다.
    `SetText`가 빈 문자열이면 그대로 숨긴다.
  - `FlipBook.midScreen`: `TextMeshProUGUI` → `MessageUI` 참조. `Next()`는 `SetText`만 부른다.
  - `FlipBook.onSetBook`/`onDialogueNull`에서 `Central`의 FadeIn/FadeOut 배선 제거.
    (Footer/Flipbook 배선은 유지)
  - **프리팹 2개 모두** 재배선함. `midScreen`은 타입이 바뀌어 기존 참조가 끊기므로
    General 쪽도 손대지 않으면 첫 대사에서 NRE가 난다.
  - 검증: 양쪽 다 `Central` 페이더를 가리키는 UnityEvent 항목 0개,
    `_fader`/`midScreen` 모두 채워짐.

- [x] **`Instructor` 1회성 처리** (2026-08-23)
  `_event.Invoke()` 전에 `enabled = false`. `enabled`는 GameObject 활성 상태와 별개라
  누가 오브젝트를 되살려도 부활하지 않는다. `Set()`이 다시 `enabled = true`로 되살린다.

- [x] **General 프리팹의 끊긴 `Flipbook` 페이더 배선 복구** (2026-08-23)
  `Assets/Scenes/Parts/UI Canvas.prefab`의 `FlipBook`에서
  `onSetBook[1]`·`onDialogueNull[1]`의 대상이 NULL이었다 (복사 과정에서 유실).
  그대로 두면 General 씬에서 대사창이 페이드인되지 않는다.
  Start 프리팹의 같은 자리를 참조 삼아 `Flipbook` 페이더의 `FadeIn`/`FadeOut`으로 복구.
  mode=1(Void), callState=2(RuntimeOnly)까지 동일하게 맞췄다.

- [x] **`UIGroupFader`에 멱등 가드 추가** (2026-08-23)
  같은 상태로 다시 페이드하면 알파가 끝값으로 튕겼다가 움직여 깜빡였다.
  (`FadeOut`을 두 번 부르면 알파 1로 튀었다가 다시 0으로)
  `_visible`/`public bool Visible`을 두고 `FadeIn`/`FadeOut`이 상태가 바뀔 때만 돌게 했다.
  SSOT 이후 `SetText`가 매 페이지 표시 여부를 정하므로 이 가드가 없으면 페이지마다 깜빡인다.
  초기값이 `true`인 이유는 아래 "주의할 점"의 Footer 항목 참고.

---

## 할 일

- [ ] **Build Settings에 Parts 씬 등록**
  현재 `EditorBuildSettings.asset`에는 `SampleScene`(비활성)과
  `z_BP/[Obsoletized]Scenes/.../Elden Shrine 1.unity` 뿐이다.
  `Parts/UI.unity`·`Parts/Character.unity`·`Parts/Backgrounds/*`가 하나도 없어
  지금 빌드하면 UI가 아예 뜨지 않는다. 에디터 멀티씬 편집으로만 굴러가는 상태.
  → 씬 목록을 정리하면서 obsolete 항목도 같이 걷어낼 것.

- [ ] **`BookTrigger.cs` 삭제** — 손으로 지워야 한다
  `BookHolder` + `CollisionInvoke`로 대체되어 남은 역할이 없다.
  `AssetDatabase.DeleteAsset`이 확인 대화상자를 띄워 스크립트로는 못 지웠다.
  Project 창에서 `Assets/Scripts/UI/BookTrigger.cs`를 지우면 된다.
  → 지우면 `z_BP/[Obsoletized]Scenes/Elden Shrine 1/Recap/Demo/Elden Shrine 1.unity`에
    Missing Script가 뜬다. **그 씬이 지금 Build Settings에 enabled로 들어 있는
    유일한 씬이다.** 어차피 씬 목록을 갈아야 하니 아래 항목과 같이 처리하는 게 낫다.

- [ ] **대사 트리거를 실제로 배치해 검증** (아직 안 함)
  부품은 다 만들었지만 아직 어느 씬에도 놓지 않았다. 배치할 때:
  - `BookHolder`는 **콜라이더와 같은 GameObject**에 둘 것.
    `InteractionDetector`가 `OverlapSphere` → `TryGetComponent<IInteractable>`로 잡으므로
    `InteractableEvent`도 콜라이더와 같은 오브젝트여야 한다.
  - 탐지 반경은 2m, 검사 주기 0.15초 (`InteractionDetector` 기본값).
  - **`Book`의 페이지 인덱스는 리셋되지 않는다.** 한 번 끝까지 본 책을 다시 열면
    대사 없이 열자마자 닫힌다. 반복시키려면 `GoToPage(0)`이 필요하다.

- [ ] **플레이 모드에서 SSOT 변경 검증** (아직 안 함) — `UI - Start`, `UI - General` **양쪽**
  에디터 컴파일과 프리팹 배선까지만 확인했다. 실제로 돌려서 볼 것:
  1. 시작 시 "Press [ V ] to Raise"가 보이는가
  2. V를 누르면 문구가 사라지고 Header·키 가이드 4개가 페이드인되는가
  3. 대사를 열었다 닫은 뒤 V를 눌러도 **아무 일도 없어야 한다** (Instructor 부활 버그)
  4. 페이지를 넘길 때 중앙 문구가 깜빡이지 않는가 (멱등 가드)
  5. `MidScreenText`가 있는 페이지 ↔ 없는 페이지를 오갈 때 표시가 맞게 따라오는가

- [x] **`HPbar` NRE (2026-08-24 03:02 발생)** — 원인 규명 완료, **수정 안 함**

  `HPbar.cs:14`의 `PlayerManager.instance`가 **C# null**이었다.
  (`playerDamagable`가 null이면 15번 줄에서 났을 것이고, 파괴된 객체를 가리켰다면
   거기서는 예외가 안 난다. 즉 static이 한 번도 할당되지 않은 상태였다)

  **직접 원인: 재생 중 도메인 리로드.**
  Preferences의 `Script Changes While Playing`이 `Recompile And Continue Playing`(0),
  Auto Refresh도 켜져 있다. 재생 중에 스크립트가 저장되면 어셈블리가 다시 로드되는데,
  그때 유니티는 MonoBehaviour를 직렬화로 복원할 뿐 **`Awake`를 다시 부르지 않는다.**
  - static은 리로드로 초기화됨 → `PlayerManager.instance`가 null인 채로 남는다
  - 직렬화 안 되는 필드도 날아감 → 같은 순간 `SkillDefiner.evaluator`도 null이 되어
    같은 초에 NRE가 났다 (`InputQueue`의 이벤트 대상은 정상적인 씬 인스턴스로 확인됨)

  같은 3-씬 구성(`LastSceneManagerSetup.txt` 02:33 기록 = 현재와 동일)으로
  깨끗하게 재생하면 **재현되지 않는다.** frame 839까지 에러 0건, `instance` 정상,
  `TimeManager.Scale = 1`.

  **왜 하필 지금 터졌나:** `UI - General`의 Header 페이더는 `_fadedOnStart = false`라
  체력바가 **첫 프레임부터 Update를 돈다**. `UI - Start`는 `true`라 V를 누르기 전까지
  꺼져 있어서 이 창이 열리지 않았다.

  **결정: 수정하지 않는다** (2026-08-24, 회원님 판단).
  빌드에서는 재생 중 재컴파일이 일어나지 않으므로 이 경로 자체가 존재하지 않는다.
  에디터에서만 나는 유령 NRE에 코드를 바꾸지 않는다.
  다시 보이면 Preferences의 `Script Changes While Playing`을 의심할 것 —
  고칠 것은 코드가 아니라 그 설정이다.
  `PlayerManager.instance.playerDamagable`을 null 검사 없이 매 프레임 읽는다.
  UI 씬이 Character 씬보다 먼저 활성화되면 첫 프레임에 NRE.
  → 가드를 덧대기보다, 배속을 미는 주체를 UI 밖으로 옮기는 쪽이 맞다.
    (UI는 읽어서 그리기만 하고, 배속 산정은 체력을 가진 쪽이 소유)

- [x] **대사 트리거 배선 정비** (2026-08-23)
  충돌로도, E키 상호작용으로도 같은 대사를 열 수 있게 입구를 하나로 모았다.

  | 발동 | 배선 |
  |---|---|
  | 밟으면 자동 | `CollisionInvoke.onCollision` → `BookHolder.Open()` |
  | E키로 말 걸기 | `InteractableEvent.onInteract` → `BookHolder.Open()` |

  - **`BookHolder` 신규** (`Scripts/Interaction/`) — `Book`을 들고 `Open()` 하나만 노출.
    `FlipBook`은 UI 씬에 있어 인스펙터로 못 꽂으므로 `FlipBook.Instance`로 런타임에 찾는다.
    `Gateway`가 `Enter()` 하나로 두 길을 받는 것과 같은 모양.
  - **`CollisionInvoke`에 `OnTriggerEnter` 추가** — 통과 가능한 볼륨으로도 발동한다.
  - **`PlayerManager.IsPlayer(Collider)` 신규** — 근접 판정을 한 곳으로 모았다.
    `Gateway`만 `attachedRigidbody`까지 봤고 나머지는 정확 일치만 봐서 규칙이 갈려 있었다.
    `Gateway`는 자기 private 판정을 버리고 이걸 쓴다.
  - **`ControlLock` 신규** (`Scripts/UI/`) — `Lock()`/`Release()`가
    `PlayerManager.instance.Root`로 `CharacterRoot`를 간접 조회해 `ToUI()`/`ToControl()`.
    UI 프리팹 2개 모두 `Flipbook` 오브젝트에 붙이고 `onSetBook`→`Lock`,
    `onDialogueNull`→`Release`로 배선함.
  - **`PlayerManager.Root` 신규 + Character 씬에서 배선** — 매니저와 `CharacterRoot`가
    같은 씬(둘 다 프리팹 인스턴스)이라 오버라이드로 꽂힌다.

- [x] **`FlipBook.SetBook`의 호출 순서 교정** (2026-08-23)
  `Next()`가 `onSetBook`보다 먼저 돌고 있었다. 페이지가 없는 책에서는
  `Next()`가 `onDialogueNull`로 먼저 닫고 뒤이어 `onSetBook`이 다시 여는 꼴이 되어,
  `ControlLock`을 물린 뒤로는 **대사 없이 조작만 잠긴 채 남는 소프트락**이 된다.
  (다 본 책을 다시 밟으면 그 경로로 들어간다) `onSetBook`을 먼저 부르도록 뒤집었다.

---

## 내 담당 아님

- **`Time.timeScale`에 종속되지 않는 캐릭터** — 회원님이 직접 처리하기로 함 (2026-08-23).
  `HPbar.Update()`가 매 프레임 `TimeManager.SetScale()`을 호출해 체력이 게임 속도를 정하는 구조.
  이쪽을 건드리게 되면 `HPbar` 항목(위)과 겹치므로 먼저 확인할 것.

---

## 참고 — 손댈 때 주의할 점

- `Main/Footer`의 `UIGroupFader`는 **의도적으로 꺼져 있다.** `Update`/`Start` 기반이 아니라
  `FadeIn`/`FadeOut`을 UnityEvent로 직접 부르는 구조라, disabled 상태로도 정상 동작한다.
  `Awake`는 disabled여도 돌기 때문에 `_originalAlphas` 캐시는 정상적으로 잡힌다. 켜지 말 것.

- `Esc Menu`의 일시정지는 별도 배선이 없다. 페이더가 `Panel`을 SetActive 하는 것에
  `PauseGame.OnEnable/OnDisable`이 얹혀 자동으로 걸린다. `_uiElements` 목록을 건드리면 같이 깨진다.

- `FadeEngine`은 `Time.unscaledDeltaTime`을 쓴다. 일시정지 중에도 페이드가 도는 이유이자,
  ESC 메뉴가 멈춘 채로 부드럽게 사라질 수 있는 이유다.

- 프리팹을 스크립트로 고칠 때는 `PrefabUtility.LoadPrefabContents` →
  수정 → `SaveAsPrefabAsset` → `UnloadPrefabContents` 경로를 쓸 것.
  YAML 직접 편집은 에디터가 해당 에셋을 들고 있을 때 덮어써질 수 있다.
