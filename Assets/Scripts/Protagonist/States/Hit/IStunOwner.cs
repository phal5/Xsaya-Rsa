/// <summary>
/// 경직(<see cref="Character_Stun"/>)을 하위에 두는 머신. 경직이 끝나면 여기로 알린다.
///
/// 경직이 부모를 <see cref="Character_Hit"/>으로 가정하면, 대사 중 피격(<see cref="Character_UI"/>)처럼
/// 다른 자리에 두는 순간 끝나지 않는 경직이 된다. 끝난 뒤 어디로 갈지는 부모가 안다.
/// </summary>
public interface IStunOwner
{
    void Complete();
}
