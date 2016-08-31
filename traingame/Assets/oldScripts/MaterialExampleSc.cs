using UnityEngine;
using System.Collections;

public class MaterialExampleSc : MonoBehaviour {

    // I created a custom shader for the train game
    // all the materails using the same shaders
    // Therefore names for properties are the same.
    //all materials are located in Assets\3d_models\raw_3d_FBX\Materials
    //1. make a public field for Material
    public Material StationMat;
    public Material TrainMat;
    public Material MonsterMat;
    public Material OutsideMonsterMat;
    // Use this for initialization
    void Start () {

        // Example how to change the values
        // do the same for all other mats listed above

        // property name _diffColor /take a Color value 
        // change main texture color
        StationMat.SetColor("_diffColor" ,Color.blue);
        // change main emission texture color
        StationMat.SetColor("_emissColor", Color.blue);
        // change the power of emission  
        StationMat.SetFloat("_emissPower", 1.2f);

        // again since all the materials use the same shader 
        // use the same property names and values for all
        // you can find the shader in TrainShader/TrainDefaultShader

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
