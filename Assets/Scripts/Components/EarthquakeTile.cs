using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EarthquakeTile : MonoBehaviour
{
    private Tile m_Tile;

    [SerializeField]
    private Image m_Image;

    [SerializeField]
    private Sprite m_DamagedTile;
    [SerializeField]
    private Sprite m_BrokenTile;

    public void InitWithState(Vector2Int index, bool isBroken, float TimeBeforeDestroy)
    {
        m_Tile = TileController.Info(index);

        m_Image.sprite = isBroken ? m_BrokenTile : m_DamagedTile;

        if(isBroken)
        {
            m_Tile.walkable = false;
        }
        else
        {
            m_Tile.damaged = true;
        }

        transform.parent = m_Tile.transform;

        ConformSize();

        Destroy(gameObject, TimeBeforeDestroy);
    }

    private void ConformSize()
    {
        Vector2 size= m_Tile.GetSize();
        GetComponent<RectTransform>().sizeDelta = size;
    }

    private void OnDestroy()
    {
        m_Tile.walkable = true;
        m_Tile.damaged = false;
    }
}
