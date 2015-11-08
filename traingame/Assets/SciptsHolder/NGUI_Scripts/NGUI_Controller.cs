using UnityEngine;

/// <summary>
/// Control All the train game GUI graphics
/// Such as: Menu, Option,Power ups,...  
/// </summary>
public class NGUI_Controller : MonoBehaviour {

    GameController gameController; // reference to the GameController scipts
    // ###########################################################
	// Use this for initialization
	void Start () 
    {        
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
	}
    // ################### Button Methods #######################
    //----------
    // Start Button
    public void On_StartButtonClick()
    {
        gameController.SendMessage("startGame");
    }
    //----------
    // Option Button
    public void On_OptionButtonClick()
    {
        gameController.SendMessage("main_options");
    }
    //----------
    // Back Button
    public void On_BackButtonClick()
    {
        gameController.SendMessage("options_back");
    }
    //----------
    // ##################### Game Buttons ########################
    //----------
    // Red
    public void On_RedClick()
    {
        gameController.SendMessage("redPressed");
    }
    //----------
    // Green
    public void On_GreenClick()
    {
        gameController.SendMessage("greenPressed");
    }
    //----------
    // Blue
    public void On_BlueClick()
    {
        gameController.SendMessage("bluePressed");
    }
    //----------
    // ###########################################################
}
