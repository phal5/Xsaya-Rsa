# UI 작업 체크리스트

`Assets/Scenes/Parts/UI.unity` + `Assets/Prefabs/UI/UI Canvas.prefab` 관련 작업 기록.
씬은 프리팹 인스턴스 한 개짜리 껍데기이므로, 실제 수정은 대부분 프리팹 쪽에서 일어난다.

최종 갱신: 2026-08-22

---

## 완료

- [x] **Instructor의 죽은 UnityEvent 항목 제거** (2026-08-22)
  `OnScreenMessage`의 `Instructor._event`에 `m_Target`이 null인 `GameObject.SetActive` 항목이 4개 있었다.
  프리팹은 씬 오브젝트를 참조할 수 없어 저장 시 대상이 날아간 흔적. 런타임에 조용히 무시되던 것들이다.
  9개 → 5개 (Header / Esc / X / Z / Arrow Keys 의 `FadeIn`만 남음).

- [x] **`Main/Footer`의 고아 CanvasRenderer 제거** (2026-08-22)
  같은 오브젝트에 `Graphic`이 없어 아무것도 렌더하지 않던 잔재.

- [x] **`UI.unity`의 무의미한 프리팹 오버라이드 제거** (2026-08-22)
  `Esc Menu`(fileID 4289678155353987793)의 `m_IsActive: 1` 오버라이드.
  프리팹 원본도 활성이라 값이 동일했다.

---

## 할 일

- [ ] **Build Settings에 Parts 씬 등록**
  현재 `EditorBuildSettings.asset`에는 `SampleScene`(비활성)과
  `z_BP/[Obsoletized]Scenes/.../Elden Shrine 1.unity` 뿐이다.
  `Parts/UI.unity`·`Parts/Character.unity`·`Parts/Backgrounds/*`가 하나도 없어
  지금 빌드하면 UI가 아예 뜨지 않는다. 에디터 멀티씬 편집으로만 굴러가는 상태.
  → 씬 목록을 정리하면서 obsolete 항목도 같이 걷어낼 것.

- [ ] **`Instructor`를 1회성으로 만들기** — 아래 SSOT 항목과 같은 뿌리
  `Instructor`는 `OnScreenMessage` 위에 `MessageUI`와 **같이** 얹혀 있다.
  발동 시 `_messageUI.SetVisibility(false)`가 자기 GameObject를 꺼버리므로
  평소에는 저절로 1회성처럼 보인다. 그런데 `FlipBook.onSetBook`이
  `Central`의 `FadeIn` → `SetActivity(true)`로 같은 GameObject를 되살리기 때문에,
  **대사가 한 번 열리고 나면 `Instructor`가 예전 `_key`/`_event`를 그대로 들고 부활한다.**
  그 뒤 V를 누르면 5개 페이더의 `FadeIn`이 다시 호출된다.
  → 남의 GameObject 활성 상태에 기대지 말고 `_event.Invoke()` 직후 `enabled = false`.
    (`Set()`이 다시 `enabled = true`로 되살린다)
  급하게 마무리하느라 남겨둔 항목.

- [ ] **`OnScreenMessage`의 소유권을 `MessageUI`로 단일화 (SSOT)**
  지금은 같은 GameObject의 표시 상태를 세 곳이 각자 건드린다:
  - `MessageUI.SetVisibility()` → `gameObject.SetActive()`
  - `Central`의 `UIGroupFader.SetActivity()` → 같은 오브젝트를 SetActive
  - `FlipBook.midScreen` → TMP를 직접 참조해 텍스트를 씀

  위의 `Instructor` 부활 버그가 바로 이 충돌이 겉으로 드러난 것이다.
  → `MessageUI`가 텍스트와 표시를 모두 소유하고, `FlipBook`·`Instructor`는 `MessageUI`만 부른다.
    `MessageUI`는 `SetActive` 대신 자기가 들고 있는 `UIGroupFader`로 표시를 넘긴다.
    (`FlipBook.midScreen`은 `MessageUI` 참조로 바꾸고, `onSetBook`/`onDialogueNull`의
     `Central` FadeIn/FadeOut 배선은 걷어낸다)
  설계 합의 후 착수.

- [ ] **`Time.timeScale`에 종속되지 않는 캐릭터 만들기**
  `HPbar.Update()`가 매 프레임 `TimeManager.SetScale()`을 호출해 체력이 게임 속도를 정한다.
  이 배속에서 예외인 액터(연출용 NPC, 컷신, 보스 특정 페이즈 등)를 어떻게 뺄지 미정.
  설계 논의중.

- [ ] **`HPbar`의 초기화 순서 정리**
  `PlayerManager.instance.playerDamagable`을 null 검사 없이 매 프레임 읽는다.
  UI 씬이 Character 씬보다 먼저 활성화되면 첫 프레임에 NRE.
  → 가드를 덧대기보다, 배속을 미는 주체를 UI 밖으로 옮기는 쪽이 맞다.
    (UI는 읽어서 그리기만 하고, 배속 산정은 체력을 가진 쪽이 소유)

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
