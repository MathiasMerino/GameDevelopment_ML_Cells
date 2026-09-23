using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("CELULAS")]
    public GameObject cellPrefab;
    public int numberOfCells = 12;

    [Header("RONDA")]
    public float roundDuration = 10f;

    [Header("INTERFAZ")]
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public TMP_Text roundText;

    [Header("MACHINE LEARNING")]
    [Range(0f, 1f)]
    public float explorationRate = 0.35f;

    [Range(0f, 1f)]
    public float learningRate = 0.30f;


    private int score = 0;
    private int round = 0;

    private List<CellAgent> activeCells =
        new List<CellAgent>();


    // TAMAÑOS POSIBLES
    private float[] sizes =
    {
        0.45f,
        0.65f,
        0.85f
    };


    // COLORES POSIBLES
    private Color[] colors =
    {
        new Color(0.15f, 0.80f, 0.25f), // Verde
        new Color(0.10f, 0.45f, 0.85f), // Azul
        new Color(0.85f, 0.80f, 0.20f), // Amarillo
        new Color(0.85f, 0.20f, 0.20f), // Rojo
        new Color(0.55f, 0.25f, 0.75f), // Morado
        new Color(0.35f, 0.35f, 0.35f)  // Gris
    };


    // TABLA DE APRENDIZAJE
    private float[,] qValues;


    void Start()
    {
        qValues =
            new float[colors.Length, sizes.Length];

        // Valor inicial neutro
        for (int c = 0; c < colors.Length; c++)
        {
            for (int s = 0; s < sizes.Length; s++)
            {
                qValues[c, s] = 0.5f;
            }
        }

        scoreText.text = "Puntos: 0";

        StartCoroutine(GameLoop());
    }


    IEnumerator GameLoop()
    {
        while (true)
        {
            round++;

            roundText.text = "Ronda: " + round;
            

            CreateCells();

            float timeRemaining = roundDuration;
            


            while (timeRemaining > 0)
            {
                timeRemaining -=
                    Time.deltaTime;

                timerText.text =
                    "Tiempo: " +
                    Mathf.CeilToInt(timeRemaining);

                yield return null;
            }


            EndRound();

            ShowBestConfiguration();

            yield return
                new WaitForSeconds(1f);
        }
    }


    void CreateCells()
    {
        for (int i = 0;
             i < numberOfCells;
             i++)
        {
            // Elegir color y tamaño
            Vector2Int configuration =
                ChooseConfiguration();

            int colorID =
                configuration.x;

            int sizeID =
                configuration.y;


            float x =
                Random.Range(-7f, 7f);

            float y =
                Random.Range(-3.2f, 2.8f);


            Vector3 randomPosition =
                new Vector3(x, y, 0);


            GameObject newCellObject =
                Instantiate(
                    cellPrefab,
                    randomPosition,
                    Quaternion.identity
                );


            CellAgent newCell =
                newCellObject.GetComponent<CellAgent>();


            newCell.Setup(
                this,
                colorID,
                sizeID,
                colors[colorID],
                sizes[sizeID]
            );


            activeCells.Add(newCell);
        }
    }


    Vector2Int ChooseConfiguration()
    {
        // Primeras rondas:
        // experimentar bastante
        if (round <= 2 ||
            Random.value < explorationRate)
        {
            int randomColor =
                Random.Range(
                    0,
                    colors.Length
                );

            int randomSize =
                Random.Range(
                    0,
                    sizes.Length
                );

            return new Vector2Int(
                randomColor,
                randomSize
            );
        }


        // EXPLOTACION:
        // buscar mejor configuracion conocida

        float bestValue = -1f;

        List<Vector2Int> bestOptions =
            new List<Vector2Int>();


        for (int c = 0;
             c < colors.Length;
             c++)
        {
            for (int s = 0;
                 s < sizes.Length;
                 s++)
            {
                float value =
                    qValues[c, s];


                if (value >
                    bestValue + 0.001f)
                {
                    bestValue =
                        value;

                    bestOptions.Clear();

                    bestOptions.Add(
                        new Vector2Int(c, s)
                    );
                }

                else if (
                    Mathf.Abs(
                        value -
                        bestValue
                    ) < 0.001f)
                {
                    bestOptions.Add(
                        new Vector2Int(c, s)
                    );
                }
            }
        }


        return bestOptions[
            Random.Range(
                0,
                bestOptions.Count
            )
        ];
    }


    public void CellDestroyed(
        CellAgent cell)
    {
        // CELULA ENCONTRADA
        // recompensa = 0

        Learn(
            cell.colorIndex,
            cell.sizeIndex,
            0f
        );


        score++;

        scoreText.text =
            "Puntos: " + score;


        activeCells.Remove(cell);


        Debug.Log(
            "Celula eliminada | Color: "
            + cell.colorIndex
            + " Tamaño: "
            + cell.sizeIndex
        );
    }


    void EndRound()
    {
        // Las que quedan vivas
        // reciben recompensa = 1

        foreach (
            CellAgent cell
            in activeCells)
        {
            if (cell != null)
            {
                Learn(
                    cell.colorIndex,
                    cell.sizeIndex,
                    1f
                );

                Destroy(
                    cell.gameObject
                );
            }
        }


        Debug.Log(
            "Fin ronda "
            + round
            + " | Supervivientes: "
            + activeCells.Count
        );


        activeCells.Clear();
    }


    void Learn(
        int colorID,
        int sizeID,
        float reward)
    {
        float oldValue =
            qValues[
                colorID,
                sizeID
            ];


        float newValue =
            oldValue
            +
            learningRate
            *
            (
                reward
                -
                oldValue
            );


        qValues[
            colorID,
            sizeID
        ] =
            newValue;
    }


    void ShowBestConfiguration()
    {
        float bestValue = -1f;

        int bestColor = 0;
        int bestSize = 0;


        for (int c = 0;
             c < colors.Length;
             c++)
        {
            for (int s = 0;
                 s < sizes.Length;
                 s++)
            {
                if (
                    qValues[c, s]
                    >
                    bestValue)
                {
                    bestValue =
                        qValues[c, s];

                    bestColor = c;
                    bestSize = s;
                }
            }
        }


        Debug.Log("ML -> Mejor configuración: " + "Color " + bestColor + " | Tamaño " + bestSize + " | Q = " + bestValue.ToString("0.00"));
       
    }
}