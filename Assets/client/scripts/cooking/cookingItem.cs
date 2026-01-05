using UnityEngine;
using UnityEngine.UI;

public class cookingItem : MonoBehaviour
{
    public Item data;
    private Image image;

    [HideInInspector] public IngredientState state = IngredientState.Raw;
    [HideInInspector] public float currentTimer;

    private ingredient ingredientData;

    // ✅ AUDIO
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sizzleSound;
    public AudioClip popSound;
    

    private void Awake()
    {
        image = GetComponent<Image>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>(); // ✅ auto-find

        SetItem(data);
    }

    private void OnValidate()
    {
        if (image == null)
            image = GetComponent<Image>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (data != null)
            SetItem(data);
    }

    private void Update()
    {
        currentTimer += Time.deltaTime;
        if (state != IngredientState.Cooking )
            return;

        if (ingredientData == null)
            return;

        

        // ✅ COOKING → COOKED (YELLOW)
        if (state == IngredientState.Cooking && currentTimer >= ingredientData.cookingTime)
        {
            SetState(IngredientState.Cooked);
            currentTimer = 0f;

            // ✅ COOKED SOUND
            if (audioSource && sizzleSound)
                audioSource.PlayOneShot(sizzleSound);
        }
        // ✅ COOKED → BURNED (RED)
        else if (state == IngredientState.Cooked && currentTimer >= ingredientData.burnTime)
        {
            SetState(IngredientState.Burned);
        }
    }

    public void SetItem(Item newItem)
    {
        data = newItem;

        if (data != null)
            image.sprite = data.icon;
        else
            image.sprite = null;

        ingredientData = data as ingredient;

        currentTimer = 0f;
        SetState(IngredientState.Raw);
    }

    // ✅ CALLED BY STOVE PANEL
    public void StartCooking()
    {
        if (ingredientData == null)
            return;

        currentTimer = 0f;
        SetState(IngredientState.Cooking);

        // ✅ SIZZLE SOUND
        if (audioSource && sizzleSound)
            audioSource.PlayOneShot(sizzleSound);
    }

    // ✅ CALLED WHEN DRAGGING AGAIN
    public void StopCooking()
    {
        currentTimer = 0f;
        SetState(IngredientState.Raw);
    }

    // ✅ CALLED WHEN PLAYER STARTS DRAGGING (POP SOUND)
    public void PlayPop()
    {
        if (audioSource && popSound)
            audioSource.PlayOneShot(popSound);
    }

    // ✅ ALL COLOR LOGIC LIVES HERE
    private void SetState(IngredientState newState)
    {
        state = newState;

        switch (state)
        {
            case IngredientState.Raw:
                image.color = Color.white;
                break;

            case IngredientState.Cooking:
                image.color = new Color(1f, 0.5f, 0f); // orange
                break;

            case IngredientState.Cooked:
                image.color = Color.yellow; // ✅ yellow
                break;

            case IngredientState.Burned:
                image.color = Color.red;
                break;
        }
    }
}
