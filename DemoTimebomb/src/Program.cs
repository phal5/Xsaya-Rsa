using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace DemoLauncher
{
    // 데모 런처 겸 자동 정리 도구.
    //  - 만료일 전에 실행하면 하위 Game 폴더의 실제 게임을 실행한다.
    //  - 만료일이 지나 실행하면 자신이 놓인 폴더(데모 폴더) 전체를 삭제한다.
    // 관리자 권한 없이, 현재 사용자 권한만으로 동작한다.
    internal static class Program
    {
        // ════════════════════════════════════════════════════════════════
        //  설정 — 여기만 수정하면 됩니다
        // ════════════════════════════════════════════════════════════════

        // 만료 시각(PC 로컬 시간). 값은 빌드 시 컴파일러가 어셈블리에 심는다:
        //   dotnet publish -p:ExpiryLocal=2026-09-25T23:59:59
        // 유니티 빌드 후처리(Assets/Editor/DemoTimebombPostBuild.cs)가 매 빌드마다
        // "빌드일 + 14일"을 이 속성으로 넘겨 exe 안에 박아 넣는다. 손으로 고칠
        // 필요가 없다. 값을 못 읽으면 안전상 만료하지 않는다(먼 미래).
        static readonly DateTime Expiry = ResolveExpiry();

        static DateTime ResolveExpiry()
        {
            try
            {
                foreach (var a in Assembly.GetExecutingAssembly()
                             .GetCustomAttributes<AssemblyMetadataAttribute>())
                {
                    if (a.Key == "ExpiryLocal" &&
                        DateTime.TryParseExact(a.Value, "yyyy-MM-ddTHH:mm:ss",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    {
                        return DateTime.SpecifyKind(dt, DateTimeKind.Local);
                    }
                }
            }
            catch { /* 무시하고 안전한 기본값으로 */ }

            // 만료일을 확정할 수 없으면 절대 지우지 않는다.
            return new DateTime(2099, 1, 1, 0, 0, 0, DateTimeKind.Local);
        }

        // 런처와 같은 폴더 안에서, 실제 유니티 빌드가 들어있는 하위 폴더 이름
        const string GameSubdir = "Game";

        // 실행할 실제 게임 실행 파일 이름
        const string GameExe = "Xsaya-Rsa.exe";

        // 만료 시 심사자에게 보여줄 안내
        const string ExpiredTitle = "데모 평가 기간 종료";
        const string ExpiredMessage =
            "이 데모의 평가 기간이 종료되었습니다.\n\n" +
            "확인을 누르면 데모 파일이 자동으로 제거됩니다.";

        const string ErrorTitle = "실행 오류";

        // ════════════════════════════════════════════════════════════════

        [STAThread]
        static int Main()
        {
            // 원본(끝에 구분자가 붙은) 경로를 그대로 넘긴다. 미리 잘라내면
            // "C:\" 가 "C:" 가 되어 드라이브 현재 폴더로 오인되는 문제가 생긴다.
            string root = AppContext.BaseDirectory;

            // 만료 전 → 실제 게임 실행
            if (DateTime.Now <= Expiry)
                return LaunchGame(root);

            // 만료 후 → 안내 후 자기 폴더 삭제
            MessageBox(IntPtr.Zero, ExpiredMessage, ExpiredTitle, MB_OK | MB_ICONINFORMATION);

            if (TrySelfDestruct(root, out string reason))
                return 0;

            // 안전장치에 걸려 삭제하지 않은 경우(위험한 위치 등)
            MessageBox(IntPtr.Zero,
                "안전을 위해 자동 제거를 실행하지 않았습니다.\n사유: " + reason,
                ExpiredTitle, MB_OK | MB_ICONWARNING);
            return 1;
        }

        // ── 게임 실행 ────────────────────────────────────────────────────
        static int LaunchGame(string root)
        {
            string gameDir = Path.Combine(root, GameSubdir);
            string exe = Path.Combine(gameDir, GameExe);

            if (!File.Exists(exe))
            {
                MessageBox(IntPtr.Zero,
                    "게임 실행 파일을 찾을 수 없습니다.\n폴더 구성이 손상되었을 수 있습니다.\n\n" + exe,
                    ErrorTitle, MB_OK | MB_ICONERROR);
                return 2;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exe,
                    WorkingDirectory = gameDir,
                    UseShellExecute = false,
                });
                return 0;
            }
            catch (Exception ex)
            {
                MessageBox(IntPtr.Zero,
                    "게임을 실행하지 못했습니다.\n" + ex.Message,
                    ErrorTitle, MB_OK | MB_ICONERROR);
                return 3;
            }
        }

        // ── 자기 폴더 삭제 ───────────────────────────────────────────────
        static bool TrySelfDestruct(string root, out string reason)
        {
            if (!IsSafeTarget(root, out reason))
                return false;

            // 안전장치 통과 후, 정규화·구분자 제거한 절대 경로를 삭제 대상으로 쓴다.
            string target = TrimSep(Path.GetFullPath(root));

            // 실행 중인 런처 exe는 자기 자신을 지울 수 없다.
            // %TEMP% 에 정리용 배치를 만들어 분리 실행한 뒤 런처는 즉시 종료한다.
            // 배치는 런처가 종료될 때까지 재시도하며 폴더를 삭제하고, 마지막에
            // 자기 자신(배치)까지 삭제한다.
            //
            // 삭제 대상 경로는 배치 본문에 직접 쓰지 않고 환경변수(TB_TARGET)로
            // 넘긴다. cmd 는 배치 파일을 시스템 코드페이지(한글 윈도우=CP949)로
            // 읽으므로, 경로에 한글이 있으면 파일에 박아넣은 문자열이 깨진다.
            // 환경변수는 유니코드로 전달되어 한글 경로도 안전하다. 배치 본문은
            // 순수 ASCII 만 유지한다.
            string bat = Path.Combine(Path.GetTempPath(),
                "cleanup_" + Guid.NewGuid().ToString("N") + ".bat");

            // 대상 폴더 자체는 남기고, 그 안의 내용(하위 폴더 + 파일)만 모두 지운다.
            //  1) 하위 폴더들을 통째로 삭제  2) 최상위 파일들을 삭제
            //  3) 폴더가 비었으면 종료, 아니면(실행 중인 런처 exe 등이 잠겨 있으면)
            //     잠깐 대기 후 재시도. 런처가 종료되면 남은 exe 도 지워져 비워진다.
            string script =
                "@echo off\r\n" +
                "for /L %%i in (1,1,60) do (\r\n" +
                "  for /d %%D in (\"%TB_TARGET%\\*\") do rmdir /s /q \"%%D\" 2>nul\r\n" +
                "  del /f /q \"%TB_TARGET%\\*\" 2>nul\r\n" +
                "  dir /a /b \"%TB_TARGET%\" 2>nul | findstr \"^\" >nul || goto done\r\n" +
                "  ping -n 2 127.0.0.1 >nul\r\n" +
                ")\r\n" +
                ":done\r\n" +
                "(goto) 2>nul & del \"%~f0\"\r\n";

            File.WriteAllText(bat, script, new UTF8Encoding(false));

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c \"" + bat + "\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.GetTempPath(),
            };
            psi.EnvironmentVariables["TB_TARGET"] = target;
            Process.Start(psi);

            reason = "";
            return true;
        }

        // ── 삭제 안전장치 ────────────────────────────────────────────────
        // 런처가 실제로 놓여 있는 폴더만 지운다. 그 폴더가 시스템/사용자
        // 중요 위치이거나 그런 폴더의 상위(조상)이면 절대 지우지 않는다.
        static bool IsSafeTarget(string root, out string reason)
        {
            reason = "";

            if (string.IsNullOrWhiteSpace(root) || !Path.IsPathRooted(root))
            {
                reason = "경로가 올바르지 않습니다.";
                return false;
            }

            string full = TrimSep(Path.GetFullPath(root));
            string driveRoot = TrimSep(Path.GetPathRoot(full) ?? "");

            // 드라이브 루트(C:\ 등) 자체는 금지
            if (full.Equals(driveRoot, StringComparison.OrdinalIgnoreCase))
            {
                reason = "드라이브 루트는 삭제할 수 없습니다.";
                return false;
            }

            // 드라이브 루트 아래 최소 2단계 이상 깊이일 것 (예: C:\A\B)
            int depth = full.Substring(driveRoot.Length)
                            .Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)
                            .Length;
            if (depth < 2)
            {
                reason = "폴더 깊이가 너무 얕아 안전상 삭제하지 않습니다.";
                return false;
            }

            // 시스템/사용자 중요 폴더 목록
            var special = new[]
            {
                Environment.SpecialFolder.UserProfile,
                Environment.SpecialFolder.Windows,
                Environment.SpecialFolder.System,
                Environment.SpecialFolder.SystemX86,
                Environment.SpecialFolder.ProgramFiles,
                Environment.SpecialFolder.ProgramFilesX86,
                Environment.SpecialFolder.DesktopDirectory,
                Environment.SpecialFolder.MyDocuments,
                Environment.SpecialFolder.MyMusic,
                Environment.SpecialFolder.MyPictures,
                Environment.SpecialFolder.MyVideos,
            };

            foreach (var kind in special)
            {
                string s = Environment.GetFolderPath(kind);
                if (string.IsNullOrEmpty(s)) continue;
                string sf = TrimSep(s);

                // 대상이 중요 폴더 그 자체이면 금지
                if (full.Equals(sf, StringComparison.OrdinalIgnoreCase))
                {
                    reason = "중요 폴더(" + sf + ")는 삭제할 수 없습니다.";
                    return false;
                }

                // 대상이 중요 폴더의 상위(조상)이면 금지
                // (지우면 그 중요 폴더까지 함께 삭제되므로)
                if (sf.StartsWith(full + "\\", StringComparison.OrdinalIgnoreCase))
                {
                    reason = "중요 폴더의 상위 경로여서 삭제하지 않습니다.";
                    return false;
                }
            }

            return true;
        }

        static string TrimSep(string p) =>
            string.IsNullOrEmpty(p) ? p : p.TrimEnd('\\', '/');

        // ── user32 MessageBox (WinForms 의존성 없이 안내창 표시) ──────────
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        const uint MB_OK = 0x0;
        const uint MB_ICONERROR = 0x10;
        const uint MB_ICONWARNING = 0x30;
        const uint MB_ICONINFORMATION = 0x40;
    }
}
