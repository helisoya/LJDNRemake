using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents an animated health / SP bar in battle
/// </summary>
public class BattleBarFill : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private Image additive;
    [SerializeField] private float speed = 2.0f;
    private float currentTarget = 1f;
    private bool upward;
    public bool filling { get; private set; }

    /// <summary>
    /// Sets the value of the bar
    /// </summary>
    /// <param name="targetValue">The bar</param>
    /// <param name="immediate">True if the change should be immediate</param>
    public void SetValue(float targetValue, bool immediate)
    {
        if (immediate)
        {
            currentTarget = targetValue;
            filling = false;
            fill.fillAmount = currentTarget;
            additive.fillAmount = currentTarget;
            return;
        }

        filling = true;

        if (currentTarget > targetValue)
        {
            // downward
            upward = false;
            fill.fillAmount = targetValue;
        }
        else
        {
            // upward
            upward = true;
            additive.fillAmount = targetValue;
        }

        currentTarget = targetValue;
    }

    void Update()
    {
        if (filling)
        {
            if (upward)
            {
                fill.fillAmount = Mathf.Clamp(fill.fillAmount + speed * Time.deltaTime, 0f, currentTarget);
            }
            else
            {
                additive.fillAmount = Mathf.Clamp(additive.fillAmount - speed * Time.deltaTime, currentTarget, 1f);
            }

            if (fill.fillAmount == additive.fillAmount)
            {
                filling = false;
            }
        }
    }
}
