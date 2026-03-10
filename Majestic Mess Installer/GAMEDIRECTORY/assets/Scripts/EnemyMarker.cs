using Engine;

public class EnemyMarker : Entity
{
    public override void OnInit() { EnemyRegistry.Register(this); }
    public override void OnExit() { EnemyRegistry.Unregister(this); }
}
