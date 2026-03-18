using UnityEngine;

public abstract class SlideSubState : MonoBehaviour
{
    protected SlideState parent;

    public void SetParent(SlideState slide)
    {
        parent = slide;
    }

    public virtual void Enter()
    {
        gameObject.SetActive(true);
    }

    public virtual void Exit()
    {
        gameObject.SetActive(false);
    }
}
