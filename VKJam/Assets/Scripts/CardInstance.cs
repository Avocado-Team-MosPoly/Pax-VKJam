using UnityEngine;

public class CardInstance : MonoBehaviour
{
    public Card CardSpawner;

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
            isFirst = false;
        }
        else
            CardSpawner.ChooseIngredients();
    }
}