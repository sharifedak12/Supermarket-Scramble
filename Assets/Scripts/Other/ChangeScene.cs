using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public string scene;
    public Color loadToColor = Color.white;
    public Animator animator;
    
	public void changeScene()
    {
        Initiate.Fade(scene, loadToColor, 1.0f);
    }

    public void changetoNextScene()
    {
        Initiate.Fade(GlobalVariables.nextLevel, loadToColor, 1.0f);
    }
    
    public void UpdatePosition()
    {
        animator.SetFloat("moveX", 1);
        animator.SetFloat("moveY", 0);
            }
}
