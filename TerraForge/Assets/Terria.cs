using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terria : MonoBehaviour
{
    [SerializeField] private int ArmorCost;
  private void OnTriggerEnter2D(Collider2D col)
  {
    if (col.GetComponent<AttributeUnit>())
    {
        col.GetComponent<AttributeUnit>().Armor += ArmorCost;
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
      if (other.GetComponent<AttributeUnit>())
      {
          other.GetComponent<AttributeUnit>().Armor -= ArmorCost;
      }
  }
}
