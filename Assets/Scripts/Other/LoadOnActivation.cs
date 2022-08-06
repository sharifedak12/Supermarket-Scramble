using UnityEngine;
using UnityEngine.SceneManagement;
 
public class LoadOnActivation : MonoBehaviour
{
        public string scene;
	public Color loadToColor = Color.white;
    public Animator animator;
	
	public void GoFade()
    {
        Initiate.Fade(scene, loadToColor, 1.0f);
    }

    void OnEnable()
    {
        GoFade();
    }
    public void UpdatePosition()
    {
        animator.SetFloat("moveX", 1);
        animator.SetFloat("moveY", 0);
            }
}