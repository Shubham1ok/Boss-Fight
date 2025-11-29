public enum NodeState { Success, Failure, Running }

public abstract class BTNode
{
    public string name = "Node";
    public virtual void OnStart() { }
    public virtual void OnStop() { }
    public abstract NodeState Tick();
}
