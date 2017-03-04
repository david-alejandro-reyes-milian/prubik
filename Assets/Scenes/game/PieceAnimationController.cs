using UnityEngine;

public class PieceAnimationController : MonoBehaviour
{
    // Use this for initialization
    private Animation animation;

    public void Awake()
    {
        animation = gameObject.GetComponent<Animation>();
        print(animation);
    }

    public void PlayEnterAnimation()
    {
        animation.Play("PieceEnterAnimation");
    }
}