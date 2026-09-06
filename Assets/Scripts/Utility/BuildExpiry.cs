using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// 정해진 날이 지나면 스스로를 지우는 빌드. 쓰자니 매우 부끄럽지만... 졸업작품 설정 상 게임 세계의 붕괴가 현실의 빌드 삭제로 이어진다.
/// 이 기능을 활용해 빌드한 게 아니라면 스스로 삭제는 작동하지 않는다.
///
/// <b>바깥에 두는 검사기가 아니라 빌드 안에 심는다.</b> 실행 파일 옆에 배치 파일을 동봉하는 방식은
/// 그 배치를 눌렀을 때만 걸린다 — 플레이어가 exe를 직접 실행하면 아무 일도 일어나지 않고,
/// 파일 하나만 지우면 기한도 함께 사라진다. 여기서는 게임이 켜지는 길목
/// (<see cref="RuntimeInitializeLoadType.BeforeSplashScreen"/>) 자체를 잡으므로 우회할 자리가 없다.
///
/// <b>관리자 권한을 쓰지 않는다.</b> 예약 작업도, 서비스도, 레지스트리도 건드리지 않는다.
/// 하는 일은 제 폴더를 지우는 것뿐이고, 그건 그 폴더를 만든 사용자면 할 수 있다.
/// 대신 빌드를 Program Files 밑에 두면 그 폴더의 쓰기 권한이 없어 삭제가 조용히 실패한다 —
/// 사용자 폴더(바탕화면·문서·다운로드) 쪽에 두고 배포한다.
///
/// <b>시계를 되돌려도 소용없게 둔다.</b> 실행할 때마다 지금까지 본 가장 나중 시각을 남기고,
/// 다음에 켤 때 그 시각과 현재 시각 중 늦은 쪽을 쓴다. 기한이 지난 뒤 날짜를 되돌려도
/// 흔적이 이미 앞서 있으므로 되살아나지 않는다. 흔적은 빌드 폴더가 아니라
/// <see cref="Application.persistentDataPath"/>에 두어 폴더째 옮기거나 복사해도 따라온다.
///
/// <b>지우는 일은 자기가 못 한다.</b> 윈도우는 실행 중인 exe를 잠그기 때문에 프로세스가 살아 있는 동안
/// 제 폴더를 지울 수 없다. 그래서 임시 폴더에 작은 청소 스크립트를 하나 떨어뜨리고,
/// 그것이 이 프로세스가 완전히 내려가기를 기다렸다가 폴더를 지운 뒤 자기 자신도 지운다.
///
/// <b>날짜는 코드에 박는다.</b> 옆에 설정 파일로 두면 그 파일이 곧 해제 키가 된다.
/// 기한을 바꾸려면 <see cref="Deadline"/>을 고치고 다시 빌드한다.
/// </summary>
public static class BuildExpiry
{
    /// <summary>이 날 0시(현지 시각)를 넘기면 빌드는 더 이상 실행되지 않는다. yyyy-MM-dd.</summary>
    const string Deadline = "2026-09-30";

    /// <summary>안내를 띄워 두는 시간(초). 이 뒤에 프로세스가 내려가고 삭제가 시작된다.</summary>
    const float NoticeSeconds = 5f;

    const string NoticeText = "체험 기간이 끝났습니다.\n\n이 빌드는 곧 삭제됩니다.";

    /// <summary>지금까지 본 가장 나중 시각을 적어 두는 자리. 빌드 폴더 바깥이라 함께 지워지지 않는다.</summary>
    static string StampPath => Path.Combine(Application.persistentDataPath, ".rt");

    static DateTime Limit => DateTime.ParseExact(Deadline, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>
    /// 스플래시보다 먼저 불린다 — 기한이 지난 빌드는 첫 씬을 보기 전에 걸린다.
    /// 에디터에서는 통째로 컴파일되지 않는다. 플레이 모드에서 프로젝트가 지워지는 사고는 없다.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void Check()
    {
#if UNITY_EDITOR
        return;
#else
        DateTime now = Now();
        if (now < Limit)
        {
            Stamp(now);
            return;
        }

        GameObject notice = new GameObject("~") { hideFlags = HideFlags.HideAndDontSave };
        notice.AddComponent<Notice>();
#endif
    }

    /// <summary>현재 시각과 지난 흔적 중 늦은 쪽. 시계를 뒤로 돌린 경우 흔적이 이긴다.</summary>
    static DateTime Now()
    {
        DateTime now = DateTime.Now;
        try
        {
            if (File.Exists(StampPath) &&
                long.TryParse(File.ReadAllText(StampPath), NumberStyles.Integer, CultureInfo.InvariantCulture, out long ticks) &&
                ticks > 0L && ticks <= DateTime.MaxValue.Ticks)
            {
                DateTime seen = new DateTime(ticks);
                if (seen > now) return seen;
            }
        }
        catch { }   // 흔적을 읽지 못하면 현재 시각으로 본다. 못 읽는 것이 통과 사유가 되지는 않는다
        return now;
    }

    static void Stamp(DateTime now)
    {
        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(StampPath, now.Ticks.ToString(CultureInfo.InvariantCulture));
        }
        catch { }
    }

    /// <summary>
    /// 지울 폴더. 윈도우·리눅스는 <c>*_Data</c>를 품은 상위 폴더, 맥은 <c>.app</c> 번들 통째다.
    /// </summary>
    static DirectoryInfo Root()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.dataPath);

        if (Application.platform == RuntimePlatform.OSXPlayer)
        {
            while (dir != null && !dir.Name.EndsWith(".app", StringComparison.OrdinalIgnoreCase)) dir = dir.Parent;
            return dir;
        }

        return dir.Parent;
    }

    /// <summary>
    /// 지워도 되는 자리인지 본다. <b>여기서 막지 못하면 남의 폴더가 날아간다</b> —
    /// 하나라도 걸리면 아무것도 하지 않고 조용히 물러난다.
    /// </summary>
    static bool Safe(DirectoryInfo root)
    {
        if (root == null || !root.Exists) return false;
        if (root.Parent == null) return false;                                      // 드라이브 루트

        string path = root.FullName.TrimEnd(Path.DirectorySeparatorChar);

        // 프로젝트 폴더는 빌드가 아니다
        if (Directory.Exists(Path.Combine(path, "Assets")) &&
            Directory.Exists(Path.Combine(path, "ProjectSettings"))) return false;

        // 바탕화면·문서·사용자 폴더처럼 흔한 상위 폴더를 통째로 지우는 사고를 막는다
        foreach (Environment.SpecialFolder folder in new[]
        {
            Environment.SpecialFolder.Desktop,
            Environment.SpecialFolder.DesktopDirectory,
            Environment.SpecialFolder.MyDocuments,
            Environment.SpecialFolder.UserProfile,
            Environment.SpecialFolder.ProgramFiles,
            Environment.SpecialFolder.ProgramFilesX86,
            Environment.SpecialFolder.CommonApplicationData,
            Environment.SpecialFolder.ApplicationData,
            Environment.SpecialFolder.System,
            Environment.SpecialFolder.Windows,
        })
        {
            string special = Environment.GetFolderPath(folder);
            if (string.IsNullOrEmpty(special)) continue;
            if (string.Equals(Path.GetFullPath(special).TrimEnd(Path.DirectorySeparatorChar), path,
                              StringComparison.OrdinalIgnoreCase)) return false;
        }

        // 사용자 프로필 <b>바로 아래</b>는 거부한다.
        //
        // Downloads·Music·Videos처럼 Environment.SpecialFolder에 이름이 없는 셸 폴더가 그 자리에 있어,
        // 위의 열거로는 표현되지 않는다. 받은 압축을 "여기에 풀기"로 Downloads에 그대로 풀면
        // 그 폴더가 곧 빌드 폴더가 되고, 아래 검사(_Data + exe)를 통과해 폴더째 지워진다 —
        // 무관한 다운로드까지 함께 사라진다.
        // 
        // 누가 게임 폴더를 압축 해제 후 내용물을 굳이 다 꺼내 놓겠느냐마는... 혹시 모른다.
        //
        // 일단 임시방편이지만, 사용자 프로필 바로 아래에 둔 빌드는 스스로 지우지 못한다. 지우지 못하는 것이
        // 남의 것을 지우는 것보다 낫다 — 배포할 때는 전용 폴더에서 꺼내지 말 것을 강조하자. 기한이 지나면 지워지는 것도 표시해 두자.
        string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(profile) &&
            string.Equals(Path.GetFullPath(profile).TrimEnd(Path.DirectorySeparatorChar),
                          root.Parent.FullName.TrimEnd(Path.DirectorySeparatorChar),
                          StringComparison.OrdinalIgnoreCase)) return false;

        // 유니티 빌드의 모습을 하고 있어야 한다
        if (root.Name.EndsWith(".app", StringComparison.OrdinalIgnoreCase)) return true;
        if (root.GetDirectories("*_Data").Length == 0) return false;
        if (root.GetFiles("*.exe").Length == 0 && root.GetFiles("*.x86_64").Length == 0) return false;

        return true;
    }

    /// <summary>
    /// 이 프로세스가 내려가기를 기다렸다가 빌드 폴더를 지우는 청소기를 임시 폴더에 띄운다.
    /// 청소기는 마지막에 자기 자신도 지운다. 띄우는 것도 지우는 것도 사용자 권한으로 한다.
    /// </summary>
    static void Erase()
    {
        try
        {
            DirectoryInfo root = Root();
            if (!Safe(root)) return;

            int pid = Process.GetCurrentProcess().Id;
            string temp = Path.GetTempPath();
            string target = root.FullName.TrimEnd(Path.DirectorySeparatorChar);

            // 청소 스크립트 본문에는 경로를 적지 않는다 — 사용자 이름이 한글이면 배치 파일의
            // 코드 페이지에 걸려 깨진다. 인자로 넘기면 UTF-16 그대로 프로세스에 전달된다.
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                string bat = Path.Combine(temp, "cleanup_" + pid + ".bat");
                File.WriteAllText(bat, string.Join("\r\n", new[]
                {
                    "@echo off",
                    ":wait",
                    "ping -n 2 127.0.0.1 >nul",                                     // timeout 과 달리 콘솔 없이도 돈다
                    "tasklist /FI \"PID eq %~1\" /NH | find \"%~1\" >nul && goto wait",
                    "rmdir /s /q \"%~2\"",
                    "del /f /q \"%~f0\"",
                    "",
                }));                                                                // 본문은 전부 ASCII 라 인코딩을 가리지 않는다

                // call 을 앞에 두어야 cmd 가 따옴표로 시작하는 인자열을 제멋대로 벗기지 않는다
                Process.Start(new ProcessStartInfo("cmd.exe",
                    "/c call \"" + bat + "\" " + pid + " \"" + target + "\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = temp,                                        // 지울 폴더 안에서 돌면 그 폴더가 지워지지 않는다
                });
            }
            else
            {
                string sh = Path.Combine(temp, "cleanup_" + pid + ".sh");
                File.WriteAllText(sh, string.Join("\n", new[]
                {
                    "#!/bin/sh",
                    "while kill -0 " + pid + " 2>/dev/null; do sleep 1; done",
                    "rm -rf \"$1\"",
                    "rm -f \"$0\"",
                    "",
                }));

                Process.Start(new ProcessStartInfo("/bin/sh", "\"" + sh + "\" \"" + target + "\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = temp,
                });
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);   // 청소에 실패해도 아래의 종료는 그대로 간다
        }
    }

    /// <summary>
    /// 기한이 지났을 때만 붙는 안내막. 게임 UI를 빌리지 않는다 —
    /// 첫 씬이 올라오기 전에도 떠야 하고, 그 씬이 무엇이든 상관없어야 한다.
    /// </summary>
    class Notice : MonoBehaviour
    {
        float _left = NoticeSeconds;
        GUIStyle _style;

        void Awake()
        {
            Time.timeScale = 0f;        // 뒤에서 게임이 굴러가지 않게 한다
            AudioListener.pause = true;
        }

        void Update()
        {
            _left -= Time.unscaledDeltaTime;
            if (_left > 0f) return;

            enabled = false;            // 종료가 한 프레임 늦어도 두 번 부르지 않는다
            Erase();
            Application.Quit();
        }

        void OnGUI()
        {
            GUI.depth = int.MinValue;   // 무엇이 그려지든 그 위에 온다

            Rect full = new Rect(0f, 0f, Screen.width, Screen.height);
            GUI.color = Color.black;
            GUI.DrawTexture(full, Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.Max(16, Screen.height / 28),
                    wordWrap = true,
                };
                _style.normal.textColor = Color.white;
            }

            GUI.Label(full, NoticeText, _style);
        }
    }
}
