using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<CharacterManager>().FromComponentInHierarchy().AsSingle().NonLazy(); ;
        Container.Bind<WebSocketListener>().FromComponentInHierarchy().AsSingle().NonLazy(); ;
        Container.Bind<TTSQueue>().FromComponentInHierarchy().AsSingle().NonLazy(); ;
    }
}
