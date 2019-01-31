using System.Collections.Generic;
using UnityEngine;

public class SC_Pump : SC_Construction {

    [Header("Pump Variables")]
    [Tooltip("Amount of Health drained to each hero in the range of that pump at the beginning of Qin's turn")]
    public int drainAmount;

    [Tooltip("Range in which the pump can drain energy")]
    public int range;

    [Tooltip("Heroes in range have their movement capability reduced by this amount")]
    public int slowAmount;

    delegate void Action (SC_Hero parameter);

    public static List<SC_Pump> pumps;

    void PerformAction(Action action) {

        foreach (SC_Hero hero in SC_Hero.heroes) {

            if (SC_Tile_Manager.TileDistance(transform.position, hero.transform.position) <= range)
                action(hero);

        }

    }

    protected override void Start () {

        base.Start();

        if (pumps == null)
            pumps = new List<SC_Pump>();

        pumps.Add(this);

        PerformAction ((SC_Hero hero) => {

            TrySlowHero(hero);

        });

    }

    public void Drain() {

        PerformAction((SC_Hero hero) => {

            SC_Qin.ChangeEnergy(Mathf.Min(hero.Health, drainAmount));

            hero.Hit(drainAmount);

        });

    }

    public override void DestroyConstruction () {

        gameObject.SetActive(false);

        PerformAction((SC_Hero hero) => {

            if (hero.PumpSlow == slowAmount) {

                hero.PumpSlow = 0;

                foreach (SC_Pump pump in pumps)
                    if(pump != this)
                        TrySlowHero(hero);

            }

        });

        pumps.Remove(this);

        base.DestroyConstruction();

    }

    public static void UpdateHeroSlow(SC_Hero hero) {

        hero.PumpSlow = 0;

        if (pumps != null) {

            foreach (SC_Pump pump in pumps) {

                if ((SC_Tile_Manager.TileDistance(hero.transform.position, pump.transform.position) <= pump.range) && (hero.PumpSlow < pump.slowAmount))
                    hero.PumpSlow = pump.slowAmount;

            }    

        }

    }

    void TrySlowHero(SC_Hero hero) {

        if (hero.PumpSlow < slowAmount)
           hero.PumpSlow = slowAmount;

    }

}
