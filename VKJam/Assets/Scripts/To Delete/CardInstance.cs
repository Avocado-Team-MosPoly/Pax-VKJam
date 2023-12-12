using UnityEngine;

public class CardInstance : MonoBehaviour
{
    public Cards CardSpawner;
    public GameObject Monster;

    public Animator animator;
    private static bool isFirst = true;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Choose()
    {
        if (isFirst)
        {
            animator.Play("card-rotate");
            CardSpawner.DisableInteract(gameObject);
            GameManager.Instance.SoundList.Play("Flip");
            isFirst = false;
        }
        else
        {
            Monster.SetActive(true);
            CardSpawner.ChooseIngredients();
        }
    }
}