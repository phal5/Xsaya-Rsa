// 씬 템플릿에서 새 씬을 만들 때 에디터가 부르는 훅이다. UnityEditor.SceneTemplate은
// 에디터 전용 어셈블리라, 이 파일이 Editor 폴더 밖에 있으면 플레이어 빌드에도 그대로 컴파일되어
// UnityEditor.dll이 빠진 빌드에서 타입을 못 찾는다. 파일을 옮기는 대신 여기서 막는다.
#if UNITY_EDITOR
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlatformerSceneTemplatePipeline : ISceneTemplatePipeline
{
    public virtual bool IsValidTemplateForInstantiation(SceneTemplateAsset sceneTemplateAsset)
    {
        return true;
    }

    public virtual void BeforeTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, bool isAdditive, string sceneName)
    {

    }

    public virtual void AfterTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, Scene scene, bool isAdditive, string sceneName)
    {

    }
}
#endif
