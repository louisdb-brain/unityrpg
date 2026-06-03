using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public GameObject sprite;
    public Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator=sprite.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void attack()
    {
        animator.SetBool("attack",true);
    }

    public void walk()
    {
        animator.SetBool("walking", true);
    }

    public void idle()
    {
        animator.SetBool("walking", false);
    }
}
