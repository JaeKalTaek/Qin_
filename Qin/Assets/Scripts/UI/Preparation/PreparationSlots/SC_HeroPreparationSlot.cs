using static SC_Global;

public class SC_HeroPreparationSlot : SC_PreparationSlot {

    public EHeroPreparationElement elementType;

    public override int ElementType { get { return (int) elementType; } }

}
