using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 주인공에게 책 한 권을 들이민다. 여러 권을 들고 있다가 그때그때 하나를 골라 편다.
///
/// <b>때리지 않는다.</b> 이 스킬의 값어치는 움직임을 끊는 것이다 — 책이 열리는 순간 조작이
/// UI로 넘어가므로, 보스는 그 사이에 자리를 잡거나 다음 것을 준비한다.
///
/// 그래서 준비도 발동도 없다. 읽히고 나서 걸리는 것은 이 스킬의 뜻이 아니다.
/// 대신 슈퍼아머도 없다 — 즉시 걸리는 것을 벌할 수 있어야 짝이 맞는다.
///
/// <b>책을 다 읽을 때까지 기다리지 않는다.</b> 후딜은 후딜대로 흐르고 책은 플레이어가 넘긴다.
/// 기다리게 만들면 보스가 대사 길이에 묶여, 긴 책 하나가 그 판의 흐름을 통째로 정한다.
/// </summary>
public class Boss_Recite : Boss_SkillBase
{
    [Header("Recite - 들이밀 책들")]
    [Tooltip("이 중 하나를 골라 편다. 페이지가 없는 책은 건너뛴다.")]
    [SerializeField] List<Book> _books = new List<Book>();

    /// <summary>
    /// 펼 책이 없으면 이 스킬은 성립하지 않는다.
    ///
    /// 사거리와 쿨다운만 보고 골랐다가 빈손으로 시전하는 일을 여기서 막는다 —
    /// 그 경우 아무 일도 일어나지 않은 채 후딜만 돌아, 보스가 이유 없이 굳는다.
    /// </summary>
    public override bool IsReady(BossManager boss)
    {
        return base.IsReady(boss) && Readable > 0;
    }

    /// <summary>펼 수 있는 책의 수. 비어 있는 책은 세지 않는다.</summary>
    int Readable
    {
        get
        {
            if (_books == null) return 0;

            int n = 0;
            foreach (Book book in _books) if (book != null && book.HasPages) n++;

            return n;
        }
    }

    /// <summary>
    /// <b>Enter에서 편다.</b> OnActivate가 아닌 것은 발동 구간이 0이라 한 프레임 늦기 때문이다 —
    /// 즉시 걸리는 것이 이 스킬의 전부인데, 그 한 프레임이 곧 빠져나갈 틈이 된다.
    /// </summary>
    protected override void OnWindup()
    {
        Book book = Pick();
        if (book == null) return;

        if (FlipBook.Instance == null)
        {
            Debug.LogWarning($"[{name}] FlipBook이 없어 책을 펴지 못했습니다. UI 씬이 올라와 있어야 합니다.", this);
            return;
        }

        // <b>되감고 편다.</b> Book은 몇 쪽까지 읽었는지를 스스로 들고 있고 SetBook은 그걸 건드리지 않는다.
        // 되감지 않으면 같은 책을 두 번째로 고른 순간 첫 장에서 이미 끝에 닿아,
        // 조작만 한 번 깜빡이고 닫힌다. -1인 이유는 SetBook이 곧바로 Next()로 한 장 넘기기 때문이다.
        book.GoToPage(-1);

        FlipBook.Instance.SetBook(book);
    }

    /// <summary>
    /// 페이지가 있는 것 중에서 고른다. 빈 책을 골라 열면 조작이 잠겼다 풀리기만 하고 아무것도 뜨지 않는다.
    ///
    /// 목록의 자리가 아니라 <b>펼 수 있는 것들</b> 중에서 뽑는다. 빈 책을 뽑아놓고 다시 뽑는 방식은
    /// 목록이 거의 다 비어 있을 때 몇 번을 돌지 알 수 없다.
    /// </summary>
    Book Pick()
    {
        int count = Readable;
        if (count == 0) return null;

        int index = Random.Range(0, count);

        foreach (Book book in _books)
        {
            if (book == null || !book.HasPages) continue;
            if (index-- == 0) return book;
        }

        return null;
    }
}
