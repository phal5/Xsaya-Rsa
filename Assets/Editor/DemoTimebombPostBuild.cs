#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

// Windows 스탠드얼론 빌드가 끝나면 자동으로 실행되는 후처리.
//  1) DemoTimebomb 런처를 "빌드일 + 14일" 만료일로 컴파일(dotnet publish)해서
//  2) 유니티 산출물을 Game/ 하위로 옮기고, 런처를 빌드 최상단에 배치한다.
// 결과: 만료일이 지나 런처를 실행하면 폴더 내용물이 스스로 비워진다.
// (만료 전에는 Game/의 실제 게임을 대신 실행한다.)
//
// 런처 소스: <프로젝트 루트>/DemoTimebomb/src/DemoLauncher.csproj
// 만료일은 exe 안에 컴파일돼 들어가므로 텍스트로 열어 고칠 수 없다.
public static class DemoTimebombPostBuild
{
    // 만료까지 일수. 여기만 바꾸면 됩니다.
    const int ExpiryDays = 14;

    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.StandaloneWindows64 &&
            target != BuildTarget.StandaloneWindows)
            return;

        try
        {
            string buildExe = pathToBuiltProject;                 // ...\build\Xsaya-Rsa.exe
            string buildDir = Path.GetDirectoryName(buildExe);
            if (string.IsNullOrEmpty(buildDir) || !Directory.Exists(buildDir))
            {
                UnityEngine.Debug.LogWarning("[Timebomb] 빌드 폴더를 찾지 못해 건너뜁니다.");
                return;
            }

            string projRoot = Directory.GetParent(Application.dataPath).FullName;
            string launcherProj = Path.Combine(projRoot, "DemoTimebomb", "src", "DemoLauncher.csproj");
            if (!File.Exists(launcherProj))
            {
                UnityEngine.Debug.LogWarning(
                    "[Timebomb] 런처 프로젝트가 없어 자동 삭제를 설치하지 않았습니다: " + launcherProj);
                return;
            }

            // 만료일 = 오늘 + N일, 그 날 23:59:59
            DateTime expiry = DateTime.Today.AddDays(ExpiryDays + 1).AddSeconds(-1);
            string expiryStr = expiry.ToString("yyyy-MM-ddTHH:mm:ss");

            // 1) 런처를 만료일 박아서 임시 폴더로 publish
            string tmpOut = Path.Combine(Path.GetTempPath(), "tb_launcher_" + Guid.NewGuid().ToString("N"));
            string args = $"publish \"{launcherProj}\" -c Release -o \"{tmpOut}\" -p:ExpiryLocal={expiryStr}";
            if (!RunDotnet(args, projRoot, out string log))
            {
                UnityEngine.Debug.LogError(
                    "[Timebomb] 런처 빌드 실패 — 빌드는 보호되지 않은 상태로 남깁니다.\n" + log);
                return;
            }
            string builtLauncher = Path.Combine(tmpOut, "Xsaya-Rsa.exe");
            if (!File.Exists(builtLauncher))
            {
                UnityEngine.Debug.LogError("[Timebomb] 런처 exe가 생성되지 않았습니다: " + builtLauncher);
                return;
            }

            // 2) 유니티 산출물 전체를 Game/ 하위로 이동
            string gameDir = Path.Combine(buildDir, "Game");
            var entries = new List<string>(Directory.GetFileSystemEntries(buildDir));
            Directory.CreateDirectory(gameDir);
            foreach (string entry in entries)
            {
                string name = Path.GetFileName(entry);
                if (string.Equals(name, "Game", StringComparison.OrdinalIgnoreCase)) continue;

                string dest = Path.Combine(gameDir, name);
                if (Directory.Exists(entry))
                {
                    if (Directory.Exists(dest)) Directory.Delete(dest, true);
                    Directory.Move(entry, dest);
                }
                else
                {
                    if (File.Exists(dest)) File.Delete(dest);
                    File.Move(entry, dest);
                }
            }

            // 3) 런처를 빌드 최상단에 배치 (심사자가 실행하는 파일)
            File.Copy(builtLauncher, Path.Combine(buildDir, "Xsaya-Rsa.exe"), true);

            try { Directory.Delete(tmpOut, true); } catch { /* 청소 실패는 무시 */ }

            UnityEngine.Debug.Log(
                $"[Timebomb] 자동 삭제 설치 완료. 만료일: {expiryStr} (오늘 +{ExpiryDays}일)\n" +
                $"  실행 파일: {Path.Combine(buildDir, "Xsaya-Rsa.exe")}\n" +
                $"  실제 게임: {Path.Combine(gameDir, "Xsaya-Rsa.exe")}");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("[Timebomb] 후처리 중 오류: " + e);
        }
    }

    static bool RunDotnet(string args, string workingDir, out string log)
    {
        string dotnet = "dotnet";
        string pf = Environment.GetEnvironmentVariable("ProgramFiles");
        if (!string.IsNullOrEmpty(pf))
        {
            string cand = Path.Combine(pf, "dotnet", "dotnet.exe");
            if (File.Exists(cand)) dotnet = cand;
        }

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = dotnet,
            Arguments = args,
            WorkingDirectory = workingDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        try
        {
            using (var p = System.Diagnostics.Process.Start(psi))
            {
                string stdout = p.StandardOutput.ReadToEnd();
                string stderr = p.StandardError.ReadToEnd();
                p.WaitForExit(180000); // 최대 3분
                log = stdout + "\n" + stderr;
                return p.HasExited && p.ExitCode == 0;
            }
        }
        catch (Exception e)
        {
            log = "dotnet 실행 실패: " + e.Message;
            return false;
        }
    }
}
#endif
