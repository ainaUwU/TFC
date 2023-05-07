using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float typingSpeed = 0.04f;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;

    [SerializeField] private TextMeshProUGUI dialogueText; //using TMPro;

    [SerializeField] private GameObject continueIcon;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices; //array de elecciones

    private TextMeshProUGUI[] choicesText; //array de cada texto para cada elecci�n

    private Story currentStory; //using Ink.Runtime;

    public bool dialogueIsPlaying { get; private set; }//read only

    public bool canContinueToNextLine = false;

    private Coroutine displayTextCoroutine;

    private static DialogueManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one Dialogue Manager in the scene");
        }
        instance = this;
    }
    public static DialogueManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);

        //inicializa choicesText
        choicesText = new TextMeshProUGUI[choices.Length]; //iguala el array de choicesText con la cantidad del array de choices
        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++; //incrementa el indice despues de cada loop
        }
    }

    private void Update()
    {
        if (!dialogueIsPlaying)
        {
            return;
        }
        if (canContinueToNextLine && Input.GetMouseButtonDown(0))
        {
            ContinueStory();
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON)//coge el json con los dialogos
    {
        currentStory = new Story(inkJSON.text);//se inicializa con la info del json
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);

        ContinueStory();
    }

    void ExitDialogueMode()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = ""; // dejamos el texto en un string vacia por si acaso
    }

    void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            //set the text
            //dialogueText.text = currentStory.Continue();
            if (displayTextCoroutine != null) //si ya esta en marcha para la corrutina para que no se vuelva loco el dialogo
            {
                StopCoroutine(displayTextCoroutine);
            }
            displayTextCoroutine = StartCoroutine(DisplayText(currentStory.Continue()));//siguiente linea de dialogo
            //Si hay elecci�n el dialogo activo, display 
        }
        else
        {
            ExitDialogueMode();
        }
    }

    void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices; //Devuele una lista de choices si la hay

        if (currentChoices.Count > choices.Length) //checkea la cantidad de decisiones y da error si sobrepasa el array
        {
            Debug.LogError("More coices than UI can support. Number of choices given:" + currentChoices.Count);
        }

        int index = 0;
        //buscar e inicializar los gobj de choices en UI, para las lineas de dialogo activas
        foreach (Choice choice in currentChoices)
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text; //choiceText es el UI text y lo igualamos al texto de las decisiones
            index++;
        }
        //Revisa las elecciones que quedan por hacer en UI y desactivalas
        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }
    }

    IEnumerator DisplayText(string line)
    {
        //vacia el texto del dialogo
        dialogueText.text = "";
        canContinueToNextLine = false;
        continueIcon.SetActive(false);
        HideChoices();

        //escribe letra por letra
        foreach (char letter in line.ToCharArray())
        {
           if(Input.GetKey(KeyCode.Space))
            {
                dialogueText.text = line;
                break;//para romper el loop si el player no quiere esperar a que termine 
            }

            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        canContinueToNextLine = true;
        continueIcon.SetActive(true);
        DisplayChoices();
    }

    void HideChoices()
    {
        foreach (GameObject choiceButton in choices)
        {
            choiceButton.SetActive(false);
        }
    }

    private IEnumerator SelectFirtsChoise()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }

    public void MakeChoice(int choiceIndex)
    {
        if (canContinueToNextLine)
        {
            currentStory.ChooseChoiceIndex(choiceIndex);
            ContinueStory();
        }
    }
}