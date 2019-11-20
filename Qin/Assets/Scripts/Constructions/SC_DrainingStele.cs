using System.Collections.Generic;
using UnityEngine;

public class SC_DrainingStele : SC_Construction {

    [Header("Draining Stele Variables")]
    [Tooltip("Amount of Health drained to each hero in the range of that draining stele at the beginning of Qin's turn")]
    public int drainAmount;

    [Tooltip("Range in which the draining stele can drain energy")]
    public int range;

    [Tooltip("Heroes in range have their movement capability reduced by this amount")]
    public int slowAmount;

    delegate void Action (SC_Hero parameter);

    public static List<SC_DrainingStele> drainingSteles;

    void PerformAction(Action action) {

        foreach (SC_Hero hero in new List<SC_Hero> (SC_Hero.heroes))
            if (SC_Tile_Manager.TileDistance(transform.position, hero.transform.position) <= range)
                action(hero);

    }

    protected override void Start () {

        base.Start();

        if (drainingSteles == null)
            drainingSteles = new List<SC_DrainingStele>();

        drainingSteles.Add(this);

        SlowHeroesInRange();

        transform.parent = uiManager.drainingStelesT;

    }

    public void Drain() {

        PerformAction((SC_Hero hero) => {

            SC_Qin.ChangeEnergy(Mathf.Min(hero.Health, drainAmount));

            hero.Hit(drainAmount);

        });

    }

    public override void DestroyConstruction (bool playSound) {

        gameObject.SetActive(false);

        PerformAction((SC_Hero hero) => {

            if (hero.DrainingSteleSlow == slowAmount) {

                hero.DrainingSteleSlow = 0;

                foreach (SC_DrainingStele drainingStele in drainingSteles)
                    if(drainingStele != this)
                        drainingStele.SlowHeroesInRange();

            }

        });

        drainingSteles.Remove(this);

        base.DestroyConstruction(playSound);

    }

    public static void UpdateHeroSlow(SC_Hero hero) {

        hero.DrainingSteleSlow = 0;

        if (drainingSteles != null) {

            foreach (SC_DrainingStele drainingStele in drainingSteles) {

                if ((SC_Tile_Manager.TileDistance(hero.transform.position, drainingStele.transform.position) <= drainingStele.range) && (hero.DrainingSteleSlow < drainingStele.slowAmount))
                    hero.DrainingSteleSlow = drainingStele.slowAmount;

            }    

        }

    }

    void SlowHeroesInRange() {

        PerformAction((SC_Hero hero) => {

            TrySlowHero(hero);

        });

    }

    void TrySlowHero(SC_Hero hero) {

        if (hero.DrainingSteleSlow < slowAmount)
           hero.DrainingSteleSlow = slowAmount;

    }

}
