using static SC_Global;

public class SC_QinPreparationSlot : SC_PreparationSlot {

    public EQinPreparationElement elementType;

    public override int ElementType { get { return (int) elementType; } }

}
