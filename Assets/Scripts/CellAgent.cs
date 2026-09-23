using UnityEngine;

public class CellAgent : MonoBehaviour
{
    public GameManager gameManager;

    // Caracteristicas de esta celula
    public int colorIndex;
    public int sizeIndex;

    private bool wasClicked = false;

    public void Setup(
        GameManager manager,
        int newColorIndex,
        int newSizeIndex,
        Color newColor,
        float newSize)
    {
        gameManager = manager;

        colorIndex = newColorIndex;
        sizeIndex = newSizeIndex;

        // Cambiar color
        GetComponent<SpriteRenderer>().color = newColor;

        // Cambiar tamaño
        transform.localScale =
            new Vector3(newSize, newSize, 1f);
    }

    private void OnMouseDown()
    {
        if (wasClicked)
            return;

        wasClicked = true;

        // Fue encontrada = fracaso de supervivencia
        gameManager.CellDestroyed(this);

        Destroy(gameObject);
    }
}