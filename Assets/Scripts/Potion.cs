using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class Potion : MonoBehaviour
{
    public PotionType potionType;
    public int xIndex;
    public int yIndex;


    public bool isMatched;
    private UnityEngine.Vector2 CurrentPos;
    private UnityEngine.Vector2 targetPos;

    public bool isMoving;

    public Potion(int _x, int _y){

            xIndex = _x;
            yIndex =_y;
    }
     public void SetIndicies(int _x, int _y){

            xIndex = _x;
            yIndex =_y;
    }

    public void UpdateAppearance(GameObject newPrefab)
{
    // Replace the current potion's visual appearance with the new prefab
    GameObject newVisual = Instantiate(newPrefab, transform.position, UnityEngine.Quaternion.identity, transform.parent);

    // Destroy the old visual GameObject (but keep the Potion script)
    Destroy(this.gameObject);

    // Copy properties from the current potion to the new visual
    Potion newPotion = newVisual.GetComponent<Potion>();
    newPotion.SetIndicies(xIndex, yIndex);
    newPotion.potionType = this.potionType; // Maintain the potion type
}


}




public enum PotionType{
    Red,
    Blue,
    Green,
    Purple,
    Pink,
    Yellow,
/* 
    RedD,
    RedA,
    RedB,
    RedC,
    BlueD,
    BlueA,
    BlueB,
    BlueC,
    GreenA,
    GreenB,
    GreenC,
    GreenD,
    PurpleA,
    PurpleB,
    PurpleC,
    PurpleD,
    PinkA,
    PinkB,
    PinkC,
    PinkD,
    YellowA,
    YellowB,
    YellowC,
    YellowD */
}
