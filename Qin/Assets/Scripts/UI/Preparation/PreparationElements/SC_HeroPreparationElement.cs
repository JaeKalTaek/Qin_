using static SC_Global;

public class SC_HeroPreparationElement : SC_PreparationElement {

    public EHeroPreparationElement elementType;

    public override int ElementType { get { return (int)elementType; } }

}
