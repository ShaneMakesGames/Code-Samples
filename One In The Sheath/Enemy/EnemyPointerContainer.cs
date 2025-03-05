using UnityEngine;
using UnityEngine.UI;


public class EnemyPointerContainer : MonoBehaviour
{
    public bool isFadingIn;
    public bool isFadingOut;

    public Image myImage;
    private float currentAngle;

    public Enemy myEnemy;

    public void PointerOnEnable(Enemy enemy)
    {
        myEnemy = enemy;
    }

    public void SetCurrentAngle(float newAngle)
    {
        currentAngle = newAngle;
    }

    public float GetCurrentAngle()
    {
        return currentAngle;
    }
}