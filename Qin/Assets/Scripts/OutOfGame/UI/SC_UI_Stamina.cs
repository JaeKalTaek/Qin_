using UnityEngine;
using UnityEngine.UI;
using static SC_Character;
using static SC_Global;
using static SC_Hero;

public class SC_UI_Stamina : MonoBehaviour {

    public Text staminaCost;

    public Slider health, staminaUsage;

    public void SetStaminaCost (int cost) {

        if (cost > 0) {

            staminaCost.text = "-" + cost;

            health.Set(Mathf.Max(0, activeCharacter.Health - cost), activeCharacter.MaxHealth, ColorMode.Health);

            health.GetComponentInChildren<Text>().text = (activeCharacter.Health - cost) + " / " + activeCharacter.MaxHealth;

            staminaUsage.gameObject.SetActive(StaminaCost != EStaminaCost.TooHigh);

            staminaUsage.Set(activeCharacter.Health, activeCharacter.MaxHealth, ColorMode.Param, StaminaCost == EStaminaCost.Enough ? Color.yellow : Color.red);

            gameObject.SetActive(true);

        } else
            gameObject.SetActive(false);

    }

}
