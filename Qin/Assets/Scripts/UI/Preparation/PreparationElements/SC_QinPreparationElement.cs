using static SC_Global;

public class SC_QinPreparationElement : SC_PreparationElement {

    public EQinPreparationElement elementType;

    public override int ElementType { get { return (int)elementType; } }

}
