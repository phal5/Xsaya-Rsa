using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 직렬화해 둔 타입 이름을 실제 <see cref="Type"/>으로 되돌린다.
///
/// MonoScript 참조는 에디터 전용이라 빌드에 남지 않는다. 그래서 FSM의 초기 상태나 스킬 조건처럼
/// "어떤 클래스인지"를 빌드까지 들고 가야 하는 곳은 <see cref="Type.AssemblyQualifiedName"/>을
/// 문자열로 저장해 둔다. 문제는 그 문자열이 <b>네임스페이스와 어셈블리 이름을 통째로 포함</b>한다는 것이다.
/// 클래스를 네임스페이스에 넣거나 asmdef를 하나 추가하는 것만으로 저장된 문자열은 무효가 되는데,
/// <see cref="Type.GetType(string)"/>은 그때 예외 대신 <b>null을 조용히 돌려준다.</b>
/// 컴파일은 멀쩡히 통과하고, 프리팹은 실행해 봐야 상태를 잃은 것이 드러난다.
///
/// 그래서 저장된 이름으로 먼저 찾고, 실패하면 <b>짧은 이름</b>으로 로드된 어셈블리를 뒤진다.
/// 후보가 여럿이면 아무것도 고르지 않는다 — 엉뚱한 타입을 붙이는 쪽이 못 찾는 쪽보다 나쁘다.
/// </summary>
public static class SerializedType
{
    /// <summary>찾은 결과. 못 찾은 것(null)도 담아 두어 어셈블리를 두 번 뒤지지 않는다.</summary>
    static readonly Dictionary<string, Type> _cache = new Dictionary<string, Type>();

    /// <summary>
    /// 저장된 이름에 해당하는 구체 타입을 돌려준다. 못 찾으면 null.
    /// </summary>
    /// <param name="stored">저장해 둔 이름. 보통 <see cref="Type.AssemblyQualifiedName"/>.</param>
    /// <param name="required">이것을 구현·상속한 타입만 인정한다. 짧은 이름이 겹칠 때 후보를 가르는 기준이기도 하다.</param>
    /// <param name="context">로그를 클릭했을 때 선택될 오브젝트.</param>
    public static Type Resolve(string stored, Type required, UnityEngine.Object context = null)
    {
        if (string.IsNullOrEmpty(stored)) return null;

        string key = required == null ? stored : stored + " -> " + required.FullName;
        if (_cache.TryGetValue(key, out Type cached)) return cached;

        Type found = Type.GetType(stored);

        // 이름이 바뀐 뒤다. 짧은 이름으로 다시 찾아본다.
        if (found == null) found = SearchByShortName(stored, required, context);

        if (found != null && required != null && !required.IsAssignableFrom(found))
        {
            Debug.LogError($"{Where(context)}{found.FullName}은(는) {required.Name}을(를) 구현하지 않습니다.", context);
            found = null;
        }

        _cache[key] = found;
        return found;
    }

    /// <summary>
    /// 네임스페이스·어셈블리를 뺀 클래스 이름만으로 찾는다.
    /// </summary>
    static Type SearchByShortName(string stored, Type required, UnityEngine.Object context)
    {
        // "Ns.Class, Assembly-CSharp, Version=..." 에서 앞의 타입 이름만 떼고, 다시 마지막 점 뒤만 남긴다.
        string full = stored.Split(',')[0].Trim();
        int dot = full.LastIndexOf('.');
        string shortName = dot < 0 ? full : full.Substring(dot + 1);

        List<Type> matches = new List<Type>();

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;

            // 의존 어셈블리가 빠져 있으면 타입 열거가 던진다. 그때도 읽힌 것만은 건네주므로 그것까지는 본다.
            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException broken) { types = broken.Types; }
            catch { continue; }

            foreach (Type type in types)
            {
                if (type == null) continue;                                  // 위에서 반쯤 읽힌 자리
                if (type.Name != shortName) continue;
                if (type.IsAbstract || type.IsInterface) continue;           // 만들 수 없는 것은 후보가 아니다
                if (required != null && !required.IsAssignableFrom(type)) continue;

                matches.Add(type);
            }
        }

        if (matches.Count == 1)
        {
            // 찾긴 했지만 저장된 값은 낡았다. 해당 프리팹·씬을 다시 저장해 두는 편이 좋다.
            Debug.LogWarning($"{Where(context)}저장된 타입 이름이 낡았습니다: \"{stored}\" -> {matches[0].FullName}. " +
                             $"짧은 이름으로 찾아 이었습니다.", context);
            return matches[0];
        }

        if (matches.Count == 0)
            Debug.LogError($"{Where(context)}타입을 찾을 수 없습니다: {stored}", context);
        else
            Debug.LogError($"{Where(context)}\"{shortName}\" 이름을 가진 타입이 여럿이라 고르지 않았습니다: " +
                           $"{string.Join(", ", matches.ConvertAll(t => t.FullName))}", context);

        return null;
    }

    static string Where(UnityEngine.Object context) => context == null ? string.Empty : $"[{context.name}] ";
}
