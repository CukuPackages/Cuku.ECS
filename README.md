# ECS
ECS Extensions.

# Dependencies
[Newtonsoft Json](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@latest)

[UniTask](https://github.com/Cysharp/UniTask?path=src/UniTask/Assets/Plugins/UniTask)

[Assets](https://github.com/CukuHub/Assets.git)

[Entitas](https://github.com/sschmid/Entitas)

# Instructions
> [!IMPORTANT]
> Contexts must be initialized before opening ECS - Entities Editor.
> 
> To do this create an Editor Script that calls on Editor start the partial class ContextInitialization which initializes the contexts (create on project level).
