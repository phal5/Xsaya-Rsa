using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Antigravity
{
    /// <summary>
    /// 에디터에서 지정한 씬 애셋들의 이름을 OnValidate()에서 추출하여 저장하고,
    /// 런타임/에디터 환경에서 씬 이름을 통해 순차적으로 Additive 방식으로 로드하는 컴포넌트입니다.
    /// </summary>
    public class AdditiveSceneLoader : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("Editor Scene Asset References")]
        [Tooltip("로드할 씬 애셋 목록 (에디터 전용)")]
        [SerializeField] private List<SceneAsset> sceneAssets = new List<SceneAsset>();
#endif

        [Header("Serialized Scene Names")]
        [Tooltip("OnValidate() 시 씬 애셋들로부터 자동 갱신되는 씬 이름 목록 (빌드 시에도 사용됨)")]
        [SerializeField] private List<string> sceneNames = new List<string>();

        [Header("Options")]
        [Tooltip("이미 로드되어 있는 씬의 중복 로드 방지 여부")]
        [SerializeField] private bool preventDuplicateLoading = true;

        [Header("Curtain")]
        [Tooltip("로딩 동안 화면을 덮을 막. 이 로더와 같은 씬에 있는 것을 꽂습니다. " +
                 "비워두면 TransitionCurtain.Current를 씁니다.")]
        [SerializeField] private TransitionCurtain curtain;

        [Tooltip("덮은 막에 띄울 지역 이름. 비우면 이름 없이 덮기만 합니다.")]
        [SerializeField] private string curtainRegionName = "";

        [Tooltip("로드가 끝나면 막을 걷습니다. 끄면 덮은 채로 남겨, 걷는 일은 받는 쪽에 맡깁니다.")]
        [SerializeField] private bool revealWhenDone = true;

        [Header("Unload")]
        [Tooltip("커튼 로드가 끝나면 이 로더가 놓인 씬(타이틀)을 내립니다. " +
                 "막을 걷기 전에 내리므로 타이틀 화면이 게임 위에 비치지 않습니다.")]
        [SerializeField] private bool unloadOwnSceneWhenDone = false;

        [Header("Opening Script")]
        [Tooltip("커튼을 덮기 <b>전에</b> 먼저 띄울 대사. 페이지가 없으면 건너뜁니다. " +
                 "FlipBook.Instance가 이 시점에 있어야 하므로, Transition Curtain과 마찬가지로 " +
                 "이 씬(타이틀)에 FlipBook을 따로 두어야 합니다 — 로드할 씬 목록의 UI 씬은 " +
                 "아직 올라오지 않아 그 안의 FlipBook을 쓸 수 없습니다.")]
        [SerializeField] private Book openingScript;

        /// <summary>
        /// 등록된 씬 이름 리스트 (읽기 전용)
        /// </summary>
        public IReadOnlyList<string> SceneNames => sceneNames;

        /// <summary>
        /// 커튼을 내리고 씬을 불러오는 중인지. 타이틀 버튼을 두 번 눌러도 두 번 돌지 않게 합니다.
        /// </summary>
        public bool Busy { get; private set; }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (sceneNames == null)
            {
                sceneNames = new List<string>();
            }

            sceneNames.Clear();

            if (sceneAssets != null)
            {
                for (int i = 0; i < sceneAssets.Count; i++)
                {
                    SceneAsset asset = sceneAssets[i];
                    if (asset != null)
                    {
                        sceneNames.Add(asset.name);
                    }
                }
            }
#endif
        }

        /// <summary>
        /// 리스트 순서대로 각 씬의 이름을 통해 씬들을 Additive(중첩) 모드로 동기 로드합니다.
        /// </summary>
        [ContextMenu("Load Scenes Additive (Sync)")]
        public void LoadScenesAdditive()
        {
            if (sceneNames == null || sceneNames.Count == 0)
            {
                Debug.LogWarning("[AdditiveSceneLoader] 로드할 씬 이름 목록이 비어있습니다.", this);
                return;
            }

            foreach (string sceneName in sceneNames)
            {
                if (string.IsNullOrWhiteSpace(sceneName))
                {
                    continue;
                }

                if (preventDuplicateLoading && IsSceneLoaded(sceneName))
                {
                    Debug.Log($"[AdditiveSceneLoader] 씬 '{sceneName}'은(는) 이미 로드되어 있어 건너뜁니다.", this);
                    continue;
                }

                Debug.Log($"[AdditiveSceneLoader] 씬 '{sceneName}' Additive 로드 시작...", this);
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            }
        }

        /// <summary>
        /// 리스트 순서대로 각 씬의 이름을 통해 씬들을 Additive(중첩) 모드로 비동기(Async) 로드합니다.
        /// </summary>
        [ContextMenu("Load Scenes Additive (Async)")]
        public void LoadScenesAdditiveAsync()
        {
            if (sceneNames == null || sceneNames.Count == 0)
            {
                Debug.LogWarning("[AdditiveSceneLoader] 로드할 씬 이름 목록이 비어있습니다.", this);
                return;
            }

            StartCoroutine(LoadScenesAdditiveRoutine());
        }

        private System.Collections.IEnumerator LoadScenesAdditiveRoutine()
        {
            foreach (string sceneName in sceneNames)
            {
                if (string.IsNullOrWhiteSpace(sceneName))
                {
                    continue;
                }

                if (preventDuplicateLoading && IsSceneLoaded(sceneName))
                {
                    Debug.Log($"[AdditiveSceneLoader] 씬 '{sceneName}'은(는) 이미 로드되어 있어 건너뜁니다.", this);
                    continue;
                }

                Debug.Log($"[AdditiveSceneLoader] 씬 '{sceneName}' 비동기 Additive 로드 시작...", this);
                AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                if (op != null)
                {
                    yield return op;
                }
            }
        }

        /// <summary>
        /// 화면을 검은 막으로 덮은 뒤 씬들을 비동기 Additive로 불러오고, 끝나면 막을 걷습니다.
        /// 타이틀 화면의 버튼에서 이 메서드를 부르면 됩니다.
        ///
        /// 덮을 막은 <b>로드가 시작되기 전에 이미 있어야</b> 합니다. 불러올 씬 안에 들어 있는 막은
        /// 그 씬이 올라온 뒤에야 생기므로 덮는 데 쓸 수 없습니다 — 타이틀 쪽에 하나 두고 꽂아야 합니다.
        /// </summary>
        [ContextMenu("Load Scenes Additive (Curtain)")]
        public void LoadScenesWithCurtain()
        {
            if (Busy)
            {
                Debug.Log("[AdditiveSceneLoader] 이미 불러오는 중이라 요청을 흘립니다.", this);
                return;
            }

            if (sceneNames == null || sceneNames.Count == 0)
            {
                Debug.LogWarning("[AdditiveSceneLoader] 로드할 씬 이름 목록이 비어있습니다.", this);
                return;
            }

            StartCoroutine(LoadScenesWithCurtainRoutine());
        }

        private System.Collections.IEnumerator LoadScenesWithCurtainRoutine()
        {
            Busy = true;

            // 커튼보다 먼저 돕니다. 다 봐야 다음(씬 로드)으로 넘어갑니다 — 겹쳐 나오거나
            // 대사가 끝나기 전에 씬이 로드되는 것을 막으려면 순서가 이래야 합니다.
            yield return PlayOpeningScriptRoutine();

            TransitionCurtain cover = ResolveCurtain();

            if (cover == null)
            {
                Debug.LogWarning("[AdditiveSceneLoader] 쓸 커튼이 없어 덮지 않고 불러옵니다. " +
                                 "타이틀 씬에 TransitionCurtain을 두고 Curtain 칸에 꽂으세요.", this);
            }
            else
            {
                yield return cover.Cover(curtainRegionName);
            }

            yield return LoadScenesAdditiveRoutine();

            if (cover != null)
            {
                yield return cover.Hold();
            }

            // 막을 걷기 <b>전에</b> 내립니다. 걷은 뒤에 내리면 그 사이 타이틀 화면이
            // 게임 위에 비칩니다 — 둘 다 Overlay에 sortingOrder가 0이라 누가 위인지도 정해지지 않습니다.
            bool survived = false;
            if (unloadOwnSceneWhenDone)
            {
                // 빼돌리기 전에 잡아둡니다. DontDestroyOnLoad를 거치고 나면
                // gameObject.scene은 더 이상 타이틀을 가리키지 않습니다.
                Scene own = gameObject.scene;

                survived = SurviveOwnSceneUnload(cover);
                yield return UnloadSceneRoutine(own);
            }

            // 덮은 그 막을 걷습니다. 여기서 TransitionCurtain.Current를 다시 묻지 않는 이유는,
            // 방금 올라온 UI 씬이 자기 막을 들고 오면서 Current를 가져가 버리기 때문입니다.
            // 그 막은 알파 0이라 걷어봐야 아무 일도 일어나지 않고, 화면은 덮인 채로 남습니다.
            if (revealWhenDone && cover != null)
            {
                yield return cover.Reveal();
            }

            Busy = false;

            // 살려둔 것들은 여기서 거둡니다. 막은 이미 걷혔고 로더는 할 일을 마쳤습니다.
            if (survived)
            {
                if (cover != null) Destroy(cover.transform.root.gameObject);
                Destroy(transform.root.gameObject);
            }
        }

        /// <summary>
        /// 지정한 오프닝 대사를 띄우고 끝날 때까지 기다립니다. 대사가 비어있으면 곧바로 지나갑니다.
        ///
        /// FlipBook이 이 씬에 없으면(로드할 목록의 UI 씬이 아직 안 올라왔으므로) 경고만 남기고
        /// 건너뜁니다 — 대사를 못 띄운다고 게임을 못 켜게 막을 일은 아닙니다.
        /// </summary>
        private System.Collections.IEnumerator PlayOpeningScriptRoutine()
        {
            if (openingScript == null || !openingScript.HasPages)
            {
                yield break;
            }

            if (FlipBook.Instance == null)
            {
                Debug.LogWarning("[AdditiveSceneLoader] FlipBook이 없어 오프닝 대사를 건너뜁니다. " +
                                 "이 씬(타이틀)에 FlipBook을 두세요.", this);
                yield break;
            }

            bool dialogueDone = false;
            void OnDialogueDone()
            {
                dialogueDone = true;
            }

            FlipBook.Instance.AddDialogueEndListener(OnDialogueDone);
            FlipBook.Instance.SetBook(openingScript);

            yield return new WaitUntil(() => dialogueDone);

            FlipBook.Instance.RemoveDialogueEndListener(OnDialogueDone);
        }

        /// <summary>
        /// 자기가 놓인 씬이 내려갈 때 이 로더와 커튼이 함께 죽지 않도록 빼돌립니다.
        ///
        /// 빼돌리지 않으면 씬을 내리는 순간 이 코루틴이 멎고 막도 함께 사라져,
        /// 검은 화면이 걷히는 대신 <b>툭 끊깁니다</b>.
        /// DontDestroyOnLoad는 루트 오브젝트에만 걸리므로 루트를 넘깁니다.
        /// </summary>
        private bool SurviveOwnSceneUnload(TransitionCurtain cover)
        {
            Scene own = gameObject.scene;

            DontDestroyOnLoad(transform.root.gameObject);

            // 막이 같은 씬에 있을 때만 함께 빼돌립니다. 다른 씬 것이면 건드릴 이유가 없습니다.
            if (cover != null && cover.gameObject.scene == own)
            {
                DontDestroyOnLoad(cover.transform.root.gameObject);
            }

            return true;
        }

        private System.Collections.IEnumerator UnloadSceneRoutine(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogWarning("[AdditiveSceneLoader] 내릴 씬이 이미 없습니다.", this);
                yield break;
            }

            if (SceneManager.sceneCount <= 1)
            {
                Debug.LogWarning("[AdditiveSceneLoader] 마지막 남은 씬은 내릴 수 없습니다.", this);
                yield break;
            }

            Debug.Log($"[AdditiveSceneLoader] 씬 '{scene.name}' 내리는 중...", this);

            AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
            if (op != null)
            {
                yield return op;
            }
        }

        /// <summary>
        /// 이 로더가 놓인 씬을 내립니다.
        ///
        /// <b>이 오브젝트도 함께 사라집니다.</b> 커튼 흐름 안에서 쓰려면 이 메서드가 아니라
        /// Unload Own Scene When Done 옵션을 쓰세요 — 그쪽은 막이 걷힐 때까지 살려둡니다.
        /// </summary>
        [ContextMenu("Unload Own Scene")]
        public void UnloadOwnScene()
        {
            UnloadScene(gameObject.scene.name);
        }

        /// <summary>
        /// 이름으로 지정한 씬을 내립니다. 이미 내려갔거나 없는 씬이면 조용히 지나갑니다.
        /// </summary>
        public void UnloadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("[AdditiveSceneLoader] 내릴 씬 이름이 비어있습니다.", this);
                return;
            }

            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.Log($"[AdditiveSceneLoader] 씬 '{sceneName}'은(는) 로드되어 있지 않아 건너뜁니다.", this);
                return;
            }

            if (SceneManager.sceneCount <= 1)
            {
                Debug.LogWarning("[AdditiveSceneLoader] 마지막 남은 씬은 내릴 수 없습니다.", this);
                return;
            }

            Debug.Log($"[AdditiveSceneLoader] 씬 '{sceneName}' 내리는 중...", this);
            SceneManager.UnloadSceneAsync(scene);
        }

        /// <summary>
        /// 쓸 막을 정합니다. 꽂아둔 것이 우선입니다 —
        /// Current는 씬이 올라오고 내려갈 때마다 주인이 바뀌므로 타이틀에서는 믿을 수 없습니다.
        /// </summary>
        private TransitionCurtain ResolveCurtain()
        {
            if (curtain != null)
            {
                return curtain;
            }

            return TransitionCurtain.Current;
        }

        /// <summary>
        /// 특정 씬이 현재 로드되어 있는지 확인합니다.
        /// </summary>
        private bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene.name.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
